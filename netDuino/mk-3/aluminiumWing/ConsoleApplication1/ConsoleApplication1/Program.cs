using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Threading;

namespace ConsoleApplication1
{
    class Program
    {
        public static SerialPort a = new SerialPort();
        public static byte[] bytesIn = new byte[11] ;

        static void Main(string[] args)
        {
            a.BaudRate = 115200;
            a.PortName = "com43";
            a.Open();
            while (true)
            {
                a.Read(bytesIn, 0, 2);
                Console.WriteLine(bytesIn[0].ToString());
                Thread.Sleep(50);
            }

        }
    }
}
