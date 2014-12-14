using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;
using System.IO.Ports;
using Marenco.Sensors;

namespace debugAccel
{
    public class Program
    {
        public static InterruptPort accelInt = new InterruptPort(Pins.GPIO_PIN_D7, false, Port.ResistorMode.PullDown, Port.InterruptMode.InterruptEdgeHigh);
        public static ADXL345 acc = new ADXL345(Pins.GPIO_PIN_A5, 1000);
//        static OutputPort red = new OutputPort(Pins.GPIO_PIN_D7, false);
        static OutputPort green = new OutputPort(Pins.GPIO_PIN_D8, false);
        public static int x=0;
        public static int y=0;
        public static int z=0;

        public static void Main()
        {
            // write your code here
            acc.setUpAccelRate(200);
            acc.setRange(enRange.range2g);
            Thread.Sleep(100);
            acc.setUpInterrupt();
            Thread.Sleep(100);
            acc.clearInterrupt();
            accelInt.OnInterrupt += accelInt_OnInterrupt;
            Debug.Print("In main");
            //while (true)
            //{
            //    Thread.Sleep(100);
            //    acc.getValues(ref x, ref y, ref z);
            //    Debug.Print(x.ToString()+"    "+y.ToString());
            //    acc.clearInterrupt();
            //}
            acc.clearInterrupt();
            //acc.clearInterrupt();
            //acc.clearInterrupt();
            //acc.clearInterrupt();
            //acc.clearInterrupt();
            Thread.Sleep(Timeout.Infinite);
        }

        static void accelInt_OnInterrupt(uint data1, uint data2, DateTime time)
        {
            acc.getValues(ref x, ref y, ref z);
            Debug.Print(x.ToString() + "    " + y.ToString());
            acc.clearInterrupt();
        }

    }
}
