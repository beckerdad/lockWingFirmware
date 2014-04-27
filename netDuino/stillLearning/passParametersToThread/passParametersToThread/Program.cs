using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;

namespace passParametersToThread
{
    public class junk
    {
        public long count;
        public long timeThen;

        public void runInt(uint data1, uint data2, DateTime time)
        {
            count++;
            Int64 timeNow = time.Ticks;
            Int64 timeDiff = timeNow - timeThen;

            if (timeDiff < 2000 - 11)
            {
                Debug.Print(count.ToString() + "   " + timeDiff.ToString());
                //               led.Write(!led.Read());
            }
            if (timeDiff > 2000 + 11)
            {
                Debug.Print(count.ToString() + "   " + timeDiff.ToString());
                //               led.Write(!led.Read());
            }
            timeThen = timeNow;
        }

        public void timerFun(object o)
        {
        }
    }

    public class Program
    {
        public static void Main()
        {

            OutputPort led = new OutputPort(Pins.ONBOARD_LED, false);
            PWM sig = new PWM(SecretLabs.NETMF.Hardware.Netduino.PWMChannels.PWM_PIN_D10, 5000, .5, false);
            InterruptPort rec = new InterruptPort(Pins.GPIO_PIN_D9, false, Port.ResistorMode.Disabled, Port.InterruptMode.InterruptEdgeHigh);
            junk c = new junk();
            c.count = 0;
            c.timeThen = 0;
            sig.Start();

            Timer tim = new Timer(new TimerCallback(c.timerFun), false, 10, 10);

            rec.OnInterrupt += new NativeEventHandler(c.runInt);

            while (true)
            {

                Thread.Sleep(100);
                led.Write(!led.Read());
            }
            
        }

    }
}
