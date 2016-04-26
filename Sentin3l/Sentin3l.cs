//////////////////////////////////////////////////////////////////////////////////////////////////
// SENTIN3L                                                                                     //
// Version 1.0                                                                                  //
//                                                                                              //
// Happily shared under the MIT License (MIT)                                                   //
//                                                                                              //
// Copyright(c) 2016 SmallRobots.it                                                             //
//                                                                                              //
// Permission is hereby granted, free of charge, to any person obtaining                        //
//a copy of this software and associated documentation files (the "Software"),                  //
// to deal in the Software without restriction, including without limitation the rights         //
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies             //
// of the Software, and to permit persons to whom the Software is furnished to do so,           //      
// subject to the following conditions:                                                         //
//                                                                                              //
// The above copyright notice and this permission notice shall be included in all               //
// copies or substantial portions of the Software.                                              //
//                                                                                              //
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,          //
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR     //
// PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE           //
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,          //
// TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE        //
// OR OTHER DEALINGS IN THE SOFTWARE.                                                           //
//                                                                                              //
// Visit http://wwww.smallrobots.it for tutorials and videos                                    //
//                                                                                              //
// Credits                                                                                      //
// The SENTIN3L is built with Lego Mindstorms Ev3 retail set                                    //
// Building instructions can be found on                                                        //
// "The Lego Mindstorms EV3 Laboratory" book written by Daniele Benedettelli                    //
//////////////////////////////////////////////////////////////////////////////////////////////////

using MonoBrickFirmware.Display;
using MonoBrickFirmware.Movement;
using MonoBrickFirmware.Sensors;
using MonoBrickFirmware.UserInput;
using SmallRobots.Ev3ControlLib;
using System.Threading;

namespace Smallrobots.Sentin3l
{
    /// <summary>
    /// Ev3 IR Remote Command
    /// </summary>
    public enum Direction
    {
        Stop = 0,
        Straight_Forward,
        Left_Forward,
        Right_Forward,
        Straight_Backward,
        Left_Backward,
        Right_Backward,
        Beacon_ON
    }
    /// <summary>
    /// Sentin3l main class
    /// </summary>
    public partial class Sentin3l : Robot
    {
        #region Fields
        /// <summary>
        /// Direction of Sentin3l motion
        /// </summary>
        public Direction direction;
    
        /// <summary>
        /// Left leg motor
        /// </summary>
        public Motor leftLegMotor;

        /// <summary>
        /// Right leg motor
        /// </summary>
        public Motor rightLegMotor;

        /// <summary>
        /// Body motor
        /// </summary>
        public Motor bodyMotor;

        /// <summary>
        /// EV3 IR Sensor
        /// </summary>
        public EV3IRSensor irSensor;
        #endregion

