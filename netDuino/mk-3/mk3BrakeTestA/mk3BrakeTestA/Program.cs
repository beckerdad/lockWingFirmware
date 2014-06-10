//
//  Complete re-write of the brake test program.
//  The threads are treated as seperate classes.
//  Hopefully this will lead to much cleaner code
//

using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;

namespace mk3BrakeTestA
{
    public static class GlobalVariables
    {
        public static byte[] pulsePeriod = new byte[8] { 127, 127, 127, 127, 127, 127, 127, 127 };
        public static UInt32 flexDur = 1500;
        public static UInt32 long1Dur = 1500;
        public static UInt32 long2Dur = 1500;
        public static UInt32 brakeDur = 1500;
        public static UInt32 throttleDur = 1050;

        public static OutputPort red = new OutputPort(Pins.GPIO_PIN_D7, false);
        public static OutputPort green = new OutputPort(Pins.GPIO_PIN_D8, false);

        public static PWM flex = new PWM(SecretLabs.NETMF.Hardware.Netduino.PWMChannels.PWM_PIN_D5, (UInt32)5000, (UInt32)1500, PWM.ScaleFactor.Microseconds, false);
        public static PWM long1 = new PWM(SecretLabs.NETMF.Hardware.Netduino.PWMChannels.PWM_PIN_D6, (UInt32)5000, (UInt32)1500, PWM.ScaleFactor.Microseconds, false);
        public static PWM long2 = new PWM(SecretLabs.NETMF.Hardware.Netduino.PWMChannels.PWM_PIN_D9, (UInt32)5000, (UInt32)1500, PWM.ScaleFactor.Microseconds, false);
        public static PWM throttle = new PWM(SecretLabs.NETMF.Hardware.Netduino.PWMChannels.PWM_PIN_D10, (UInt32)5000, (UInt32)1050, PWM.ScaleFactor.Microseconds, false);
        public static PWM brake = new PWM(SecretLabs.NETMF.Hardware.Netduino.PWMChannels.PWM_PIN_D3, (UInt32)10000, (UInt32)1500, PWM.ScaleFactor.Microseconds, false);

        public static bool kill = false;
        public static bool stopped = false;
        public static bool stopAtPoint = false;

        public static double rpm = 0;
    }




    public class Program
    {



        public static void Main()
        {
            //
            //  Instantiate the thread classes.
            //

//            SetPwms setPwms = new SetPwms();
            ReadPPM readPPM = new ReadPPM();
            readMagnets speedController = new readMagnets();
            GlobalVariables.flex.Start();
            GlobalVariables.long1.Start();
            GlobalVariables.long2.Start();
            GlobalVariables.throttle.Start();
            GlobalVariables.brake.Start();

            //
            //  Keep going.
            //
            Thread.Sleep(Timeout.Infinite);

        }
    }
}
