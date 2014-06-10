using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;
using Marenco.Comms;

namespace mk3BrakeTestA
{
    class readMagnets
    {
        private BlueSerial blueserial = new BlueSerial();

        private static InterruptPort speedInt = new InterruptPort(Pins.GPIO_PIN_A2, false, Port.ResistorMode.Disabled, Port.InterruptMode.InterruptEdgeLow);

        private static long timeNow = 0;
        private static long timeThen = 0;
        private static long timeDiffWas = 0;
        private static UInt16 count = 0;

        private static long timeDiff = 0;
        private static long rpmStartTime = 0;
        private const double ticksPerSecond = (double) System.TimeSpan.TicksPerSecond;

        public readMagnets()
        {
            speedInt.OnInterrupt += new NativeEventHandler(speedCallBack);
        }

        private void speedCallBack(uint port, uint data, DateTime time)
        {
            //
            //  Synch the pulses.
            timeNow = time.Ticks;
            timeDiff = timeNow - timeThen;

            if (timeDiff > 2 * timeDiffWas)
            {
                count = 1;
                long rpmEndTime = time.Ticks;
                GlobalVariables.rpm = (double) (rpmEndTime - rpmStartTime);    // Time in ticks
                GlobalVariables.rpm = 60.0d / (GlobalVariables.rpm / ticksPerSecond);
                rpmStartTime = rpmEndTime;
            }
            if (count > 5)
            {
                count = 0;
                if (GlobalVariables.kill)
                {
                    GlobalVariables.stopAtPoint = true;
                    GlobalVariables.throttle.Duration = (UInt32) (20 * 800d / 256d + 1000);
                }
            }

            //
            //  Write out the pulse times.
            //
            if (GlobalVariables.stopAtPoint)
            {
                blueserial.Print((UInt16) (count + 10));
                blueserial.Print(timeDiff);
            }
            else
            {
                blueserial.Print(count);
                blueserial.Print(timeDiff);
            }

            //  Clean up the loop.

            timeDiffWas = timeDiff;
            timeThen = timeNow;
            count++;

        }
    }
}