        #region Constructors
        /// <summary>
        /// Default constructor
        /// </summary>
        public Sentin3l() : base()
        {
            LcdConsole.Clear();
            LcdConsole.WriteLine("SENTIN3L init");

            // Motors initialization
            leftLegMotor = new Motor(MotorPort.OutC);
            rightLegMotor = new Motor(MotorPort.OutB);
            bodyMotor = new Motor(MotorPort.OutA);
            LcdConsole.WriteLine("Motors ok");

            // Sensors initialization
            irSensor = new EV3IRSensor(SensorPort.In4);
            LcdConsole.WriteLine("Sensors ok");

            // IR Remote task initialization
            TaskScheduler.Add(new IRRemoteTask());
            LcdConsole.WriteLine("IRRemoteTask OK");

            TaskScheduler.Add(new DriveTask());
            LcdConsole.WriteLine("DriveTask OK");
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Starts the robot behaviour
        /// </summary>
        public void Start()
        {
            // Welcome messages
            LcdConsole.Clear();
            LcdConsole.WriteLine("*****************************");
            LcdConsole.WriteLine("*                           *");
            LcdConsole.WriteLine("*      SmallRobots.it       *");
            LcdConsole.WriteLine("*                           *");
            LcdConsole.WriteLine("*       SENTIN3L  1.0       *");
            LcdConsole.WriteLine("*                           *");
            LcdConsole.WriteLine("*                           *");
            LcdConsole.WriteLine("*   Enter to start          *");
            LcdConsole.WriteLine("*   Escape to quit          *");
            LcdConsole.WriteLine("*                           *");
            LcdConsole.WriteLine("*****************************");

            // Busy wait for user
            bool enterButtonPressed = false;
            bool escapeButtonPressed = false;
            while (!(enterButtonPressed || escapeButtonPressed))
            {
                // Either the user presses the touch sensor, or presses the escape button
                // If users presses both, escape button will prevale
                enterButtonPressed = (Buttons.ButtonStates.Enter == Buttons.GetKeypress(new CancellationToken(true)));
                escapeButtonPressed = (Buttons.ButtonStates.Escape == Buttons.GetKeypress(new CancellationToken(true)));
            }

            if (escapeButtonPressed)
            {
                return;
            }

            if (enterButtonPressed)
            {
                LcdConsole.Clear();
                LcdConsole.WriteLine("*****************************");
                LcdConsole.WriteLine("*                           *");
                LcdConsole.WriteLine("*      SmallRobots.it       *");
                LcdConsole.WriteLine("*                           *");
                LcdConsole.WriteLine("*       SENTIN3L  1.0       *");
                LcdConsole.WriteLine("*                           *");
                LcdConsole.WriteLine("*                           *");
                LcdConsole.WriteLine("*        Starting....       *");
                LcdConsole.WriteLine("*                           *");
                LcdConsole.WriteLine("*                           *");
                LcdConsole.WriteLine("*****************************");

                // Acually starts the robot
                TaskScheduler.Start();
            }
        }
        #endregion
    }

    /// <summary>
    /// Periodic Task that receives commands from the Ev3 IR Remote
    /// </summary>
    public class IRRemoteTask : PeriodicTask
    {
        #region Fields
        /// <summary>
        /// Last command received from the Ev3 IR Remote
        /// </summary>
        byte remoteCommand;

        bool beaconActivated;
        #endregion

        #region Constructors
        /// <summary>
        /// Default constructor
        /// </summary>
        public IRRemoteTask() : base()
        {
            // Fields initialization
            remoteCommand = 0;
            beaconActivated = false;

            // Set the action
            Action = OnTimer;

            // Set the period
            Period = 100;
        }
        #endregion

        #region Private methods
        private void OnTimer(Robot robot)
        {
            if (beaconActivated && ((Sentin3l)robot).irSensor.ReadBeaconLocation().Distance > -100)
            {
                // Don't change Ev3 IR Sensor mode
                return;
            }
            else
            {
                // Ev3 IR Sensor mode can be changed because it's not detected anymore
                beaconActivated = false;
            }

            remoteCommand = ((Sentin3l)robot).irSensor.ReadRemoteCommand();
            switch (remoteCommand)
            {
                case 0:
                    ((Sentin3l)robot).direction = Direction.Stop;
                    LcdConsole.WriteLine("Stop");
                    break;
                case 1:
                    ((Sentin3l)robot).direction = Direction.Left_Forward;
                    LcdConsole.WriteLine("Left_Forward");
                    break;
                case 3:
                    ((Sentin3l)robot).direction = Direction.Right_Forward;
                    LcdConsole.WriteLine("Right_Forward");
                    break;
                case 5:
                    ((Sentin3l)robot).direction = Direction.Straight_Forward;
                    LcdConsole.WriteLine("Straight_Forward");
                    break;
                case 2:
                    ((Sentin3l)robot).direction = Direction.Left_Backward;
                    LcdConsole.WriteLine("Left_Backward");
                    break;
                case 4:
                    ((Sentin3l)robot).direction = Direction.Right_Backward;
                    LcdConsole.WriteLine("Right_Backward");
                    break;
                case 8:
                    ((Sentin3l)robot).direction = Direction.Straight_Backward;
                    LcdConsole.WriteLine("Straight_Backward");
                    break;
                case 9:
                    ((Sentin3l)robot).direction = Direction.Beacon_ON;
                    LcdConsole.WriteLine("Beacon_ON");
                    beaconActivated = true;
                    break;
                default:
                    ((Sentin3l)robot).direction = Direction.Stop;
                    break;
            }
        }
        #endregion
    }

