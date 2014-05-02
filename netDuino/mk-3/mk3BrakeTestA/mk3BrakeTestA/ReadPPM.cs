using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;


namespace mk3BrakeTestA
{
    class ReadPPM
    {
        //
        //  Mostly hard coded timing constants. It works
        //
        private long timeWas = 0;
        private int cC = 0;
        private int glitch = 0;
        private bool[] glitchOn = new bool[8] { false, false, false, false, false, false, false, false };
        private byte[] pulsePeriodOld = new byte[8] { 127, 127, 127, 127, 127, 127, 127, 127 };
        private byte[] pulseAve = new byte[8];
        private InterruptPort ppm = new InterruptPort(Pins.GPIO_PIN_D11, false, Port.ResistorMode.PullDown, Port.InterruptMode.InterruptEdgeHigh);
        //
        //  Constructor
        //
        public ReadPPM()
        {
            ppm.OnInterrupt += new NativeEventHandler(ppmCallBack);
        }
        //
        //  Interrupt callback
        //
        private void ppmCallBack (uint port, uint data, DateTime time)
        {
            long timeNow = time.Ticks;
            long timeOn = timeNow - timeWas;
            timeWas = timeNow;

            ServoMixer.MixServos();

            if (timeOn > 50000)     // Start counting
            {
                cC = 0;
            }
            else
            {
                //
                //  The simple decoding.
                //
                long signedPeriod = (timeOn - 10000) / 40;
                if (signedPeriod < 0) signedPeriod = 0;
                if (signedPeriod > 255) signedPeriod = 255;
                GlobalVariables.pulsePeriod[cC] = (byte)signedPeriod;
                //
                //  Try and catch glitches.
                //
                glitch = (int) GlobalVariables.pulsePeriod[cC] - (int) pulsePeriodOld[cC];
                if ((glitch > 20 || glitch < -20) && !glitchOn[cC])
                {
                    GlobalVariables.pulsePeriod[cC] = pulsePeriodOld[cC];
                    glitchOn[cC] = true;
                }
                else
                {
                    glitchOn[cC] = false;
                    pulsePeriodOld[cC] = GlobalVariables.pulsePeriod[cC];
                }
                cC++;
                if (cC > 8)
                {
                    cC = 8;
                }
                if (cC == 8)
                {
                    //
                    //  Mix the servo signals
                    //
              
                    ServoMixer.MixServos();

                    //
                    //  Set the swash plate servos.
                    //

                    GlobalVariables.flex.Duration = GlobalVariables.flexDur;
                    GlobalVariables.long1.Duration = GlobalVariables.long1Dur;
                    GlobalVariables.long2.Duration = GlobalVariables.long2Dur;
                    //  Feed through the throttle
                    if (!GlobalVariables.kill) GlobalVariables.throttle.Duration = GlobalVariables.throttleDur;
                    //  Set the kill process
                    if (GlobalVariables.pulsePeriod[4] > 127)
                    {
                        GlobalVariables.kill = true;
//                        GlobalVariables.red.Write(true);
//                        GlobalVariables.green.Write(true);
                    }
                    else
                    {
                        GlobalVariables.kill = false;
                        GlobalVariables.stopped = false;
                        //GlobalVariables.brake.Duration = 1445;
                        GlobalVariables.throttle.Duration = (UInt32)((double)GlobalVariables.pulsePeriod[5] * 800d / 256d + 1000);
//                        GlobalVariables.green.Write(false);
                   }
                }
            }
            
        }
    }
}
