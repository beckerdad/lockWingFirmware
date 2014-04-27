using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;

namespace calibrateServoAngle
{

    class ServoPulse() : PWM
    {
        private static int[] pwmPinAssignments = new int[5] { 5, 6, 9, 10, 3 };
        private static PWM servoOut;

        //public void ServoPulse()
        //{
        //}

        public void servoPulse(int channelNumber)
        {
            switch (channelNumber)
            {
                case 0:
                    PWM servoOut = new PWM(SecretLabs.NETMF.Hardware.Netduino.PWMChannels.PWM_PIN_D5, 20000, 1500, PWM.ScaleFactor.Microseconds, false);
                    servoOut.Start();
                    break;
            //    case 1:
            //        servoOut = new PWM(SecretLabs.NETMF.Hardware.Netduino.PWMChannels.PWM_PIN_D6, 20000, 1500, PWM.ScaleFactor.Microseconds, false);
            //        servoOut.Start();
            //        break;
            //    case 2:
            //        servoOut = new PWM(SecretLabs.NETMF.Hardware.Netduino.PWMChannels.PWM_PIN_D9, 20000, 1500, PWM.ScaleFactor.Microseconds, false);
            //        servoOut.Start();
            //        break;
            //    case 3:
            //        servoOut = new PWM(SecretLabs.NETMF.Hardware.Netduino.PWMChannels.PWM_PIN_D10, 20000, 1500, PWM.ScaleFactor.Microseconds, false);
            //        servoOut.Start();
            //        break;
            //    case 4:
            //        servoOut = new PWM(SecretLabs.NETMF.Hardware.Netduino.PWMChannels.PWM_PIN_D3, 20000, 1500, PWM.ScaleFactor.Microseconds, false);
            //        servoOut.Start();
            //        break;
            }  
        }

        public void setServoDuration(UInt32 duration)
        {
            servoOut.Duration = duration;
        }
    }
}


