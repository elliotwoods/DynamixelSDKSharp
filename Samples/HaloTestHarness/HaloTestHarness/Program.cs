using System;
using System.Linq;
using System.Speech.Synthesis;
using DynamixelSDKSharp;

namespace HaloTestHarness
{
  
    class Program
    {
        static SpeechSynthesizer talker = new SpeechSynthesizer();

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
            talker.SelectVoiceByHints(VoiceGender.Female);
            talker.SetOutputToDefaultAudioDevice();
            talker.Speak(message);
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
                UpdateLineWithColor("Current: " + axis.ReadValue(RegisterType.PresentPosition), ConsoleColor.Green);
            }
            Console.ReadKey();

            limit = axis.ReadValue(RegisterType.PresentPosition);
            WriteLineWithColor(String.Format("\rLimit value is {0}. Correct? [y/any]", limit), ConsoleColor.Cyan);

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

            servo.WriteValue(register, value, true);
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

        static void MoveToPositionSync(Servo servo, int position)
        {
            servo.WriteValue(RegisterType.ProfileVelocity, 10);
            servo.WriteValue(RegisterType.GoalPosition, position, true);

            while(servo.ReadValue(RegisterType.PresentPosition) != position)
            {
                UpdateLineWithColor("Current: " + servo.ReadValue(RegisterType.PresentPosition), ConsoleColor.Green);
            }
        }

        static void sweepLimits(Servo servo)
        {
            try
            {
                chalkAndTalk("Enabling Torque");
                servo.WriteValue(RegisterType.TorqueEnable, 1);

                Console.WriteLine("Setting Position I Gain");
                servo.WriteValue(RegisterType.PositionIGain, 100);

                int max = servo.ReadValue(RegisterType.MaxPositionLimit);
                int min = servo.ReadValue(RegisterType.MinPositionLimit);
                int center = min + ((max - min) / 2);
                chalkAndTalk("\rMoving to center");
                MoveToPositionSync(servo, center);

                chalkAndTalk("\rMoving to maximum limit.");
                MoveToPositionSync(servo, max);

                chalkAndTalk("\rMoving to minimum limit.");
                MoveToPositionSync(servo, min);

                chalkAndTalk("\rDisabling Torque");
                servo.WriteValue(RegisterType.TorqueEnable, 0);
            }
            catch (Exception ex)
            {
                WriteLineWithColor("Couldn't achive goal: " + ex.Message, ConsoleColor.Red);
                Exit();
            }
        }

        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                WriteLineWithColor(String.Format("Usage: {0} <<port>> <<baud>>", System.AppDomain.CurrentDomain.FriendlyName), ConsoleColor.Red);
                Exit();
            }

            string portName = args[0];
            int baud = 0;
            if (! Int32.TryParse(args[1], out baud))
            {
                WriteLineWithColor("Couldn't parse command line arguments. Bailing.", ConsoleColor.Red);
                Exit();
            }

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
                WriteLineWithColor("Failed to find servos. Check wiring. Bailing.", ConsoleColor.Red);
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
            try
            {
                axis1Servo.WriteValue(RegisterType.TorqueEnable, 0, true);
                axis2Servo.WriteValue(RegisterType.TorqueEnable, 0, true);
            } catch (Exception ex)
            {
                WriteLineWithColor("Couldn't disable torque: " + ex.Message, ConsoleColor.Red);
                Exit();
            }

            int a1Center = 0;
            while (!getAxisCalibrationValue(axis1Servo, "Set Axis 1 to center position and press any key.", out a1Center)) { };

            int a1MaxLimit = 0;
            while (!getAxisCalibrationValue(axis1Servo, "Set Axis 1 to anticlockwise (left edge towards you) limit position and press any key.", out a1MaxLimit)) { };

            int a1MinLimit = a1MaxLimit - ((a1MaxLimit - a1Center) * 2);
            WriteLineWithColor(String.Format("Axis 1 minimum limit calculated as {0}.", a1MinLimit), ConsoleColor.Green);

            if (!storeToEEPROMAndReadback(axis1Servo, RegisterType.MaxPositionLimit, a1MaxLimit)) Exit();
            if (!storeToEEPROMAndReadback(axis1Servo, RegisterType.MinPositionLimit, a1MinLimit)) Exit();

            sweepLimits(axis1Servo);

            int a2Center = 0;
            while (!getAxisCalibrationValue(axis2Servo, "Set Axis 2 to center (weights down) position and press any key.", out a2Center)) { };

            int a2MinLimit = 0;
            while (!getAxisCalibrationValue(axis2Servo, "Set Axis 2 to minimum (weights towards you) limit and press any key.", out a2MinLimit)) { };

            int a2MaxLimit  = a2MinLimit - ((a2MinLimit - a2Center) * 2);
            WriteLineWithColor(String.Format("Axis 2 maximum limit calculated as {0}.", a2MaxLimit), ConsoleColor.Green);

            if (!storeToEEPROMAndReadback(axis2Servo, RegisterType.MinPositionLimit, a2MinLimit)) Exit();
            if (!storeToEEPROMAndReadback(axis2Servo, RegisterType.MaxPositionLimit, a2MaxLimit)) Exit();

            sweepLimits(axis2Servo);

            Console.ReadKey();
        }
    }
}
