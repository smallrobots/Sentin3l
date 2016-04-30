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
using SmallRobots.Ev3ControlLib;

namespace Smallrobots.Sentin3l
{
    public class LegMotorControl : PIDController
    {
        #region Constants
        /// <summary>
        /// sampleTime for this task 20 milliseconds
        /// </summary>
        const int sampleTime = 20;
        #endregion

        #region Fields
        /// <summary>
        /// The Motor associated to this leg
        /// </summary>
        protected Motor legMotor;
        #endregion

        #region Constructors
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="legMotor">The motor to be associated with this leg</param>
        public LegMotorControl(Motor legMotor) : base(sampleTime)
        {
            // Initializes PID Controller parameters
            LcdConsole.WriteLine("Leg Param init...");
            Kp = 1.5f;             LcdConsole.WriteLine("Kp = " + Kp.ToString());
            Ki = 0.2f;              LcdConsole.WriteLine("Ki = " + Ki.ToString());
            Kd = 0.0f;              LcdConsole.WriteLine("Kd = " + Kd.ToString());
            LowPassConstant = 1.0f; LcdConsole.WriteLine("LPC = " + LowPassConstant.ToString());
            SetPoint = 0.0f;        LcdConsole.WriteLine("Sp = " + SetPoint.ToString());

            MaxPower = 50;         LcdConsole.WriteLine("MaxP = " + MaxPower.ToString());
            MinPower = -50;        LcdConsole.WriteLine("MinP = " + MinPower.ToString());

            // Associates the motor to the leg
            this.legMotor = legMotor;

            // Reset tacho count
            LcdConsole.WriteLine("Reset Tacho...");
            legMotor.ResetTacho();
            LcdConsole.WriteLine("Reset Tacho OK");
        }
        #endregion

        #region Override
        /// <summary>
        /// Executes the the PID Algorithm toward the legMotor defined in the constructor
        /// </summary>
        /// <param name="robot"></param>
        protected override void PIDAlgorithm(Robot robot)
        {
            // Updates the input signals
            ProcessVariableSignal = legMotor.GetTachoCount();

            // Computes the base algorithm
            base.PIDAlgorithm(robot);

            // Updates the output
            legMotor.SetPower(OutputSignal);
        }
        #endregion
    }
}
