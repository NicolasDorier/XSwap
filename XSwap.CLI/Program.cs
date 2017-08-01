using System;
using System.IO;
using Microsoft.Extensions.Logging;
using XSwap.CLI.Configuration;
using XSwap.CLI.Logging;

namespace XSwap.CLI
{
	class Program
	{
		static void Main(string[] args)
		{
			Logs.Configure(new FuncLoggerFactory(i => new CustomerConsoleLogger(i, (a, b) => true, false)));
			try
			{

				var config = new SwaperConfiguration();
				config.LoadArgs(args);

				using(var repo = new Repository(Path.Combine(config.DataDir, "db")))
				{
					new Interactive() { Configuration = config, Repository = repo }.Run();
				}
			}
			catch(ConfigException ex)
			{
				if(!string.IsNullOrEmpty(ex.Message))
					Logs.Configuration.LogError(ex.Message);
			}
			catch(Exception ex)
			{
				Logs.Configuration.LogError(ex.Message);
				Logs.Configuration.LogDebug(ex.StackTrace);
			}
		}
	}
}