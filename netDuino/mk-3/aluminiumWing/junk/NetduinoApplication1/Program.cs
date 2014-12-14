using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;

namespace NetduinoApplication1
{
    public class Program
    {
        public static InterruptPort a = new InterruptPort(Pins.GPIO_PIN_D3, false, Port.ResistorMode.Disabled, Port.InterruptMode.InterruptEdgeBoth);
        public static void Main()
        {
            // write your code here
            a.OnInterrupt += a_OnInterrupt;

        }

        static void a_OnInterrupt(uint data1, uint data2, DateTime time)
        {
            Debug.Print("junk");
        }

    }
}
