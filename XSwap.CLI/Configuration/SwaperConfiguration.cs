using System;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.IO;
using NBitcoin;
using System.Net;
using XSwap.CLI.Logging;

namespace XSwap.CLI.Configuration
{
	public class SwaperConfiguration
	{
		public string ConfigurationFile
		{
			get;
			private set;
		}
		public string DataDir
		{
			get;
			private set;
		}

		internal void LoadArgs(string[] args)
		{
			ConfigurationFile = args.Where(a => a.StartsWith("-conf=", StringComparison.Ordinal)).Select(a => a.Substring("-conf=".Length).Replace("\"", "")).FirstOrDefault();
			DataDir = args.Where(a => a.StartsWith("-datadir=", StringComparison.Ordinal)).Select(a => a.Substring("-datadir=".Length).Replace("\"", "")).FirstOrDefault();
			if(DataDir != null && ConfigurationFile != null)
			{
				var isRelativePath = Path.GetFullPath(ConfigurationFile).Length > ConfigurationFile.Length;
				if(isRelativePath)
				{
					ConfigurationFile = Path.Combine(DataDir, ConfigurationFile);
				}
			}

			if(ConfigurationFile != null)
				AssetConfigFileExists();

			if(DataDir == null)
			{
				DataDir = DefaultDataDirectory.GetDefaultDirectory("XSwap");
			}

			if(ConfigurationFile == null)
			{
				ConfigurationFile = GetDefaultConfigurationFile(DataDir);
			}

			Logs.Configuration.LogInformation("Data directory set to " + DataDir);
			Logs.Configuration.LogInformation("Configuration file set to " + ConfigurationFile);

			if(!Directory.Exists(DataDir))
				throw new ConfigException("Data directory does not exists");


			var consoleConfig = new TextFileConfiguration(args);
			var config = TextFileConfiguration.Parse(File.ReadAllText(ConfigurationFile));
			consoleConfig.MergeInto(config, true);

			SupportedChains = KnownChains.Enumerate().Select(c => new SupportedChain(RPCArgs.Parse(config, c), c)).ToArray();
			
		}

		public SupportedChain[] SupportedChains
		{
			get; set;
		}

		private void AssetConfigFileExists()
		{
			if(!File.Exists(ConfigurationFile))
				throw new ConfigException("Configuration file does not exists");
		}

		public static string GetDefaultConfigurationFile(string dataDirectory)
		{
			var config = Path.Combine(dataDirectory, "swapper.config");
			Logs.Configuration.LogInformation("Configuration file set to " + config);
			if(!File.Exists(config))
			{
				Logs.Configuration.LogInformation("Creating configuration file");
				StringBuilder builder = new StringBuilder();
				builder.AppendLine("#You can customize your RPC settings to the supported currencies here");

				builder.AppendLine();
				foreach(var currency in KnownChains.Enumerate())
				{
					var name = currency.Names[0].ToLowerInvariant();
					builder.AppendLine("#" + name + ".rpc.url=" + currency.DefaultRPCUrl.AbsoluteUri);
					var creds = currency.DefaultCredential ?? new NetworkCredential("user", "password");
					builder.AppendLine("#" + name + ".rpc.user=" + creds.UserName);
					builder.AppendLine("#" + name + ".rpc.password=" + creds.Password);
					builder.AppendLine("#" + name + ".rpc.cookiefile=" + currency.GetDefaultCookieFilePath());
					builder.AppendLine();
				}

				File.WriteAllText(config, builder.ToString());
			}
			return config;
		}
	}
}
