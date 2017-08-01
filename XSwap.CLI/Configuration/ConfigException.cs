using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace XSwap.CLI
{
	public class ConfigException : Exception
	{
		public ConfigException():base("")
		{

		}
		public ConfigException(string message) : base(message)
		{

		}
	}
}
