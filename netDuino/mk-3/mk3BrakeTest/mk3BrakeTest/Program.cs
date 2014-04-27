//
//  Almost completely new version of the swash plate controller
//  for testing the new mk-3 brake mechanism in the wind tunnel.
//
//  Work with the PWM signals in the range of 1000 to 2000.
//  and 50 Hz at this time. The throttle will be initialized to zero.
//  Becker, March 2014
//

using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;
using LoveElectronics.Resources;

namespace mk3BrakeTest
{
    public class Program
    {
        #region UserConstants
        const UInt32 ticksPerSecond = (UInt32) System.TimeSpan.TicksPerSecond;
        public const double flexCol = 1.904d;
        public const double long1Col = -1.9335d;
        public const double long2Col = 1.82d;
        public const double flexCyc = -1.1229d;
        public const double long1Cyc = -1.2546d;
        public const double long2Cyc = -.11651d;
        public const double thouPerDeg = 10.5682d;      // Thousands per degree
        public const double colPerByte = 12d / 255d * 0.95d; // To make sure we get good signs
        public const double cycPerByte = 30d / 255d * 0.95d;
        public static double colCmd = 0.0d;
        public static double cycCmd = 0.0d;
        public static double brakeCmd = 0.0d;
        public static double throttleCmd = 0.0d;
        public static int flexOff = 0;
        public static int long1Off = 0;
        public static int long2Off = 50;
        public static UInt32 flexDur = 1500;
        public static UInt32 long1Dur = 1500;
        public static UInt32 long2Dur = 1500;
        public static UInt32 brakeDur = 1500;
        public static UInt32 throttleDur = 1050;
        public static UInt32 offTime = 0;
        public static UInt32 junkTime = 0;
        public static UInt32[] timeDataOut = new UInt32[3] { 0, 0, 0 };
        public static byte[] byte4 = new byte[4] { 0, 0, 0, 0 };
        public static byte[] byte12 = new byte[12] {0,0,0,0,0,0,0,0,0,0,0,0};
        public static bool stopFlag = false;
        public static bool killFlag = false;
        public static bool switchPulse = true;

        #endregion

        #region staticVariables
        static public bool synch = true;
        static public int cC = 0;
        static public int cCheck = 0;
        static public int glitch = 0;
        static public bool[] glitchOn = new bool[8] { false, false, false, false, false, false, false, false };
        static public int onPeriod = 0;
        static public Int64 timeWas = 0;
        static public Int64 timeOn = 0;
        static public Int64 timeNow = 0;
        static public Int64 signedPeriod = 0;
        static public byte[] pulsePeriod = new byte[8] { 127, 127, 127, 127, 127, 127, 127, 127 };
        static public byte[] pulsePeriodOld = new byte[8] { 127, 127, 127, 127, 127, 127, 127, 127 };
        static public byte[] pulseAve = new byte[8];
        #endregion

        #region Declarations
        //
        //  PPM2 interrupt port. Do not put on a PWM signal
        //
        static public InterruptPort ppm = new InterruptPort(Pins.GPIO_PIN_D7, false, Port.ResistorMode.PullDown, Port.InterruptMode.InterruptEdgeHigh);
        
        //
        //  Servo signals
        //
        //      static PWM pulse1 = new PWM(SecretLabs.NETMF.Hardware.Netduino.PWMChannels.PWM_PIN_D5, pwmPeriod, (UInt32)PWMCenter, PWM.ScaleFactor.Microseconds, false);

