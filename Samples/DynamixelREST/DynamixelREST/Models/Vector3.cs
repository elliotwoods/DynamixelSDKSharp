﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamixelREST.Models
{
	//This exists because System.Numeric's vector 3 has uppercase x, y, z components and the HAM needs them to be lower case and I can't be bothered to write custom JsonConverter.
	//TODO: Fix.
	public class Vector3
	{
		public float x { get; set; }
		public float y { get; set; }
		public float z { get; set; }

        public Vector3()
        {

        }

        public Vector3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public void convertToUnit()
        {
            var magnitude = (float) Math.Sqrt((x * x) + (y * y) + (z * z));
            x /= magnitude;
            y /= magnitude;
            z /= magnitude;
        }
	}
}
