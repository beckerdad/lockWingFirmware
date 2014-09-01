using System;
using Microsoft.SPOT;
using System.Threading;

namespace matlabInterface
{
    class rpmControlLoop
    {
        private const double rpmMaxSpeed = 1100d; //1450.0d;                        // rpm set point
        private const int rpmGood = 20;                                             // Green led if closer than this, red otherwise

        private const int controlPeriod = 25;                                      // Control loop period in milliseconds
        private const int controlStart = 100;                                      // Delay before timer starts

        private const double aOne = 1.0d;                                           // Always 1.
        private const double iGain = 0.01d * controlPeriod / 1000.0d;                 // Integral gain (B) times Period.
        private const double cOne = 1.0d;                                           // Always 1
        private const double propGain = 0.1d;                                           // Proportional gain 
        private const double rpmIntLimit = 200.0d;                               // Integral limit. About 100 in the simulations.
        private const double rpmIntSlope = rpmIntLimit / 256.0d;
        private const double rpmIntLimitBot = 20.0d;
        private const int controlCutIn = 30;                                        // Speed at which the stick responds
        private const int controlRampEnd = 180;                                     // Setting for ramp end
        private const double rpmCutInSpeed = 300.0d;                                // rpm                                 // Integral limit
        private static double rpmCNow = 0.0d;

        private static double rpmX = 0.0d;
        private static double s3Float = 0.0d;
        private static double rpmRequired = 0.0d;
        private const double rpmSlope = (double)(rpmMaxSpeed - rpmCutInSpeed) / (controlRampEnd - controlCutIn);
        private const double rpmConst = rpmCutInSpeed - rpmSlope * controlCutIn;
        private static double rpmError = 0.0d;

        private static double rpmLocal = 0;
        private static double rpmCMDLocal = 0;

        public rpmControlLoop()
        {
        }

        private static Timer controlTimer = new Timer(delegate
        {
            lock (GVars.lockToken)
            {
                rpmCMDLocal = GVars.rpmCommand;
                rpmLocal = GVars.rpm;
            }
            //            Debug.Print(s3Float.ToString() + "In control loop");
            //
            //  Only start controlling from a minimum value
            //

            if (rpmCMDLocal > controlCutIn)
            {
                rpmRequired = rpmSlope * rpmCMDLocal + rpmConst;
                if (rpmRequired > rpmMaxSpeed) rpmRequired = rpmMaxSpeed;

                //                Debug.Print(rpmRequired.ToString());

                rpmError = (rpmRequired - rpmLocal);

                //
                //  Indicate that the rpm is within 50 rpm.
                //

                //if (System.Math.Abs((int)rpmError) < rpmGood)
                //{
                //    GVars.green.Write(true);
                //    GVars.red.Write(false);
                //}
                //else
                //{
                //    GVars.green.Write(false);
                //    GVars.red.Write(true);
                //}

                //
                //  Limit the integrator
                //

                if (rpmX > rpmIntLimit) rpmX = rpmIntLimit;
                if (rpmX < -rpmIntLimit) rpmX = -rpmIntLimit;

                //
                //  Limit the integral gain at low speeds
                //
                if (rpmLocal < 800.0d)
                {
                    rpmCNow = rpmLocal / 800.0d * cOne;
                }
                else
                {
                    rpmCNow = cOne;
                }
                if (rpmLocal < 300.0d)
                {
                    rpmX = 0.0d;
                }

                //
                //  Control algorithm.
                //

                lock (GVars.lockToken)
                {
                    s3Float = rpmCNow * rpmX + GVars.propFix * propGain * rpmError;
                    rpmX = aOne * rpmX + GVars.iFix * iGain * rpmError;
                }
                if (s3Float < 28) s3Float = 28;
                //                Debug.Print(rpm.ToString() + " " + rpmRequired.ToString() + " " + s3Float.ToString() + " " + rpmX.ToString());

            }
            else
                //
                //  Feed the low cycle command through.
                //
            {
                s3Float = rpmCMDLocal;
                if (s3Float < 0) s3Float = 0;
            }

            if (s3Float > 255) s3Float = 255;
            //
            //  This is the only function setting the power, so no need to lock.
            //
//            if (!GVars.stopAtPoint)
//            {
                   GVars.motorDrive.Duration = (UInt32)(((int) s3Float ) * GVars.powerScale * GVars.byte2Pulse + 1000.0d);
//            }
//            else
//            {
//                rpmX = 0.0d;
//            }
        },
 null,
 controlStart,
 controlPeriod);

    }
}
