

using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;

namespace mk3BrakeTest
{
    public class Program
    {
        #region UserConstants
        #endregion

        #region declarations
        //
        //  PPM signal interrupt pin. A5 is analog 2
        //
        static public InterruptPort ppm = new InterruptPort(Pins.GPIO_PIN_D4, false, Port.ResistorMode.PullDown, Port.InterruptMode.InterruptEdgeHigh);
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
        static public byte[] pulsePeriod = new byte[8];
        static public byte[] pulsePeriodOld = new byte[8];
        static public byte[] pulseAve = new byte[8];

        //
        //  The PWM signals for the different channels
        //

        static PWM flex = new PWM(SecretLabs.NETMF.Hardware.Netduino.PWMChannels.PWM_PIN_D5, 20000, 1500, PWM.ScaleFactor.Microseconds, false);
        static PWM long1 = new PWM(SecretLabs.NETMF.Hardware.Netduino.PWMChannels.PWM_PIN_D6, 20000, 1500, PWM.ScaleFactor.Microseconds, false);
        static PWM long2 = new PWM(SecretLabs.NETMF.Hardware.Netduino.PWMChannels.PWM_PIN_D9, 20000, 1500, PWM.ScaleFactor.Microseconds, false);
        //Close the throttle
        static PWM throttle = new PWM(SecretLabs.NETMF.Hardware.Netduino.PWMChannels.PWM_PIN_D10, 20000, 1000, PWM.ScaleFactor.Microseconds, false);
        static PWM brake = new PWM(SecretLabs.NETMF.Hardware.Netduino.PWMChannels.PWM_PIN_D3, 20000, 1500, PWM.ScaleFactor.Microseconds, false);
        
        //
        //  LED's for general use.
        //
        public static OutputPort onLed = new OutputPort(Pins.ONBOARD_LED, false);       // Can timeout
        public static OutputPort red = new OutputPort(Pins.GPIO_PIN_D7, false);          // Speed hold false
        public static OutputPort green = new OutputPort(Pins.GPIO_PIN_D8, false);       // Speed hold true

        #endregion

        #region methodsDefinitions
        //
        //  Timer for the servo settings
        //
        static Timer pulseServos = new Timer(delegate
            {

            }
            , null, 0, 20);
        //
        //  Read the PPM signal
        //
        static void ppm_OnInterrupt(uint port, uint data, DateTime time)
        {

            timeNow = time.Ticks;
            timeOn = timeNow - timeWas;
            timeWas = timeNow;


            if (timeOn > 50000)
            {
                 cC = 0;
            }
            else
            {
                signedPeriod = (timeOn - 10000) / 40;
                if (signedPeriod < 0) signedPeriod = 0;
                if (signedPeriod > 255) signedPeriod = 255;
                pulsePeriod[cC] = (byte)signedPeriod;

                glitch = (int)pulsePeriod[cC] - (int)pulsePeriodOld[cC];

                if ((glitch > 20 || glitch < -20) && !glitchOn[cC])
                {
                    Debug.Print(cC.ToString() + " " + glitch.ToString());
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
                // Debug.Print(cC.ToString() + " " + signedPeriod.ToString());
            }
        }
        #endregion

        public static void Main()
        {
            //
            //  Strat the ppm interrupt
            //
            ppm.OnInterrupt += new NativeEventHandler(ppm_OnInterrupt);
            //
            //  Initialize some stuff
            //
            flex.Start();
            long1.Start();
            long2.Start();
            //
            //  An that is all folks
            //
            Thread.Sleep(Timeout.Infinite);
        }

    }
}
