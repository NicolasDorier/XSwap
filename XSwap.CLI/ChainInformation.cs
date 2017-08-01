using System;
using System.Collections.Generic;
using System.Text;
using NBitcoin;
using NBitcoin.JsonConverters;
using System.Text.RegularExpressions;
using System.IO;

namespace XSwap.CLI
{
	public class ChainInformation
	{
		public TimeSpan BlockTime
		{
			get; set;
		}

		public string DefaultCookieFile
		{
			get; set;
		}

		public Uri DefaultRPCUrl
		{
			get; set;
		}

		public Network Network
		{
			get; set;
		}

		public string[] Names
		{
			get;
			set;
		}
		public bool IsTest
		{
			get;
			set;
		}

		public TimeSpan GetTimeSpan(int blockCount)
		{
			return new TimeSpan(BlockTime.Ticks * blockCount);
		}

		public int GetBlockCount(TimeSpan span)
		{
			return (int)Math.Round(((double)span.Ticks / BlockTime.Ticks), MidpointRounding.ToEven);
		}

		public string GetDefaultCookieFilePath()
		{
			var match = Regex.Match(DefaultCookieFile, "~/\\.([^/]*)/(.*)");
			if(!match.Success)
				return DefaultCookieFile;
			return Path.GetFullPath(Path.Combine(DefaultDataDirectory.GetDefaultDirectory(match.Groups[1].Value, false), match.Groups[2].Value));
		}
	}
}