        static PWM flex = new PWM(SecretLabs.NETMF.Hardware.Netduino.PWMChannels.PWM_PIN_D5, (UInt32) 5000, (UInt32) 1500, PWM.ScaleFactor.Microseconds, false);
        static PWM long1 = new PWM(SecretLabs.NETMF.Hardware.Netduino.PWMChannels.PWM_PIN_D6, (UInt32) 5000, (UInt32) 1500, PWM.ScaleFactor.Microseconds, false);
        static PWM long2 = new PWM(SecretLabs.NETMF.Hardware.Netduino.PWMChannels.PWM_PIN_D9, (UInt32) 5000, (UInt32) 1500, PWM.ScaleFactor.Microseconds, false);
        static PWM throttle = new PWM(SecretLabs.NETMF.Hardware.Netduino.PWMChannels.PWM_PIN_D10, (UInt32) 5000, (UInt32) 1050, PWM.ScaleFactor.Microseconds, false);
        //Close the throttle
        static PWM brake = new PWM(SecretLabs.NETMF.Hardware.Netduino.PWMChannels.PWM_PIN_D3, (UInt32) 10000, (UInt32) 1500, PWM.ScaleFactor.Microseconds, false);
        //
        //  LED's for general use.
        //
        public static OutputPort onLed = new OutputPort(Pins.ONBOARD_LED, false);       // Can timeout
//        public static OutputPort red = new OutputPort(Pins.GPIO_PIN_D7, false);          // Speed hold false
        public static OutputPort green = new OutputPort(Pins.GPIO_PIN_D8, false);       // Speed hold true
        //
        //        public static Microsoft.SPOT.Net.NetworkInformation.NetworkInterface NI = Microsoft.SPOT.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces()[0];
        //        public static Socket send = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        //        public static IPEndPoint sendingEndPoint = new IPEndPoint(IPAddress.Parse("192.168.1.211"), 49002);

        public static BlueSerial blot = new BlueSerial();

        public static AnalogInput current = new AnalogInput(Cpu.AnalogChannel.ANALOG_4);
        public static InterruptPort speedInt = new InterruptPort(Pins.GPIO_PIN_A2, false, Port.ResistorMode.Disabled, Port.InterruptMode.InterruptEdgeBoth);

        #endregion

        #region methodsDefinitions
        //
        //  Timer for the servo settings
        //
        static Timer pulseServos = new Timer(delegate
        {
            // The radio inputs are scaled from 0 to 255
            //  Assume that 3 is collective 2 is cyclic
            //
            //  The first step is to convert the command 
            //  into mm and degrees. The pulsePeriod is a 
            //  byte now and forever.
            //

            colCmd = (double) (pulsePeriod[2] - 127) * colPerByte;
            cycCmd = (double) (pulsePeriod[1] - 127) * cycPerByte;
            throttleCmd = (double)(pulsePeriod[5]);

            flexDur = (UInt32) ((flexCol * colCmd + flexCyc * cycCmd) * thouPerDeg + 1500 + flexOff);
//            Debug.Print(flexDur.ToString());
            long1Dur = (UInt32) ((long1Col * colCmd + long1Cyc * cycCmd) * thouPerDeg + 1500 + long1Off);
            long2Dur = (UInt32) ((long2Col * colCmd + long2Cyc * cycCmd) * thouPerDeg + 1500 + long2Off);

            throttleDur = (UInt32)(throttleCmd * 800d / 256d + 1000);

            if (pulsePeriod[4] < 127)
            {
                // Brake off
                brakeDur = 1445;
//                Debug.Print(brakeDur.ToString() + "Less than 127");
                stopFlag = false;
                killFlag = false;
            }
            else
            {
                // Initiate stop procedure
                //brakeDur = 1542;
                stopFlag = true;
            }

            //
            //  Set the servos
            //

            if (!stopFlag)
            {
                flex.Duration = flexDur;
                long1.Duration = long1Dur;
                long2.Duration = long2Dur;
                throttle.Duration = throttleDur;
                brake.Duration = brakeDur;
            }
            else
            {
                if (switchPulse)
                {
                    throttleDur = (UInt32)(21 * 800d / 256d + 1000);
                    throttle.Duration = throttleDur;
                    switchPulse = false;
                }
                else
                {
                    throttleDur = (UInt32)(36 * 800d / 256d + 1000);
                    throttle.Duration = throttleDur;
                    switchPulse = true;

                }
            }
        }
        , null, 200, 40);

        static Timer anaOut = new Timer(delegate
            {
                double aOut = current.Read();
//                Debug.Print(aOut.ToString());
            }, null, 200, 50);

