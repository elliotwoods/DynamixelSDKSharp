using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dispatcher.Requests.System
{
	class Census : IRequest
	{
		public object Perform()
		{
			var servos = PortPool.X.Servos;

			//check which indices are missing in continuous count
			var missingInidicesList = new List<int>();
			{
				var maxIndex = servos.Last().Key;
				for(int i=0; i<maxIndex; i++)
				{
					if(!servos.ContainsKey(i))
					{
						missingInidicesList.Add(i);
					}
				}
			}

			return new {
				count = servos.Count,
				missingIndices = missingInidicesList,
				allIndices = servos.Keys
			};
		}
	}
}
