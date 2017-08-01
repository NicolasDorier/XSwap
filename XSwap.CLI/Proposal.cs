using NBitcoin;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace XSwap.CLI
{
	public class Proposal
	{
		public ChainAsset From
		{
			get;
			private set;
		}
		public ChainAsset To
		{
			get;
			private set;
		}

		public static Proposal Parse(string str)
		{
			str = str.Trim();
			var match = Regex.Match(str, "([0-9]+(\\.[0-9]+)?)([A-Za-z][A-Za-z0-9]+)=>([0-9]+(\\.[0-9]+)?)([A-Za-z][A-Za-z0-9]+)");
			if(!match.Success)
				throw new FormatException("Invalid proposal format (Valid example: 2LTC=>1BTC)");

			var amountA = Money.Coins(decimal.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture));
			var chainA = match.Groups[3].Value.ToUpperInvariant();
			var amountB = Money.Coins(decimal.Parse(match.Groups[4].Value, CultureInfo.InvariantCulture));
			var chainB = match.Groups[6].Value.ToUpperInvariant();
			
			return new Proposal()
			{
				From = new ChainAsset(amountA, chainA),
				To = new ChainAsset(amountB, chainB),
			};
		}

		public override string ToString()
		{
			return From.ToString() + "=>" + To.ToString();
		}
	}
}
