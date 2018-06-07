using System;
using System.Collections.Generic;
using System.Linq;

using DynamixelSDKSharp;

namespace Dispatcher.Requests.Heliostat
{
    [RequestHandler(Method = Method.POST)]
    [Serializable]
    class Nudge : IRequest 
    {
        public int heliostatID { get; set; }

        public Nudge()
        {

        }

        public object Perform()
        {
            var hstat = Program.Heliostats.Where(h => h.ID == heliostatID).SingleOrDefault();
            if (hstat == null)
            {
                throw new Exception(String.Format("Heliostat Nudge: Couldn't find heliostat with ID {0}", heliostatID));
            }

            if (hstat.axis1Servo == null)
            {
                throw new Exception(String.Format("Heliostat Nudge: Couldn't find servo for axis 1"));
            }

            if (hstat.axis2Servo == null)
            {
                throw new Exception(String.Format("Heliostat Nudge: Couldn't find servo for axis 2"));
            }

            hstat.axis1Servo.WriteValue(RegisterType.GoalPosition, 2048);
            hstat.axis2Servo.WriteValue(RegisterType.GoalPosition, 2048);

            return new { };
        }
    }
}
