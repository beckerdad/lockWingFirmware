using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;

//
//  Check head travel and eventually the motor controller.
//
//  From now on the convention will be that the commands are 1 
//  byte long, 0 .. 255. 
//  It is assumed that this will be the commnd limits, it is
//  the Netduino programs task to make sure that this is equates
//  to the allowed travel on the servo's. Hence, there should be an 
//  additional scaling on the commands.
//
//  There will be only one offset. The front servo cannot be adjusted
//  around zero because this will require more than a spline shift.
//  This offset is used to prevent the swash plate from hitting limits.
//  The offset will be applied directly at the period sum.
//  The left and right servo links will be used to adjust the swash-plate
//  zero angle.
//  No non-linear corrections are implemented at this time.
//
//  5 October, start with rigging. Make sure the left and right servos are
//  level. The servo offsets are done in terms of microseconds. It is simpler
//  to keep track of.
//
//  Introduce side specific code
//

namespace matlabInterface
{
    public class Program
    {
        //
        //  User defined constants.
        //






        //
        //  Instantiations.
        //

        public static InterruptPort hal = new InterruptPort(Pins.GPIO_PIN_A2, false, Port.ResistorMode.Disabled, Port.InterruptMode.InterruptEdgeLow);

        //
        //  Network interfaces.

        public static Microsoft.SPOT.Net.NetworkInformation.NetworkInterface NI = Microsoft.SPOT.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces()[0];
        public static Socket sockOut = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        public static Socket sockIn = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        public static IPEndPoint sendingEndPoint = new IPEndPoint(IPAddress.Parse("192.168.60.243"), 49001);  // 244,231,243
        public static IPEndPoint receiveEndPoint = new IPEndPoint(IPAddress.Parse("192.168.60.238"), 49002);

        public const double rpmScale = (double)60.0d * System.TimeSpan.TicksPerSecond;

        public static void Main()
        {
            //
            //  Bind IP address
            //

            Debug.Print(NI.IPAddress.ToString());
            sockIn.ReceiveTimeout = 5;
            sockIn.Bind(receiveEndPoint);

            //
            //  Start things
            //

            hal.OnInterrupt += hal_OnInterrupt;
            GVars.motorDrive.Start();
            GVars.front.Start();
            GVars.inside.Start();
            GVars.outside.Start();
            GVars.brake.Start();

            //
            //  Keep the program running.
            //

            Thread.Sleep(Timeout.Infinite);

        }

        static void hal_OnInterrupt(uint data1, uint data2, DateTime time)
        {
            lock (GVars.lockToken)
            {
                GVars.timeNow = (UInt64)time.Ticks;
                GVars.rpm = rpmScale / ((double)(GVars.timeNow - GVars.timeOld));
                GVars.rotorStopped = false;
            }
            GVars.timeOld = GVars.timeNow;
            GVars.red.Write(!GVars.red.Read());
        }

        //
        //  Detect when the rotor is stopped. Anything below 1 Hz is stopped.
        //

        static Timer stopRotor = new Timer(delegate
        {
            lock (GVars.lockToken)
            {
                if (GVars.rotorStopped)
                {
                    GVars.rpm = 0;
                }
                else
                {
                    GVars.rotorStopped = true;
                }
            }
        },
            null,
            100,
            500);

        public static Timer loopTimer = new Timer(delegate
        {
            int bytesAvailable = sockIn.Available;
            byte[] rpmByte = new byte[10];
            int accelOut = GVars.analogIn.ReadRaw();
            //               Debug.Print(bytesAvailable.ToString());
            if (bytesAvailable > 0)
            {
                sockIn.Receive(GVars.podCommand);
            }

            //
            //  Set the servos. All servos move the swash plate up.
            //
            lock (GVars.lockToken)
            {
                UInt32 servoFront = (UInt32)(((int)GVars.podCommand[1] - 127) * GVars.servoSign[1] * GVars.servoScale * GVars.byte2Pulse) +
                                    (UInt32)(((int)GVars.podCommand[2] - 127) * GVars.servoSign[1] * GVars.servoScale * GVars.byte2Pulse) + GVars.frontAdd;
                UInt32 servoInside = (UInt32)(((int)GVars.podCommand[1] - 127) * GVars.servoSign[2] * GVars.servoScale * GVars.byte2Pulse * GVars.collectiveScale) -
                                     (UInt32)(((int)GVars.podCommand[2] - 127) * GVars.servoSign[2] * GVars.servoScale * GVars.byte2Pulse * GVars.cyclicScale) + GVars.inAdd;
                UInt32 servoOutside = (UInt32)(((int)GVars.podCommand[1] - 127) * GVars.servoSign[3] * GVars.servoScale * GVars.byte2Pulse * GVars.collectiveScale) -
                                      (UInt32)(((int)GVars.podCommand[2] - 127) * GVars.servoSign[3] * GVars.servoScale * GVars.byte2Pulse * GVars.cyclicScale) + GVars.outAdd;


                GVars.front.Duration = servoFront;
                GVars.inside.Duration = servoInside;
                GVars.outside.Duration = servoOutside;
                //                brake.Duration = (UInt32)(((int)podCommand[3] - 127) * servoScale * byte2Pulse + 1500.0d);

                //
                //  Write out rpm. I do not want to lock the writing of rpm.
                //
                
                rpmByte[0] = (byte)((Int16)GVars.rpm & 0xFF);
                rpmByte[1] = (byte)(((Int16)GVars.rpm >> 8) & 0xFF);
                rpmByte[2] = (byte)((Int16)(GVars.propOut*100.0d) & 0xFF);
                rpmByte[3] = (byte)(((Int16)(GVars.propOut*100.0d) >> 8) & 0xFF);
                rpmByte[4] = (byte)((Int16)(GVars.intOut*100.0d) & 0xFF);
                rpmByte[5] = (byte)(((Int16)(GVars.intOut*100.0d) >> 8) & 0xFF);
                rpmByte[6] = (byte)((Int16)accelOut & 0xFF);
                rpmByte[7] = (byte)(((Int16)accelOut >> 8) & 0xFF);
                rpmByte[8] = (byte)((Int16)(GVars.timeNow >> 10) & 0xFF);
                rpmByte[9] = (byte)((Int16)(GVars.timeNow >> 18) & 0xFF);

                //                    Debug.Print(GVars.propFix.ToString());

                sockOut.SendTo(rpmByte, sendingEndPoint);

                GVars.rpmCommand = GVars.podCommand[0];
//                GVars.propFix = (double)((GVars.podCommand[4] * GVars.podCommand[4])) / (255.0d * 255.0d) * 4.8d + .2d;  // 1/255^2*4.8
//                GVars.intFix = (double)((GVars.podCommand[5] * GVars.podCommand[5])) / (255.0d * 255.0d) * 4.8d + .2d;
                GVars.propFix = (double)((GVars.podCommand[4] * GVars.podCommand[4])) / (255.0d * 255.0d) * 5.0d;  // 1/255^2*4.8
                GVars.intFix = (double)((GVars.podCommand[5] * GVars.podCommand[5])) / (255.0d * 255.0d) * 5.0d;
            }
        }
            , null, 100, 10);
    }
}