        //
        //  Decode the PPM2 signal
        //
        static void ppm_OnInterrupt(uint port, uint data, DateTime time)
        {

            timeNow = time.Ticks;
            timeOn = timeNow - timeWas;
            timeWas = timeNow;


            if (timeOn > 50000)
            {
                //                Debug.Print(cCheck.ToString());

                //                send.SendTo(pulsePeriod, sendingEndPoint);
                cC = 0;
            }
            else
            {
                signedPeriod = (timeOn - 10000) / 40;
                if (signedPeriod < 0) signedPeriod = 0;
                if (signedPeriod > 255) signedPeriod = 255;
                pulsePeriod[cC] = (byte) signedPeriod;

                glitch = (int)pulsePeriod[cC] - (int)pulsePeriodOld[cC];

                if ((glitch > 20 || glitch < -20) && !glitchOn[cC])
                {
 //                   Debug.Print(cC.ToString() + " " + glitch.ToString());
                    pulsePeriod[cC] = pulsePeriodOld[cC];
                    glitchOn[cC] = true;
                }
                else
                {
                    glitchOn[cC] = false;
                    pulsePeriodOld[cC] = pulsePeriod[cC];
                }
                cC++;
                cCheck = cC;
                if (cC > 8)
                {
                    cC = 8;
                    cCheck = 11;
                }
 //               if (cC == 5)
 //                   Debug.Print(pulsePeriod[5].ToString());
            }
        }

        #endregion

        public static void Main()
        {
            // write your code here

            //            Debug.Print(NI.IPAddress.ToString());
            //            NI.EnableStaticIP("192.168.1.210", "255.255.255.0", "192.168.1.1");
            flex.Start();
            long1.Start();
            long2.Start();
            throttle.Start();
            brake.Start();

            ppm.OnInterrupt += new NativeEventHandler(ppm_OnInterrupt);
            speedInt.OnInterrupt += new NativeEventHandler(speedInt_OnInterrupt);



            Thread.Sleep(Timeout.Infinite);

        }




        static void speedInt_OnInterrupt(uint data1, uint data2, DateTime time)
        {
            if (data2 > 0)
            {
                junkTime = (UInt32) Microsoft.SPOT.Hardware.Utility.GetMachineTime().Ticks;
                timeDataOut[1] = junkTime - offTime;
                offTime = junkTime;
                byte4 = LoveElectronics.Resources.BitConverter.GetBytes(timeDataOut[1]);
                byte12[4] = byte4[0];
                byte12[5] = byte4[1];
                byte12[6] = byte4[2];
                byte12[7] = byte4[3];
                blot.Print(byte12);
            }
            else
            {
                junkTime = (UInt32)Microsoft.SPOT.Hardware.Utility.GetMachineTime().Ticks;
                timeDataOut[2] = junkTime - offTime;
                byte4 = LoveElectronics.Resources.BitConverter.GetBytes(timeDataOut[2]);
                byte12[8] = byte4[0];
                byte12[9] = byte4[1];
                byte12[10] = byte4[2];
                byte12[11] = byte4[3];

                if (stopFlag)       // Switch on controller has been set
                {

                    if (timeDataOut[2] - timeDataOut[1] > 9000)
                    {
                        if (killFlag)
                        {
                            Thread.Sleep(100);
                            //throttleDur = (UInt32)(21 * 800d / 256d + 1000);
                            //throttle.Duration = throttleDur;
                            Debug.Print("Second kill");
                        }
                        else
                        {
                            //throttleDur = (UInt32)(21 * 800d / 256d + 1000);   // idle motor
                            //throttle.Duration = throttleDur;
                            Thread.Sleep(600);
                            brakeDur = 1542;
                            brake.Duration = brakeDur;
                            //                            Thread.Sleep(200);
                            //                          throttleDur = (UInt32)(21 * 800d / 256d + 1000);
                            //                        throttle.Duration = throttleDur;
                            killFlag = true;
                            Debug.Print("First kill");
                        }
                    }
                    //else
                    //{
                        // motor braking stage


                        // Create the thread object. This does not start the thread.

//                    throttleDur = (UInt32)(21 * 800d / 256d + 1000);



//                    }



                }

            }




        }

    }
}
