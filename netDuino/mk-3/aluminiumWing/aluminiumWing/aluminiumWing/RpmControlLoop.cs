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
        private const double rpmMaxSpeed = 1450d; //1450.0d;                        // rpm set point
        private const int rpmGood = 20;                                             // Green led if closer than this, red otherwise
        private const int controlPeriod = 25;                                      // Control loop period in milliseconds
        private const int controlStart = 10;                                      // Delay before timer starts
        //
        //  Gains are from the last strap down experiment on 8 October 2014.
        //  The integral limit was not chacked, this may be a bit low.
        //
        private const double feedForward = 0.1d;                                    // To reduce the integral variable
        private const double aOne = 1.0d;                                           // Always 1.
        private const double iGain = 0.1d * controlPeriod / 1000.0d;                 // Integral gain (B) times Period.
        private const double cOne = 1.0d;                                           // Always 1
        private const double propGain = 0.0336d;                                           // Proportional gain 
        //
        //  Non-control variables.
        //
        private const double rpmIntLimit = 20.0d;                               // Integral limit. About 100 in the simulations.
        private const double rpmIntSlope = rpmIntLimit / 256.0d;
        private const double rpmIntLimitBot = 20.0d;
        private const int controlCutIn = 30;                                        // Speed at which the stick responds
        private const int controlRampEnd = 180;                                     // Setting for ramp end
        private const double rpmCutInSpeed = 300.0d;                                // rpm                                 // Integral limit
        //
        //  "Static" variables.
        //
        private static double rpmX = 0.0d;
        private static double s3Float = 0.0d;
        private static double rpmRequired = 0.0d;
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
                rpmRequired = rpmSlope * rpmCMDLocal + rpmConst;
                if (rpmRequired > rpmMaxSpeed) rpmRequired = rpmMaxSpeed;

                rpmError = (rpmRequired - rpmLocal);
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
                    s3Float = cOne * rpmX + propGain * rpmError + feedForward * rpmRequired;
                    rpmX = aOne * rpmX + iGain * rpmError;
                }
                if (s3Float < 28) s3Float = 28;
            }
            else
            //
            //  Feed the low cycle command through.
            //
            {
                s3Float = rpmCMDLocal;
                if (s3Float < 0) s3Float = 0;
            }
            if (s3Float > 255.0d) s3Float = 255;
            GVars.motorDrive.Duration = (UInt32)(((int)s3Float) * GVars.powerScale * GVars.byte2Pulse + 1000.0d);
        },
 null,
 controlStart,
 controlPeriod);
    }
}
