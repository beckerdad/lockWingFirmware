using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;

namespace calibrateServoAngle
{
    public class Program
    {
        static UInt32 servoSignal = 1000;
        static Line a = new Line(1.0d);
       static ServoPulse flex = new ServoPulse(0);
//        static ServoPulse long1 = new ServoPulse(1);

        public static void Main()
        {
            // write your code here


            InterruptPort knop = new InterruptPort(Pins.ONBOARD_BTN, false, Port.ResistorMode.Disabled, Port.InterruptMode.InterruptEdgeLow);

            //servoOut.Start();

            knop.OnInterrupt += new Microsoft.SPOT.Hardware.NativeEventHandler(knop_OnInterrupt);

            Thread.Sleep(Timeout.Infinite);

        }

        static void knop_OnInterrupt(uint data1, uint data2, DateTime time)
        {
            servoSignal += 100;

            if (servoSignal > 1490)
            {
                servoSignal = 1290;
            }

 //           flex.Duration = servoSignal;
 //           long1.Duration = servoSignal;
        }

    }
}