    /// <summary>
    /// Periodic Task that drives the Sentin3l
    /// </summary>
    public class DriveTask : PeriodicTask
    {
        #region Fields
        Direction previousDirection;
        #endregion

        #region Constructors
        public DriveTask() : base()
        {
            // Set the action
            Action = OnTimer;

            // Set the Period
            Period = 100;
        }
        #endregion

        #region Private methods
        private void OnTimer(Robot robot)
        {
            // Adjust the LED Pattern
            if (((Sentin3l)robot).direction == Direction.Stop)
            {
                Buttons.LedPattern(3);
            }
            else if (((Sentin3l)robot).direction == Direction.Beacon_ON)
            {
                Buttons.LedPattern(2);
            }
            else
            {
                Buttons.LedPattern(1);
            }

            sbyte fullPower = 50;
            sbyte turnPower = 25;

            // Move the Formula Ev3
            switch (((Sentin3l)robot).direction)
            {
                case Direction.Beacon_ON:
                    // Read the beacon distance and location
                    break;
                case Direction.Straight_Forward:
                    if (previousDirection != Direction.Straight_Forward)
                    {
                        previousDirection = Direction.Straight_Forward;
                        ((Sentin3l)robot).leftLegMotor.SetPower(fullPower);
                        ((Sentin3l)robot).rightLegMotor.SetPower(fullPower);
                    }
                    break;
                case Direction.Straight_Backward:
                    if (previousDirection != Direction.Straight_Backward)
                    {
                        previousDirection = Direction.Straight_Backward;
                        ((Sentin3l)robot).leftLegMotor.SetPower((sbyte)-0);
                        ((Sentin3l)robot).rightLegMotor.SetPower((sbyte)-0);
                    }
                    break;
                case Direction.Left_Forward:
                    if (previousDirection != Direction.Left_Forward)
                    {
                        previousDirection = Direction.Left_Forward;
                        ((Sentin3l)robot).leftLegMotor.SetPower((sbyte)turnPower);
                        ((Sentin3l)robot).rightLegMotor.SetPower((sbyte)0);
                    }
                    break;
                case Direction.Right_Forward:
                    if (previousDirection != Direction.Right_Forward)
                    {
                        previousDirection = Direction.Right_Forward;
                        ((Sentin3l)robot).leftLegMotor.SetPower((sbyte)0);
                        ((Sentin3l)robot).rightLegMotor.SetPower((sbyte)turnPower);
                    }
                    break;
                case Direction.Left_Backward:
                    if (previousDirection != Direction.Left_Backward)
                    {
                        previousDirection = Direction.Left_Backward;
                        ((Sentin3l)robot).leftLegMotor.SetPower((sbyte)-0);
                        ((Sentin3l)robot).rightLegMotor.SetPower((sbyte)0);
                    }
                    break;
                case Direction.Right_Backward:
                    if (previousDirection != Direction.Right_Backward)
                    {
                        previousDirection = Direction.Right_Backward;
                        ((Sentin3l)robot).leftLegMotor.SetPower((sbyte)0);
                        ((Sentin3l)robot).rightLegMotor.SetPower((sbyte)-0);
                    }
                    break;
                default:
                    ((Sentin3l)robot).leftLegMotor.SetPower(0);
                    ((Sentin3l)robot).rightLegMotor.SetPower(0);
                    break;
            }
        }
        #endregion
    }
}
