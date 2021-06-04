using System;
using System.Linq;
using System.Speech.Synthesis;
using DynamixelSDKSharp;

namespace HaloTestHarness
{
  
    class Program
    {
        static SpeechSynthesizer talker = new SpeechSynthesizer();

        Program()
        {
            talker.SelectVoiceByHints(VoiceGender.Female);
            talker.SetOutputToDefaultAudioDevice();
        }

        static void WriteLineWithColor(string message, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        static void UpdateLineWithColor(string message, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.Write("\r" + message + "        "); //sorry
            Console.ResetColor();
        }

        static void Exit(int code = -1)
        {
            Console.ReadLine();
            System.Environment.Exit(code);
        }

        static void speak(string message)
        {
            talker.SpeakAsync(message);
        }

        static void chalkAndTalk(string message)
        {
            Console.WriteLine(message);
            speak(message);
        }

        static bool getAxisCalibrationValue(Servo axis, string message, out int limit)
        {
            WriteLineWithColor(message, ConsoleColor.Cyan);

            while (!Console.KeyAvailable)
            {
                UpdateLineWithColor("Present position: " + axis.ReadValue(RegisterType.PresentPosition), ConsoleColor.Green);
            }
            Console.ReadKey();

            limit = axis.ReadValue(RegisterType.PresentPosition);
            WriteLineWithColor(String.Format("\rLimit value is {0}. Correct? [y/any]", limit), ConsoleColor.Cyan);
            Console.WriteLine();

            if (Console.ReadKey(true).Key == ConsoleKey.Y)
            {
                return true;
            } else
            {
                return false;
            }
        }

        static bool storeToEEPROMAndReadback(Servo servo, RegisterType register, int value)
        {
            Console.WriteLine("Storing {0} to EEPROM.", register.ToString());

            servo.WriteValue(register, value);
            int readBack = servo.ReadValue(register);

            if (readBack == value)
            {
                Console.WriteLine("Success.");
                return true;
            } else
            {
                WriteLineWithColor(String.Format("Readback EEPROM value ({0}) does not match written value ({1})", readBack, value), ConsoleColor.Red);
                return false;
            }
        }

        static void moveToPositionBlocking(Servo servo, int position)
        {
            servo.WriteValue(RegisterType.GoalPosition, position);

            while ((Math.Abs(servo.ReadValue(RegisterType.PresentPosition) - position) > Properties.Settings.Default.PositionEpsilon))
            {
                UpdateLineWithColor("Present position: " + servo.ReadValue(RegisterType.PresentPosition), ConsoleColor.Green);
            }
        }

        static void initialiseSettings(Servo servo)
        {
            servo.WriteValue(RegisterType.ProfileAcceleration, Properties.Settings.Default.ProfileAcceleration);
            servo.WriteValue(RegisterType.ProfileVelocity, Properties.Settings.Default.ProfileVelocity);
            servo.WriteValue(RegisterType.PositionIGain, Properties.Settings.Default.PositionIGain);
        }

        static void sweepLimitsAxis1(Servo servo)
        {
            try
            {
                chalkAndTalk("Enabling Torque");
                servo.WriteValue(RegisterType.TorqueEnable, 1);

                Console.WriteLine("Setting servo defaults");
                Program.initialiseSettings(servo);

                int max = servo.ReadValue(RegisterType.MaxPositionLimit);
                int min = servo.ReadValue(RegisterType.MinPositionLimit);
                int center = min + ((max - min) / 2);

                chalkAndTalk("\rMoving to minimum limit.             ");
                moveToPositionBlocking(servo, min);

                chalkAndTalk("\rMoving to maximum limit.              ");
                moveToPositionBlocking(servo, max);

                chalkAndTalk("\rMoving to center.               ");
                moveToPositionBlocking(servo, center);

                chalkAndTalk("\rDisabling Torque                 ");
                servo.WriteValue(RegisterType.TorqueEnable, 0);

                Console.WriteLine();
            }
            catch (Exception ex)
            {
                WriteLineWithColor("Dynamixel error: " + ex.Message, ConsoleColor.Red);
                Exit();
            }
        }

        static void sweepLimitsAxis2(Servo servo)
        {
            try
            {
                chalkAndTalk("Enabling Torque");
                servo.WriteValue(RegisterType.TorqueEnable, 1);

                Console.WriteLine("Setting servo defaults");
                Program.initialiseSettings(servo);

                int max = servo.ReadValue(RegisterType.MaxPositionLimit);
                int min = servo.ReadValue(RegisterType.MinPositionLimit);
                int center = 2048;

                chalkAndTalk("\rMoving to maximum limit.           ");
                moveToPositionBlocking(servo, max);

                chalkAndTalk("\rMoving to minimum limit.            ");
                moveToPositionBlocking(servo, min);

                chalkAndTalk("\rMoving to center.             ");
                moveToPositionBlocking(servo, center);

                chalkAndTalk("\rDisabling Torque                  ");
                servo.WriteValue(RegisterType.TorqueEnable, 0);

                Console.WriteLine();
            }
            catch (Exception ex)
            {
                WriteLineWithColor("Dynamixel error: " + ex.Message, ConsoleColor.Red);
                Exit();
            }
        }

        static void calibrate(Servo axis1Servo, Servo axis2Servo)
        {
            Console.WriteLine("Clearing axis limits.");
            axis1Servo.WriteValue(RegisterType.OperatingMode, 3);
            axis1Servo.WriteValue(RegisterType.MinPositionLimit, 0);
            axis1Servo.WriteValue(RegisterType.MaxPositionLimit, 4095);
            axis2Servo.WriteValue(RegisterType.OperatingMode, 3);
            axis2Servo.WriteValue(RegisterType.MinPositionLimit, 0);
            axis2Servo.WriteValue(RegisterType.MaxPositionLimit, 4095);
            Console.WriteLine();

            int a1MaxLimit = 0;
            chalkAndTalk("Please rotate axis 1 to anticlockwise limit");
            while (!getAxisCalibrationValue(axis1Servo, "Set Axis 1 to anticlockwise (left edge towards you) position and press any key.", out a1MaxLimit)) { };
            Console.WriteLine();

            int a1MinLimit = 0;
            chalkAndTalk("Please rotate axis 1 to clockwise limit");
            while (!getAxisCalibrationValue(axis1Servo, "Set Axis 1 to clockwise (right edge towards you) limit position and press any key.", out a1MinLimit)) { };

            if (!storeToEEPROMAndReadback(axis1Servo, RegisterType.MaxPositionLimit, a1MaxLimit - Properties.Settings.Default.axis1LimitOffset)) Exit();
            if (!storeToEEPROMAndReadback(axis1Servo, RegisterType.MinPositionLimit, a1MinLimit + Properties.Settings.Default.axis1LimitOffset)) Exit();

            sweepLimitsAxis1(axis1Servo);
            Console.WriteLine();

            chalkAndTalk("Please ensure Axis 2 screw is loose");
            WriteLineWithColor("Setting Axis 2 center position. Ensure mirror is not attached and press any key.", ConsoleColor.Cyan);
            Console.ReadKey(true);

            axis2Servo.WriteValue(RegisterType.TorqueEnable, 1);
            chalkAndTalk("Homing axis 2");
            moveToPositionBlocking(axis2Servo, 2048);

            chalkAndTalk("Please tighten Axis 2 servo screw");
            WriteLineWithColor("\rAttach mirror and press any key.", ConsoleColor.Cyan);
            Console.ReadKey(true);

            axis2Servo.WriteValue(RegisterType.TorqueEnable, 0);
            chalkAndTalk("Torque disabled.");
            Console.WriteLine();

            int a2MinLimit = 0;
            chalkAndTalk("Please rotate axis 2 to backwards limit");
            while (!getAxisCalibrationValue(axis2Servo, "Set Axis 2 to backwards (weights towards you) position and press any key.", out a2MinLimit)) { };
            Console.WriteLine();

            int a2MaxLimit = 0;
            chalkAndTalk("Please rotate axis 2 to forwards limit");
            while (!getAxisCalibrationValue(axis2Servo, "Set Axis 2 to forwards (weights away from you) limit and press any key.", out a2MaxLimit)) { };

            if (!storeToEEPROMAndReadback(axis2Servo, RegisterType.MinPositionLimit, a2MinLimit + Properties.Settings.Default.axis2LimitOffset)) Exit();
            if (!storeToEEPROMAndReadback(axis2Servo, RegisterType.MaxPositionLimit, a2MaxLimit - Properties.Settings.Default.axis2LimitOffset)) Exit();

            sweepLimitsAxis2(axis2Servo);
        }

        static void sweepTest(Servo axis1Servo, Servo axis2Servo)
        {
            WriteLineWithColor("Press any key to sweep limits", ConsoleColor.Cyan);
            Console.ReadKey(true);

            WriteLineWithColor("Sweeping. Press any key to stop.", ConsoleColor.Cyan);
            speak("Sweeping. Press any key to stop.");

            try
            {
                axis1Servo.WriteValue(RegisterType.TorqueEnable, 1);
                axis1Servo.WriteValue(RegisterType.ProfileVelocity, 10);
                axis1Servo.WriteValue(RegisterType.PositionIGain, 100);
                axis2Servo.WriteValue(RegisterType.TorqueEnable, 1);
                axis2Servo.WriteValue(RegisterType.ProfileVelocity, 10);
                axis2Servo.WriteValue(RegisterType.PositionIGain, 100);

                var a1CurrentLimit = (Int16)axis1Servo.ReadValue(RegisterType.CurrentLimit);
                var a2CurrentLimit = (Int16)axis2Servo.ReadValue(RegisterType.CurrentLimit);

                var a1TargetEndpoint = RegisterType.MinPositionLimit;
                var a2TargetEndpoint= RegisterType.MinPositionLimit;

                var a1Goal = axis1Servo.ReadValue(a1TargetEndpoint);
                var a2Goal = axis2Servo.ReadValue(a2TargetEndpoint);
                axis1Servo.WriteValue(RegisterType.GoalPosition, a1Goal);
                axis2Servo.WriteValue(RegisterType.GoalPosition, a2Goal);

                WriteLineWithColor("Present Current:", ConsoleColor.Green);
                Console.WriteLine();

                while (!Console.KeyAvailable)
                {
                    if (Math.Abs(axis1Servo.ReadValue(RegisterType.PresentPosition) - a1Goal) < 3)
                    {
                        a1TargetEndpoint = (a1TargetEndpoint == RegisterType.MinPositionLimit) ? RegisterType.MaxPositionLimit : RegisterType.MinPositionLimit;
                        a1Goal = axis1Servo.ReadValue(a1TargetEndpoint);
                        axis1Servo.WriteValue(RegisterType.GoalPosition, a1Goal);
                    }

                    if (Math.Abs(axis2Servo.ReadValue(RegisterType.PresentPosition) - a2Goal) < 3)
                    {
                        a2TargetEndpoint = (a2TargetEndpoint == RegisterType.MinPositionLimit) ? RegisterType.MaxPositionLimit : RegisterType.MinPositionLimit;
                        a2Goal = axis2Servo.ReadValue(a2TargetEndpoint);
                        axis2Servo.WriteValue(RegisterType.GoalPosition, a2Goal);
                    }

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write("\r");
                    var s1PresentCurrent = Math.Abs(((Int16)axis1Servo.ReadValue(RegisterType.PresentCurrent)) * 2.69f);
                    var s2PresentCurrent = Math.Abs(((Int16)axis2Servo.ReadValue(RegisterType.PresentCurrent)) * 2.69f);

                    Console.SetCursorPosition(0, Console.CursorTop - 1);
                    var s1Line = "\r\b".PadRight((int)((s1PresentCurrent / 500.0f) * 100.0f), '|').PadRight(102, ' ') + String.Format("{0: 000.0}mA", s1PresentCurrent) + "\n";
                    var s2Line = "\r".PadRight((int)((s2PresentCurrent / 500.0f) * 100.0f), '|').PadRight(101, ' ') + String.Format("{0: 000.0}mA", s2PresentCurrent);

                    Console.Write(s1Line + s2Line);
                }
                Console.ReadKey();

                Console.ForegroundColor = ConsoleColor.White;
                axis1Servo.WriteValue(RegisterType.TorqueEnable, 0);
                axis2Servo.WriteValue(RegisterType.TorqueEnable, 0);
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine();
                chalkAndTalk("Torque disabled.");
                Console.WriteLine();
            }
            catch (Exception ex)
            {
                WriteLineWithColor("Dynamixel error: " + ex.Message, ConsoleColor.Red);
                Exit();
            }
        }

        private static void homeServos(Servo axis1Servo, Servo axis2Servo)
        {
            WriteLineWithColor("Press any key to home servos.", ConsoleColor.Cyan);
            Console.ReadKey(true);

            try
            {
                chalkAndTalk("Enabling torque.");

                axis1Servo.WriteValue(RegisterType.TorqueEnable, 1);
                axis2Servo.WriteValue(RegisterType.TorqueEnable, 1);

                int max, min, center = 0;
                max = axis1Servo.ReadValue(RegisterType.MaxPositionLimit);
                min = axis1Servo.ReadValue(RegisterType.MinPositionLimit);
                center = min + ((max - min) / 2);
                WriteLineWithColor(String.Format("\rAxis 1 center position: {0}", center), ConsoleColor.Green);
                moveToPositionBlocking(axis1Servo, center);

                max = axis2Servo.ReadValue(RegisterType.MaxPositionLimit);
                min = axis2Servo.ReadValue(RegisterType.MinPositionLimit);
                center = min + ((max - min) / 2);
                WriteLineWithColor(String.Format("\rAxis 2 center position: {0}", center), ConsoleColor.Green);
                moveToPositionBlocking(axis2Servo, center);

                axis1Servo.WriteValue(RegisterType.TorqueEnable, 0);
                axis2Servo.WriteValue(RegisterType.TorqueEnable, 0);
                chalkAndTalk("\rTorque disabled.               ");
                Console.WriteLine();
            }
            catch (Exception ex)
            {
                WriteLineWithColor("Dynamixel Error: " + ex.Message, ConsoleColor.Red);
                Exit(-1);
            }


        }

        static void Main(string[] args)
        {
            Console.CursorVisible = false;

            string portName = Properties.Settings.Default.SerialPort;
            int baud = Properties.Settings.Default.BaudRate;

            Port dxl = null;
            Console.WriteLine("Opening port {0} using baud rate {1}", portName, (int)baud);
            try
            {
                dxl = new Port(portName, (BaudRate)baud);
            }
            catch (Exception ex)
            {
                WriteLineWithColor("Couldn't open serial port: " + ex.Message, ConsoleColor.Red);
                Exit();
            }

            Console.WriteLine("Looking for servos");

            try
            {
                dxl.Refresh();
            } catch (Exception ex)
            {
                WriteLineWithColor(String.Format("Failed to find servos. Check wiring. Bailing. : {0}", ex.Message), ConsoleColor.Red);
                Exit();
            }

            if (dxl.Servos.Count != 2)
            {
                WriteLineWithColor(String.Format("Servo count error. Expected 2, got {0}. Check wiring, etc.", dxl.Servos.Count), ConsoleColor.Red);
                Exit();
            }

            Servo axis1Servo = null;
            Servo axis2Servo = null;
            if (dxl.ServoIDs[0] % 2 != 0)
            {
                axis1Servo = dxl.Servos[dxl.ServoIDs[0]];
                axis2Servo = dxl.Servos[dxl.ServoIDs[1]];
            }
            else
            {
                axis1Servo = dxl.Servos[dxl.ServoIDs[1]];
                axis2Servo = dxl.Servos[dxl.ServoIDs[0]];
            }

            WriteLineWithColor(String.Format("Axis 1 ID is: {0}", axis1Servo.ID), ConsoleColor.Green);
            WriteLineWithColor(String.Format("Axis 2 ID is: {0}", axis2Servo.ID), ConsoleColor.Green);
            WriteLineWithColor("Correct? [y/any]", ConsoleColor.Cyan);

            if (Console.ReadKey(true).Key != ConsoleKey.Y)
            {
                WriteLineWithColor("Servo ID/Axis mismatch. Bailing.", ConsoleColor.Red);
                Exit();
            }

            Console.WriteLine("Disabling Torque");
            Console.WriteLine();
            try
            {
                axis1Servo.WriteValue(RegisterType.TorqueEnable, 0);
                axis2Servo.WriteValue(RegisterType.TorqueEnable, 0);
            } catch (Exception ex)
            {
                WriteLineWithColor("Dynamixel error: " + ex.Message, ConsoleColor.Red);
                Exit();
            }

            while(true)
            {
                WriteLineWithColor("Options:", ConsoleColor.Cyan);
                WriteLineWithColor("1 - Calibrate", ConsoleColor.Cyan);
                WriteLineWithColor("2 - Sweep Test", ConsoleColor.Cyan);
                WriteLineWithColor("3 - Home Servos", ConsoleColor.Cyan);
                WriteLineWithColor("4 - Quit", ConsoleColor.Cyan);

                var key = Console.ReadKey(true).KeyChar;
                if (key == '1')
                {
                    calibrate(axis1Servo, axis2Servo);
                }
                else if (key == '2')
                {
                    sweepTest(axis1Servo, axis2Servo);
                }
                else if (key == '3')
                {
                    homeServos(axis1Servo, axis2Servo);
                }
                else if (key == '4')
                {
                    WriteLineWithColor("Done", ConsoleColor.Magenta);
                    Exit(0);
                }

                Console.WriteLine();
            }
        }
    }
}
