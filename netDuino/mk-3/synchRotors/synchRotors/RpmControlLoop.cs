using System;
using Microsoft.SPOT;
using System.Threading;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;

// Add feedforward.
//
//  Keep the rpm controller seprate so I can make sense of the gains.
//

namespace aluminiumWing
{
    public class RpmControlLoop
    {
        //
        //  User tuned variables.
        //
        public const double rpmMaxSpeed = 1450d; //1450.0d;                        // rpm set point
        private const int rpmGood = 20;                                             // Green led if closer than this, red otherwise
        private const int controlPeriod = 25;                                      // Control loop period in milliseconds
        private const int controlStart = 10;                                      // Delay before timer starts
        //
        //  Gains are from the last strap down experiment on 8 October 2014.
        //  The integral limit was not chacked, this may be a bit low.
        //  Play with these numbers to get good performance.
        //
        private const double feedForward = 0.1d;                                    // To reduce the integral variable
        private const double aOne = 1.0d;                                           // Always 1.
        private const double iGain = 0.1d * controlPeriod / 1000.0d;                 // Integral gain (B) times Period.
        private const double cOne = 1.0d;                                            // Always 1
        private const double propGain = 0.0336d;                                     // Proportional gain 
        //
        //  Non-control variables.
        //
        private const double rpmIntLimit = 60.0d;                                   // Integral limit. About 100 in the simulations.
        private const double rpmIntSlope = rpmIntLimit / 256.0d;
        private const double rpmIntLimitBot = 20.0d;
        private const int controlCutIn = 30;                                        // Speed at which the stick responds
        private const int controlRampEnd = 180;                                     // Setting for ramp end
        private const double rpmCutInSpeed = 300.0d;                                // rpm 
        //
        //  Synchronization variables.
        //
        public const double lockPeriod = 0.45;              // Lock period in terms of revolutions. Normalized to 1
        public const double rpmSynchGain = 20;              // Gain in terms of period
        public const double catchUpRPM = 50;                // rpm difference to drive into lock.
        //
        //  "Static" variables.
        //
        private static double rpmSet = 0;
        private static double rpmX = 0.0d;
        private static double s3Float = 0.0d;
        private const double rpmSlope = (double)(rpmMaxSpeed - rpmCutInSpeed) / (controlRampEnd - controlCutIn);
        private const double rpmConst = rpmCutInSpeed - rpmSlope * controlCutIn;
        private static double rpmError = 0.0d;

        private static double rpmLocal = 0;
        private static double rpmCMDLocal = 0;

        //
        //  Constructor
        //
        public RpmControlLoop()
        {
        }
        //
        //  Motor control timer.
        //
        private static Timer controlTimer = new Timer(delegate
        {
            //
            //  Unwind the integrater when the rotors are locked.
            //

            if (GVars.stopCMD || GVars.stopStart) rpmX = 0;

            lock (GVars.lockToken)
            {
                rpmCMDLocal = GVars.rpmCommand;
                rpmLocal = GVars.rpm;
            }
            //
            //  Only start controlling from a minimum value
            //
            if (rpmCMDLocal > controlCutIn)
            {
                lock (GVars.lockToken)
                {
                    GVars.rpmRequired = rpmSlope * rpmCMDLocal + rpmConst;
                    if (GVars.rpmRequired > rpmMaxSpeed) GVars.rpmRequired = rpmMaxSpeed;
#if rightSynch          // Make absolutely sure.
                    rpmSet = GVars.rpmRequired + GVars.addRPM;
#endif
#if leftSynch
                    rpmSet = GVars.rpmRequired;
#endif
                }
                rpmError = (rpmSet - rpmLocal);
                //
                //  Limit the integrator
                //
                if (rpmX > rpmIntLimit) rpmX = rpmIntLimit;
                if (rpmX < -rpmIntLimit) rpmX = -rpmIntLimit;
                //
                //  Set the integrator to zero for fresh starts.
                //
                if (rpmCMDLocal < 30.0d)
                {
                    rpmX = 0.0d;
                }
                //
                //  Control algorithm.
                //
                lock (GVars.lockToken)
                {
                    // Make feedforward proportional to rpm required, not rpmSet.
                    s3Float = cOne * rpmX + propGain * rpmError + feedForward * GVars.rpmRequired;
                    rpmX = aOne * rpmX + iGain * rpmError;
                }
                if (s3Float < 28) s3Float = 28;                     // Have to do more aOne this.
            }
            else
            //
            //  Feed the low cycle command through.
            //
            {
                s3Float = rpmCMDLocal;
                if (s3Float < 0) s3Float = 0;
            }
            if (s3Float > 255.0d) s3Float = 255.0d;
            if (!GVars.stopCMD && !GVars.stopStart) 
            GVars.motorDrive.Duration = (UInt32)(((int)s3Float) * GVars.powerScale * GVars.byte2Pulse + 1000.0d);
        },
 null,
 controlStart,
 controlPeriod);
        //
        //  Calculate the added rpm needed to synchronize.
        //

        public static void adjustRPM(long canTicks)
        {
            const double ticksPS = System.TimeSpan.TicksPerSecond;
            double lockTime = 0.0d;
            double timeDiff = 0.0d;
            double timeDiffRL = 0.0d;
            double timeDiffLL = 0.0d;
            double periodNow = 0.0d;
            double rpmLockScale = 0.0d;

            lock (GVars.lockToken)
                //
                //  The interrupt happens on the HAL interrupt on the right hand side
                //  POD. canTicks is the time stamp of a message that came from the 
                //  left hand pod. 
                //  Both timeDiffLL and timeDiffRL are asumed to be greater than zero.
                //  The smallest value is chosen for control.
            {
                periodNow = 1.0d / (GVars.rpm / 60.0d);
                rpmLockScale = GVars.rpmRequired / rpmMaxSpeed;
                lockTime = lockPeriod * periodNow;                              // Time in seconds where synch happens
                timeDiffRL = (double)(canTicks - GVars.halTimeNow) / ticksPS;   // left is leading
                timeDiffLL = (double)(GVars.halTimeNow - canTicks) / ticksPS + periodNow;   // right is leading

                if (timeDiffLL <= timeDiffRL)
                {
                    timeDiff = timeDiffLL;
                }
                else
                {
                    timeDiff = -timeDiffRL;
                }
                if (timeDiff < lockTime && timeDiff > -lockTime)
                {
                    GVars.addRPM = (int)(rpmSynchGain *
                        timeDiff / lockTime *               // Linear scaling with lock Time
                        rpmLockScale);                      // Washout with rpm 
                    GVars.red.Write(true);
                    Debug.Print(GVars.addRPM.ToString());
                }
                else
                //  Speed up the right side untile we are within the synch window.
                {
                    GVars.addRPM = catchUpRPM;
                    GVars.red.Write(false);
                    Debug.Print("No Synch");
                }
            }
        }
    }
}
