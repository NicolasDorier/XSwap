using CommandLine;
using System;
using System.Collections.Generic;
using System.Text;

namespace XSwap.CLI
{
	[Verb("test", HelpText = "Test connection to RPC of a blockchain")]
	public class TestOptions
	{
		[Value(0, HelpText = "The name of the chain to test")]
		public string ChainName
		{
			get; set;
		}
	}

	[Verb("newkey", HelpText = "Create a public key")]
	public class NewOptions
	{
	}

	[Verb("take", HelpText = "Take an offer, this will always ask you confirmation before broadcasting anything")]
	public class TakeOptions
	{
		[Value(0, HelpText = "The offer")]
		public string Offer
		{
			get; set;
		}
	}

	[Verb("propose", HelpText = "Propose an offer")]
	public class ProposeOptions
	{
		[Value(0, HelpText = "Create an offer (example for offering 2 LTC against 1 BTC: 2LTC=>1BTC)")]
		public string Proposition
		{
			get; set;
		}

		[Value(1, HelpText = "Public key of the other party")]
		public string PubKey
		{
			get; set;
		}
	}

	[Verb("exit", HelpText = "Quit.")]
	public class QuitOptions
	{
		//normal options here
	}
}
