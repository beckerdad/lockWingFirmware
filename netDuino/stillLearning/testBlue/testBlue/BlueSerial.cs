using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;
using System.IO.Ports;

namespace testBlue
{
    public class BlueSerial
    {
        static SerialPort serialPort;
        public BlueSerial(int baudRate = 115200, Parity parity = Parity.None, int dataBits = 8, StopBits stopBits = StopBits.One)
        {
            serialPort = new SerialPort(SerialPorts.COM1, baudRate, parity, dataBits, stopBits);
            serialPort.ReadTimeout = 1; // Set to 10ms. Default is -1?!
            serialPort.Open();
        }

        public void Print(byte[] effort)
        {
            serialPort.Write(effort, 0, effort.Length);
        }

    }
}

