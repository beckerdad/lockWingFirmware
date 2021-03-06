﻿using System;
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
        public static OutputPort green = new OutputPort(Pins.GPIO_PIN_D7, false);
        public static PWM c3 = new PWM(SecretLabs.NETMF.Hardware.Netduino.PWMChannels.PWM_PIN_D3, (UInt32)20000, (UInt32)1500, PWM.ScaleFactor.Microseconds, false);
        public static PWM c5 = new PWM(SecretLabs.NETMF.Hardware.Netduino.PWMChannels.PWM_PIN_D5, (UInt32)20000, (UInt32)1500, PWM.ScaleFactor.Microseconds, false);
        public static PWM c6 = new PWM(SecretLabs.NETMF.Hardware.Netduino.PWMChannels.PWM_PIN_D6, (UInt32)20000, (UInt32)1500, PWM.ScaleFactor.Microseconds, false);
        public static PWM c9 = new PWM(SecretLabs.NETMF.Hardware.Netduino.PWMChannels.PWM_PIN_D9, (UInt32)20000, (UInt32)1500, PWM.ScaleFactor.Microseconds, false);
        public static PWM c10 = new PWM(SecretLabs.NETMF.Hardware.Netduino.PWMChannels.PWM_PIN_D10, (UInt32)20000, (UInt32)1500, PWM.ScaleFactor.Microseconds, false);
        public static InterruptPort hal = new InterruptPort(Pins.GPIO_PIN_A2, false, Port.ResistorMode.Disabled, Port.InterruptMode.InterruptEdgeLow);

        public static void Main()
        {
            // write your code here
            c3.Start();
            c5.Start();
            c6.Start();
            c9.Start();
            c10.Start();
            hal.OnInterrupt += hal_OnInterrupt;

            Thread.Sleep(Timeout.Infinite);

            //

        }

        static void hal_OnInterrupt(uint data1, uint data2, DateTime time)
        {
            green.Write(!green.Read());
        }

    }
}
