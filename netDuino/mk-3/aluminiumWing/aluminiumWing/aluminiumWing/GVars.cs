//#define left
#define right

using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;

namespace aluminiumWing
{
    public static class GVars
    {

        //  This is a bit inconsistent but the left and right flags are
        //  in this routine only. The servo offsets should be in the
        //  canFeedThrough class.
        //
        //  Rig the in and out bot horn aligned with slit in motor 33 and 20+16=36
        //
        //  Add a cyclic offset to rotate the tip path plane inwards.
        //

        
#if (left)
        private const Int16 swashIn = 10;
        public static double[] servoSign = new double[5] { 1, -1, 1, -1, 1 };
        // The offsets are defined relative to 1500.
        public const UInt32 frontAdd = 1500+30;
        public const UInt32 inAdd = 1500+40-swashIn;
        public const UInt32 outAdd = 1500-160-swashIn;
        public const UInt32 brakeAdd = 1500;
        public const UInt16 mask = 8;                          // Filter and mask  8 16
        public const UInt16 filter = 0x7f7;                    // 0x7f7 and 0x7ef
#endif

#if (right)
        private const Int16 swashIn = 30;
        public static double[] servoSign = new double[5] { 1, -1, -1, 1, 1 };
        // The offsets are defined relative to 1500.
        public const UInt32 frontAdd = 1500 + 30;
        public const UInt32 inAdd = 1500 - 40+swashIn;
        public const UInt32 outAdd = 1500 + 160+swashIn;
        public const UInt32 brakeAdd = 1500;
        public const UInt16 mask = 16;                          // Filter and mask  8 16
        public const  UInt16 filter = 0x7ef;                    // 0x7f7 and 0x7ef#elif right
#endif
                // in and out is 32 long, front is 38 long.
        public const double collectiveScale = 38.0d / 32.0d;
        public const double cyclicScale = 0.5d * 38.0d / 32.0d;


        //  Maximize the sensitivity of the servos with a byte as input.
        //  Based on a scaling of 1, both cyclic and collective are set with a 
        //  difference of 30/128. Add the 2 numbers

        public const double servoScale = 30.0d / 128.0d;
        public const double powerScale = 0.99d;        // Perhaps this causes fold over if 1
        public const double byte2Pulse = 500.0d / 128.0d;

        public static OutputPort red = new OutputPort(Pins.GPIO_PIN_D7, false);
        public static OutputPort green = new OutputPort(Pins.GPIO_PIN_D8, false);
        //
        //  Servo drives
        //
        public static PWM motorDrive = new PWM(SecretLabs.NETMF.Hardware.Netduino.PWMChannels.PWM_PIN_D5, (UInt32)10000, (UInt32)1000, PWM.ScaleFactor.Microseconds, false);
        public static PWM front = new PWM(SecretLabs.NETMF.Hardware.Netduino.PWMChannels.PWM_PIN_D6, (UInt32)10000, frontAdd, PWM.ScaleFactor.Microseconds, false);
        public static PWM inside = new PWM(SecretLabs.NETMF.Hardware.Netduino.PWMChannels.PWM_PIN_D9, (UInt32)10000, inAdd, PWM.ScaleFactor.Microseconds, false);
        public static PWM outside = new PWM(SecretLabs.NETMF.Hardware.Netduino.PWMChannels.PWM_PIN_D10, (UInt32)10000, outAdd, PWM.ScaleFactor.Microseconds, false);
        public static PWM brake = new PWM(SecretLabs.NETMF.Hardware.Netduino.PWMChannels.PWM_PIN_D3, (UInt32)10000, brakeAdd, PWM.ScaleFactor.Microseconds, false);
        public static AnalogInput analogIn = new AnalogInput(Cpu.AnalogChannel.ANALOG_4);
        //
        //  Global variables.
        //
        public static UInt64 timeOld = 0;
        public static UInt64 timeNow = 0;

        public static double rpm = 0;
        public static double rpmCommand = 0;
        public static bool rotorStopped = true;
        //
        //  Lock token
        //
        public static readonly object lockToken = new object();
    }
}
