//#define filter
#undef filter

using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;
using Marenco.Comms;

namespace aluminiumWing
{
    public class CanFeedThrough
    {
        //
        //  Definitions
        //
        public static MCP2515 canHandler = new MCP2515();
        private static MCP2515.CANMSG rxMessage = new MCP2515.CANMSG();
        private static InterruptPort CANMsgReady = new InterruptPort(Pins.GPIO_PIN_D4, true,  // A1 D2
                         Port.ResistorMode.PullUp, Port.InterruptMode.InterruptEdgeLow);

        //
        //  Filter on the data. This is too dificult to figure out, so use Matlab
        //  to get the correct values. Assume tau = 0.1, Period = 0.01 (100 Hz)
        //  c2d(ss(tf([1],[tau 1])),Period);
        //
#if filter
        private const double a = 0.904837418035960d;
        private const double b = 0.038065032785616d;
        private const double c = 2.5d;
        private static double xOldCol = 0;
        private static double xOldCyc = 0;
        private static double xCol = 0;
        private static double xCyc = 0;
#endif

        //
        //  Constructor
        //
        public CanFeedThrough()
        {
            //
            //  Start the Can controller and the interrupts
            //
            CANMsgReady.OnInterrupt += CANMsgReady_OnInterrupt;

            //  
            //  Set up the CAN parameters
            //
            canHandler.InitCAN(MCP2515.enBaudRate.CAN_BAUD_500K, GVars.filterRXF0, GVars.mask);
            canHandler.SetCANNormalMode();
            canHandler.ResetCanInterrupt();
        }
        //
        //  CAN interrupt handler.
        //
        static void CANMsgReady_OnInterrupt(uint data1, uint data2, DateTime time)
        {
            canHandler.Receive(out rxMessage, 8);       // Receive before reset
            canHandler.ResetCanInterrupt();

            if (rxMessage.CANID == GVars.CANSet)
            {

#if filter
            //
            //  Rename to my convention on the Netduino. Switch the signs here. It is just
            //  simpler but quite untidy.
            //
            double uCol = (double)(255 - rxMessage.data[3]);
            double uCyc = (double)(255 - rxMessage.data[4]);
            //
            //  Implement a glitch filter.
            //
            int collective = (int)(c * xCol);
            int cyclic = (int)(c * xCyc);
            xCol = a * xOldCol + b * uCol;
            xCyc = a * xOldCyc + b * uCyc;
            xOldCol = xCol;
            xOldCyc = xCol;
#else
                int collective = (int)(255 - rxMessage.data[3]);
                int cyclic = (int)(255 - rxMessage.data[4]);
#endif

                UInt32 servoFront = (UInt32)((collective - 127) * GVars.servoSign[1] * GVars.servoScale * GVars.byte2Pulse) +
                                    (UInt32)((cyclic - 127) * GVars.servoSign[1] * GVars.servoScale * GVars.byte2Pulse) + GVars.frontAdd;
                UInt32 servoInside = (UInt32)((collective - 127) * GVars.servoSign[2] * GVars.servoScale * GVars.byte2Pulse * GVars.collectiveScale) -
                                     (UInt32)((cyclic - 127) * GVars.servoSign[2] * GVars.servoScale * GVars.byte2Pulse * GVars.cyclicScale) + GVars.inAdd;
                UInt32 servoOutside = (UInt32)((collective - 127) * GVars.servoSign[3] * GVars.servoScale * GVars.byte2Pulse * GVars.collectiveScale) -
                                      (UInt32)((cyclic - 127) * GVars.servoSign[3] * GVars.servoScale * GVars.byte2Pulse * GVars.cyclicScale) + GVars.outAdd;

                //  Only place this is set, so no need to lock
                GVars.front.Duration = servoFront;
                GVars.inside.Duration = servoInside;
                GVars.outside.Duration = servoOutside;
                GVars.brake.Duration = 1500;

                lock (GVars.lockToken)
                {
                    GVars.rpmCommand = rxMessage.data[2]; ;
                }
            }
                //
                //  Do synchronization
                //
#if rightSynch
            else if (rxMessage.CANID == GVars.CANSynch)
            {
                RpmControlLoop.adjustRPM(time.Ticks);
            }
#endif
        }
    }
}

