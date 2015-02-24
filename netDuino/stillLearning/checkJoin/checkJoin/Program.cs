using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;

namespace checkJoin
{
    public class Program
    {
        public static OutputPort led = new OutputPort(Pins.ONBOARD_LED, false);
        public static InterruptPort but = new InterruptPort(Pins.ONBOARD_BTN, false, Port.ResistorMode.Disabled, Port.InterruptMode.InterruptEdgeHigh);
        public static Thread wait = new Thread(stopRotor);
        public static bool isStopped = false;
        public static readonly object lockToken = new object();


        public static void Main()
        {
            but.OnInterrupt += but_OnInterrupt;
            // write your code here
            while (true)
            {
                lock (lockToken)
                {
                    led.Write(!led.Read());
                    Thread.Sleep(50);
                    Debug.Print("In Main");
                }

            }

        }

        static void but_OnInterrupt(uint data1, uint data2, DateTime time)
        {
            Debug.Print("In interrupt");
            Thread wait = new Thread(stopRotor);
                wait.Start();
                wait.Join();
        }

        static void stopRotor()
        {
            Debug.Print("In thread");
            lock (lockToken)
            {
                led.Write(true);
                Thread.Sleep(3000);
            }
//            isStopped = false;
        }
    }
}
