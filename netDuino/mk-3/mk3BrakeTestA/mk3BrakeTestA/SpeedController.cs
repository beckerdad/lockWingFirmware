using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;
using Marenco.Comms;

namespace mk3BrakeTestA
{
    class SpeedController
    {
        private BlueSerial blueserial = new BlueSerial();

        private int[] magnet = new int[12] { 0, 1, 2, 3, 4, 5, 0, 1, 2, 3, 4, 5 };
        private int[] rev = new int[12] { 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1 };
        private int[] action = new int[12] { 1, 1, 1, 1, 0, 0, 1, 1, 1, 1, 1, 1 };
        private long[] tMag = new long[12] { 0, 133438, 287227, 468709, 537252, 610786, 690098, 974040, 1370440, 2032104, 2404593, 3001050 };

        private InterruptPort speedInt = new InterruptPort(Pins.GPIO_PIN_A2, false, Port.ResistorMode.Disabled, Port.InterruptMode.InterruptEdgeLow);
        private long timeNow = 0;
        private long timeThen = 0;
        private long timeDiffWas = 0;
        private long stopStart = 0;
        private int count = 0;
        private int stopCount = 0;
        private int slowDown = 0;


        public SpeedController()
        {
            speedInt.OnInterrupt += new NativeEventHandler(speedCallBack);
        }

        private void speedCallBack(uint port, uint data, DateTime time)
        {
            //
            //  Synch the pulses.
            timeNow = time.Ticks;
            long timeDiff = timeNow - timeThen;
//            GlobalVariables.green.Write(!GlobalVariables.green.Read());
            //Synchronize
            if (timeDiff > 2 * timeDiffWas)
            {
                count = 1;

            }
            if (count > 5)
            {
                count = 0;
//                if (!GlobalVariables.kill) blueserial.Print((UInt32) timeDiff);    //  Time difference now.
                if (!GlobalVariables.kill) GlobalVariables.green.Write(!GlobalVariables.green.Read());
//                byte junk =  GlobalVariables.pulsePeriod[5];
//                blueserial.Print(System.Text.Encoding.UTF8.GetBytes("Th = " + junk.ToString() + "\n"));
            }

            //
            //  Stop the rotor if the flag has been set. Wait for count = 0
            //
            if (GlobalVariables.kill & count == 0 & slowDown == 0 & !GlobalVariables.stopped)
            {
                slowDown = 1;
                stopStart = timeNow;
                GlobalVariables.red.Write(false);
                GlobalVariables.green.Write(false);

//                blueserial.Print(new byte[4] { 0x3f, 0x3f, 0x3f, 0x3f });   // Brake marker        
//                blueserial.Print(System.Text.Encoding.UTF8.GetBytes("Stop Started\n"));   // Brake marker
            }

            if (slowDown == 1  & GlobalVariables.kill)
            {
                long timeIntoStop = timeNow - stopStart;
                if (timeIntoStop <= tMag[stopCount])
                    // Apply the brake
                {
                    GlobalVariables.throttle.Duration = (UInt32)(21 * 800d / 256d + 1000);
//                    blueserial.Print(new byte[4] { 0x3f, 0x3f, 0x3f, 0x3a });
//                    blueserial.Print((UInt32) timeIntoStop);
//                    blueserial.Print(System.Text.Encoding.UTF8.GetBytes("on\n"));   // Brake marker
                    GlobalVariables.red.Write(true);
                    GlobalVariables.green.Write(false);
                }
                else
                    // Release the brake. 28 was a fairly slow rotation rate. It seems good.
                {
                    GlobalVariables.throttle.Duration = (UInt32)(31 * 800d / 256d + 1000);
//                    blueserial.Print(new byte[4] { 0x3f, 0x3f, 0x3f, 0x3b });
//                    blueserial.Print((UInt32) timeIntoStop);
//                    blueserial.Print(System.Text.Encoding.UTF8.GetBytes("Off\n"));   // Brake marker
                    GlobalVariables.red.Write(false);
                    GlobalVariables.green.Write(true);
                }
                stopCount++;
                if (stopCount == 10)
                {
                    slowDown = 2;
                    stopCount = 0;
                }
            }

            //  Rotor is stopped, release the brake.
            if (slowDown == 2)
            {
                GlobalVariables.throttle.Duration = (UInt32) (31 * 800d / 256d + 1000);
                GlobalVariables.stopped = true;
                slowDown = 0;
                GlobalVariables.red.Write(true);
                GlobalVariables.green.Write(true);
            }
            timeDiffWas = timeDiff;
            timeThen = timeNow;
            count++;
        }
    }
}
