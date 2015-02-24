
using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;

namespace aluminiumWing
{
    public static class GVars
    {
        //
        //  Can messages are:
        //  CANStop = 2
        //  CANSynch = 4
        //  CANSet = 8 for left
        //  CANSet = 16 for right
        //
        //  Servo signs and oofsets are different for the two pods.
        //
        //  swashIn turns the rotor tip path plane inwards for stability.
        //  I do not know why left and right are different.
        //

#if leftSynch
        private const Int16 swashIn = 10;
        public static double[] servoSign = new double[5] { 1, -1, 1, -1, 1 };
        // The offsets are defined relative to 1500.
        public const UInt32 frontAdd = 1500+30;
        public const UInt32 inAdd = 1500+40-swashIn;
        public const UInt32 outAdd = 1500-160-swashIn;
        public const UInt32 brakeAdd = 1500;
        public const UInt32 brakeOff = 1500;            // Brake off servo signal
        public const UInt32 brakeOn = 1500;             // Brake on servo signal
        public const UInt16 mask = 0x7F5;                          // Mask for 2 4 8 (0 10) Dec
        public const UInt16 filterRXF0 = 0;                        // does not matter
        public const UInt16 CANStop = 2;
        public const UInt16 CANSynch = 4;
        public const UInt16 CANSet = 8;
#endif

#if rightSynch
        private const Int16 swashIn = 30;
        public static double[] servoSign = new double[5] { 1, -1, -1, 1, 1 };
        // The offsets are defined relative to 1500.
        public const UInt32 frontAdd = 1500 + 30;
        public const UInt32 inAdd = 1500 - 40+swashIn;
        public const UInt32 outAdd = 1500 + 160+swashIn;
        public const UInt32 brakeAdd = 1500;
        public const UInt32 brakeOff = 1500;            // Brake off servo signal
        public const UInt32 brakeOn = 1500;             // Brake on servo signal
        public const UInt16 mask = 0x7E9;                       // mask for 2 4 16 (0 6 22) (Dec)
        public const UInt16 filterRXF0 = 0;                    // does not matter
        public const UInt16 CANStop = 2;
        public const UInt16 CANSynch = 4;
        public const UInt16 CANSet = 16;
#endif
        // Servo arms on in and out are 32 long, front is 38 long.

        public const double collectiveScale = 38.0d / 32.0d;
        public const double cyclicScale = 0.5d * 38.0d / 32.0d;

        //
        //  Maximize the sensitivity of the servos with a byte as input.
        //  Based on a scaling of 1, both cyclic and collective are set with a 
        //  difference of 30/128. Add the 2 numbers
        // 

        public const double servoScale = 30.0d / 128.0d;
        public const double powerScale = 0.99d;                 // Perhaps this causes fold over if 1
        public const double byte2Pulse = 500.0d / 128.0d;

        public static OutputPort red = new OutputPort(Pins.GPIO_PIN_D7, false);
        public static OutputPort green = new OutputPort(Pins.GPIO_PIN_D8, true);  // May be broken on one bord
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
        public static Int64 halTimeOld = 0;
        public static Int64 halTimeNow = 0;

        public static double rpm = 0.0001d;
        public static double rpmCommand = 0;
        public static bool rotorStopped = true;
        //
        //  RPM control variables
        //
        public static double rpmRequired = 0.00001d;
        public static double addRPM = 0;
        //
        //  Lock token
        //
        public static readonly object lockToken = new object();
        //
        //  Stop control flags
        //
        public static bool stopCMD = false;              // Stop command is received.
        public static bool stopStart = false;
        public static bool stopped = false;
        //
        //  Stop control timing
        //
        public static int timeStopDelay = 0;
        public static int timeBrakeOn = 400;
        public static int timePowerOn = 500;
    }
}
