using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;

namespace matlabInterface
{
    public static class GVars
    {

        public const double servoScale = 30.0d / 128.0d;
        public const double powerScale = 1.0d;
        public const double byte2Pulse = 500.0d / 128.0d;

        public static OutputPort red = new OutputPort(Pins.GPIO_PIN_D7, false);
        public static OutputPort green = new OutputPort(Pins.GPIO_PIN_D8, false);

        //
        //  Servo drives
        //

        public static PWM motorDrive = new PWM(SecretLabs.NETMF.Hardware.Netduino.PWMChannels.PWM_PIN_D5, (UInt32)20000, (UInt32)1050, PWM.ScaleFactor.Microseconds, false);
        public static PWM front = new PWM(SecretLabs.NETMF.Hardware.Netduino.PWMChannels.PWM_PIN_D6, (UInt32)20000, (UInt32)1500, PWM.ScaleFactor.Microseconds, false);
        public static PWM inside = new PWM(SecretLabs.NETMF.Hardware.Netduino.PWMChannels.PWM_PIN_D9, (UInt32)20000, (UInt32)1500, PWM.ScaleFactor.Microseconds, false);
        public static PWM outside = new PWM(SecretLabs.NETMF.Hardware.Netduino.PWMChannels.PWM_PIN_D10, (UInt32)20000, (UInt32)1500, PWM.ScaleFactor.Microseconds, false);
        public static PWM brake = new PWM(SecretLabs.NETMF.Hardware.Netduino.PWMChannels.PWM_PIN_D3, (UInt32)20000, (UInt32)1500, PWM.ScaleFactor.Microseconds, false);

        //
        //  Global variables.
        //

        public static UInt64 timeOld = 0;
        public static readonly object lockToken = new object();

        public static double rpm = 0;
        public static double rpmCommand = 0;
        public static bool rotorStopped = true;

        public static double iFix = 1;
        public static double propFix = 1;
    }
}
