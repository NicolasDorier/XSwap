using NBitcoin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace XSwap.CLI
{
	public class KnownChains
	{
		public static ChainInformation GetByName(string name)
		{
			return Enumerate()
				.Where(c => c.Names.Contains(name, StringComparer.OrdinalIgnoreCase))
				.FirstOrDefault();
		}

		public static IEnumerable<ChainInformation> Enumerate()
		{
			yield return Bitcoin;
			yield return BitcoinTest;
			//yield return Litecoin;
			//yield return LitecoinTest;
			yield return BitcoinA;
			yield return BitcoinB;
		}

		public static ChainInformation Bitcoin
		{
			get;
			set;
		} = new ChainInformation()
		{
			BlockTime = TimeSpan.FromMinutes(10.0),
			DefaultCookieFile = "~/.bitcoin/.cookie",
			DefaultRPCUrl = new Uri("http://localhost:8332/"),
			Network = Network.Main,
			Names = new[] { "BTC" }
		};

		public static ChainInformation BitcoinA
		{
			get;
			set;
		} = new ChainInformation()
		{
			BlockTime = TimeSpan.FromMinutes(10.0),
			DefaultCredential = new NetworkCredential("user", "pwd"),
			DefaultRPCUrl = new Uri("http://localhost:46273/"),
			Network = Network.RegTest,
			Names = new[] { "BTCA" },
			IsTest = true
		};

		public static ChainInformation BitcoinB
		{
			get;
			set;
		} = new ChainInformation()
		{
			BlockTime = TimeSpan.FromMinutes(10.0),
			DefaultCredential = new NetworkCredential("user", "pwd"),
			DefaultRPCUrl = new Uri("http://localhost:46274/"),
			Network = Network.RegTest,
			Names = new[] { "BTCB" },
			IsTest = true
		};

		public static ChainInformation BitcoinTest
		{
			get;
			set;
		} = new ChainInformation()
		{
			BlockTime = TimeSpan.FromMinutes(10.0),
			DefaultCookieFile = "~/.bitcoin/testnet3/.cookie",
			DefaultRPCUrl = new Uri("http://localhost:18332/"),
			Network = Network.TestNet,
			Names = new[] { "TBTC" },
			IsTest = true
		};

		//public static ChainInformation Litecoin
		//{
		//	get;
		//	set;
		//} = new ChainInformation()
		//{
		//	BlockTime = TimeSpan.FromMinutes(2.5),
		//	DefaultCookieFile = "~/.litecoin/.cookie",
		//	DefaultRPCUrl = new Uri("http://localhost:9332/"),
		//	Network = NBitcoin.Litecoin.Networks.Mainnet,
		//	Names = new[] { "LTC" }
		//};

		//public static ChainInformation LitecoinTest
		//{
		//	get;
		//	set;
		//} = new ChainInformation()
		//{
		//	BlockTime = TimeSpan.FromMinutes(2.5),
		//	DefaultCookieFile = "~/.litecoin/testnet3/.cookie",
		//	DefaultRPCUrl = new Uri("http://localhost:19332/"),
		//	Network = NBitcoin.Litecoin.Networks.Testnet,
		//	Names = new[] { "TLTC" },
		//	IsTest = true
		//};
	}
}
