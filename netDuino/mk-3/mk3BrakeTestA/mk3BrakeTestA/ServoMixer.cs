//
//  Mix the radio signals and set the PWM durations.
//

using System;
using System.Threading;
using Microsoft.SPOT.Hardware;

namespace mk3BrakeTestA
{
    static public class ServoMixer
    {
        //
        //  Numbers to mess with
        //
        private const int cyclePeriod = 10;             // Cycle period
        private const int timeToStart = 100;
        private static double colPerByte = 12d / 255d * 0.95d; // To make sure we get good signs
        private static double cycPerByte = 30d / 255d * 0.95d;
        private const double flexCol = 1.904d;
        private const double long1Col = -1.9335d;
        private const double long2Col = 1.82d;
        private const double flexCyc = -1.1229d;
        private const double long1Cyc = -1.2546d;
        private const double long2Cyc = -.11651d;
        private const double thouPerDeg = 10.5682d;
        private const int flexOff = 0;
        private const int long1Off = 0;
        private const int long2Off = 50;

        // The radio inputs are scaled from 0 to 255
        //  Assume that 3 is collective 2 is cyclic
        //
        //  The first step is to convert the command 
        //  into mm and degrees. The pulsePeriod is a 
        //  byte now and forever.
        //

        public static void MixServos ()
        {
            double colCmd = (double)(GlobalVariables.pulsePeriod[2] - 127) * colPerByte;
            double cycCmd = (double)(GlobalVariables.pulsePeriod[1] - 127) * cycPerByte;
            double throttleCmd = (double)(GlobalVariables.pulsePeriod[5]);

            GlobalVariables.flexDur = (UInt32)((flexCol * colCmd + flexCyc * cycCmd) * thouPerDeg + 1500 + flexOff);
            GlobalVariables.long1Dur = (UInt32)((long1Col * colCmd + long1Cyc * cycCmd) * thouPerDeg + 1500 + long1Off);
            GlobalVariables.long2Dur = (UInt32)((long2Col * colCmd + long2Cyc * cycCmd) * thouPerDeg + 1500 + long2Off);

            GlobalVariables.flex.Duration = GlobalVariables.flexDur;
            GlobalVariables.long1.Duration = GlobalVariables.long1Dur;
            GlobalVariables.long2.Duration = GlobalVariables.long2Dur;

        }
    }
}
