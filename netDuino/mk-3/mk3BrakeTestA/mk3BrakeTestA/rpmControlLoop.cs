using System;
using Microsoft.SPOT;
using System.Threading;

namespace mk3BrakeTestA
{
    class rpmControlLoop
    {
        private const double rpmMaxSpeed = 1100d; //1450.0d;                 // rpm
        private const int rpmGood = 20;                         // Green led if closer than this, red otherwise

        static private int controlPeriod = 10;                   // Control loop period in milliseconds
        static private int controlStart = 100;                  // Delay before timer starts

        private static double rpmA = 1.0d * 1.0d;                                  // Proportional gain (D) After UDP tuning
        private static double rpmB = 0.008d * 0.754d*1.2d;                              // Integral gain (B)
        private static double rpmC = .5902d * 0.8215d*1.2d;                             // Proportional gain (D)
        private static double rpmD = .1d * 1.385d;                                 // Integral gain (B)
        private const double rpmOverallGain = 1.0d;
        private const double rpmIntLimitMax = 300.0d;                            // Integral limit
        private const double rpmIntSlope = rpmIntLimitMax / 256.0d;
        private const double rpmIntLimitBot = 20.0d;
        private const int controlCutIn = 30;                                    // Speed at which the stick responds
        private const int controlRampEnd = 180;                                 // Setting for ramp end
        private const double rpmCutInSpeed = 300.0d;                                // rpm
        private const double rpmIntLimit = 250.0d;
        private static double rpmCNow = 0.0d;

        private static double rpmX = 0.0d;
        private static double s3Float = 0.0d;
        private static double rpmRequired = 0.0d;
        private const double rpmSlope = (double)(rpmMaxSpeed - rpmCutInSpeed) / (controlRampEnd - controlCutIn);
        private const double rpmConst = rpmCutInSpeed - rpmSlope * controlCutIn;
        private static double rpmError = 0.0d;

        public rpmControlLoop()
        {
        }

        private static Timer controlTimer = new Timer(delegate
        {
            s3Float = (float)(GlobalVariables.pulsePeriod[5]);
            //            Debug.Print(s3Float.ToString() + "In control loop");
            //
            //  Only start controlling from a minimum value
            //

            if (s3Float > controlCutIn)
            {
                rpmRequired = rpmSlope * s3Float + rpmConst;
                if (rpmRequired > rpmMaxSpeed) rpmRequired = rpmMaxSpeed;

                rpmError = (rpmRequired - GlobalVariables.rpm);

                //
                //  Indicate that the rpm is within 50 rpm.
                //

                if (System.Math.Abs((int)rpmError) < rpmGood)
                {
                    GlobalVariables.green.Write(true);
                    GlobalVariables.red.Write(false);
                }
                else
                {
                    GlobalVariables.green.Write(false);
                    GlobalVariables.red.Write(true);
                }

                //
                //  Limit the integrator
                //

                if (rpmX > rpmIntLimit) rpmX = rpmIntLimit;
                if (rpmX < -rpmIntLimit) rpmX = -rpmIntLimit;

                //
                //  Limit the integral gain at low speeds
                //

                if (GlobalVariables.rpm < 800.0d)
                {
                    rpmCNow = GlobalVariables.rpm / 800.0d * rpmC;
                }
                else
                {
                    rpmCNow = rpmC;
                }
                if (GlobalVariables.rpm < 300.0d)
                {
                    rpmX = 0.0d;
                }

                //
                //  Control algorithm.
                //

                s3Float = rpmCNow * rpmX + rpmD * rpmError * rpmOverallGain;
                rpmX = rpmA * rpmX + rpmB * rpmError * rpmOverallGain;

                //                Debug.Print(rpm.ToString() + " " + rpmRequired.ToString() + " " + s3Float.ToString() + " " + rpmX.ToString());
                
            }

            if (!GlobalVariables.stopAtPoint)
            {
                GlobalVariables.throttle.Duration = (UInt32)(s3Float * 800d / 256d + 1000);
            }
            else
            {
                rpmX = 0.0d;
            }
        },
 null,
 controlStart,
 controlPeriod);

    }
}
