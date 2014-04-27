using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;

namespace testBlue
{
    public class Program
    {
        public static void Main()
        {
            // write your code here
            BlueSerial blot = new BlueSerial();
            byte[] index = new byte[1] { 0};
            byte[] hello = new byte[6] { 115,116,114,105,110,103 };
            index[0] = 0;

            while (true)
            {
                blot.Print(hello);
                index[0] += 1;
                Thread.Sleep(50);
            }

        }

    }
}
