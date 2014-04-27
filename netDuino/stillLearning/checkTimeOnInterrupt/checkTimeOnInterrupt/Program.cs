using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;
using LoveElectronics.Resources;

namespace checkTimeOnInterrupt
{
    public class Program
    {

        public static Int64 timeThen = 0;
        public static Int64 count = 0;

//        public static OutputPort led = new OutputPort(Pins.ONBOARD_LED, false);

        public static void Main()
        {

            OutputPort led = new OutputPort(Pins.ONBOARD_LED, false);
            PWM sig = new PWM(SecretLabs.NETMF.Hardware.Netduino.PWMChannels.PWM_PIN_D10, 5000, .5, false);
            InterruptPort rec = new InterruptPort(Pins.GPIO_PIN_D9, false, Port.ResistorMode.Disabled, Port.InterruptMode.InterruptEdgeHigh);

            sig.Start();

            rec.OnInterrupt += new NativeEventHandler(rec_OnInterrupt);

            while (true)
            {
                keepBusy();
                Thread.Sleep(100);
                led.Write(!led.Read());
            }

        }

        static void rec_OnInterrupt(uint data1, uint data2, DateTime time)
        {
            count++;
            Int64 timeNow = time.Ticks;
            Int64 timeDiff = timeNow - timeThen;

            if (timeDiff < 2000 - 11)
            {
                Debug.Print(count.ToString() + "   " + timeDiff.ToString());
 //               led.Write(!led.Read());
            }
            if (timeDiff > 2000 + 11)
            {
                Debug.Print(count.ToString() + "   " + timeDiff.ToString());
 //               led.Write(!led.Read());
            }
            timeThen = timeNow;

        }

        static void keepBusy()
        {
            byte[] a = new byte[8] {1,2,3,4,5,6,7,8};
            Double b = BitConverter.ToDouble(a);
        }
 
    }
}
