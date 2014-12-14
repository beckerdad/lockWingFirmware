﻿//#define leftSynch  These are defined in the project properties to get global scope
//#define rightSynch

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
//  16 October 2014 tilt the tip path planes inwards to increase
//  yaw stability. Done in GVars
//
//  Crashed again. Up the PWM frequency to 100 Hz.
//
//  Start rotorSynch branch to try and synchronize the two rotors.
//  Change rpm required based on the timing of the CAN message
//  relative to the hall effect sensor. Left will be the master.
//  Right will try and follow.
//
namespace aluminiumWing
{

    public class Program
    {
        //
        //  Instantiations.
        //
        private static InterruptPort hal = new InterruptPort(Pins.GPIO_PIN_A2, false, Port.ResistorMode.Disabled, Port.InterruptMode.InterruptEdgeLow);
        private static CanFeedThrough runCan = new CanFeedThrough();
        private static RpmControlLoop runRPM = new RpmControlLoop();
        //
        //  "Static" variables
        //
        public const double rpmScale = (double)60.0d * System.TimeSpan.TicksPerSecond;   // Scale rpm
        //
        //  Main program. Just start the threads.
        //
        public static void Main()
        {
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

        //
        //  A simple rpm calculation. Keep in the main program.
        //  Also send a CAN message from left to right.
        //

        static void hal_OnInterrupt(uint data1, uint data2, DateTime time)
        {
            lock (GVars.lockToken)
            {
                GVars.halTimeNow = time.Ticks;
                GVars.rpm = rpmScale / ((double)(GVars.halTimeNow - GVars.halTimeOld));
            }
            GVars.halTimeOld = GVars.halTimeNow;
            GVars.red.Write(!GVars.red.Read());
            //
            //  If this is the left pod, 
            //  Send an empty CAN message to the right pod.
            //  If right. do nothing.
            //

#if leftSynch
            Marenco.Comms.MCP2515.CANMSG tick = new Marenco.Comms.MCP2515.CANMSG();
            tick.CANID = GVars.CANSynch;
            tick.data = new byte[0];
            CanFeedThrough.canHandler.Transmit(tick, 100);
            GVars.addRPM = 0;
#endif
        }
    }
}
