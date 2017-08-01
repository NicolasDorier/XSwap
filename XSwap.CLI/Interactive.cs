using CommandLine;
using System.Linq;
using System;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using XSwap.CLI.Configuration;
using XSwap.CLI.Logging;
using NBitcoin;
using NBitcoin.DataEncoders;
using System.IO;
using System.Threading.Tasks;

namespace XSwap.CLI
{
	public class Interactive
	{
		public SwaperConfiguration Configuration
		{
			get;
			set;
		}
		public Repository Repository
		{
			get;
			set;
		}

		internal void Run()
		{
			Parser.Default.ParseArguments<QuitOptions, NewOptions, TakeOptions, ProposeOptions, TestOptions>(new[] { "help" });

			int bufSize = 1024;
			Stream inStream = Console.OpenStandardInput();
			Console.SetIn(new StreamReader(inStream, Console.InputEncoding, false, bufSize));

			while(!Quit)
			{
				Thread.Sleep(100);
				Console.Write(">>> ");
				var split = Console.ReadLine().Split(null);
				try
				{
					Process(split);
				}
				catch(FormatException ex)
				{
					Console.WriteLine("Invalid format");
					Console.WriteLine(ex.Message);
					Parser.Default.ParseArguments<QuitOptions, NewOptions,TakeOptions, ProposeOptions, TestOptions>(new[] { "help", split[0] });
				}
			}
		}
		public void Process(string cmd)
		{
			Process(cmd.Split(null));
		}
		public void Process(string[] split)
		{
			Parser.Default.ParseArguments<QuitOptions, NewOptions, TakeOptions, ProposeOptions, TestOptions>(split)
									.WithParsed<TestOptions>(_ => Test(_))
									.WithParsed<NewOptions>(_ => New(_))
									.WithParsed<TakeOptions>(_ => Take(_).GetAwaiter().GetResult())
									.WithParsed<ProposeOptions>(_ => Propose(_).GetAwaiter().GetResult())
									.WithParsed<QuitOptions>(_ =>
									{
										Quit = true;
									});
		}

		private async Task Take(TakeOptions o)
		{
			if(o.Offer == null)
				throw new FormatException();

			var offer = OfferData.Parse(o.Offer);
			var counterOffer = offer.CreateCounterOffer();
			if(Repository.GetPrivateKey(offer.Taker.PubKey) == null)
			{
				Console.WriteLine("This offer does not use our pubkey");
				return;
			}

			//TODO need to check the timelocks
			if(!AutoAccept)
			{
				Console.WriteLine($"Do you accept to exchange {counterOffer.Initiator.Asset} against {counterOffer.Taker.Asset}? (yes to accept)");
				while(true)
				{
					var line = Console.ReadLine();
					if(line.Equals("yes", StringComparison.OrdinalIgnoreCase))
						break;
				}
			}

			var swapper = CreateSwapper();
			var waitingOffer = swapper.WaitOfferAsync(offer);
			var waitingCounterOfferTaken = swapper.WaitOfferTakenAsync(counterOffer);
			var txId = await swapper.BroadcastOffer(counterOffer, true).ConfigureAwait(false);
			Console.WriteLine("Counter offer broadcasted " + txId);
			Console.WriteLine("Waiting the offer to be broadcasted...");
			txId = await waitingOffer.ConfigureAwait(false);
			Console.WriteLine("Offer broadcasted " + txId);

			Console.WriteLine("Waiting the counter offer to be taken...");
			blocked.Set();
			await waitingCounterOfferTaken.ConfigureAwait(false);
			blocked.Reset();
			Console.WriteLine("Counter offer taken by the other party");
			txId = await swapper.TakeOffer(offer).ConfigureAwait(false);
			Console.WriteLine("Offer taken");
			Console.WriteLine("Exchange complete");
		}

		public bool Quit
		{
			get;
			private set;
		}

		public string DataToTransfer
		{
			get;
			private set;
		}
		public bool AutoAccept
		{
			get;
			set;
		}

		ManualResetEvent blocked = new ManualResetEvent(false);

		public void WaitBlocked()
		{
			blocked.WaitOne();
		}

		private void New(NewOptions o)
		{
			var pubKey = CreateSwapper().NewPubkey();
			Console.WriteLine("Transfer this pubkey to the other party:");
			Console.WriteLine(Separator);
			DataToTransfer = pubKey.ToHex();
			Console.WriteLine(DataToTransfer);
			Console.WriteLine(Separator);
		}

		private async Task Propose(ProposeOptions o)
		{
			if(o.Proposition == null)
				throw new FormatException();

			var proposal = Proposal.Parse(o.Proposition);
			GetSupportedChain(proposal.From.Chain).EnsureIsSetup();
			GetSupportedChain(proposal.To.Chain).EnsureIsSetup();

			var key = new PubKey(o.PubKey ?? "");
			var swapper = CreateSwapper();
			var offerData = await swapper.ProposeOffer(proposal, key).ConfigureAwait(false);

			Console.WriteLine("Transfer this offer to the other party:");
			Console.WriteLine(Separator);
			DataToTransfer = offerData.ToString(false);
			Console.WriteLine(DataToTransfer);
			Console.WriteLine(Separator);

			Console.WriteLine();
			Console.WriteLine("Waiting for the other party to broadcast the counter offer...");
			Console.WriteLine();

			var counterOffer = offerData.CreateCounterOffer();
			blocked.Set();
			var txId = await swapper.WaitOfferAsync(counterOffer).ConfigureAwait(false);
			blocked.Reset();
			Console.WriteLine("Counter Offer broadcasted by the other party " + txId);

			txId = await swapper.BroadcastOffer(offerData, false).ConfigureAwait(false);
			Console.WriteLine("Offer broadcasted by us " + txId);

			Console.WriteLine();
			Console.WriteLine("Waiting confirmation of the offer and counter offer...");
			Console.WriteLine();

			var offerConfirmation = swapper.WaitOfferConfirmationAsync(offerData).ContinueWith(oo =>
			{
				Console.WriteLine("Offer confirmed");
			});
			var counterOfferConfirmation = swapper.WaitOfferConfirmationAsync(counterOffer).ContinueWith(oo =>
			{
				Console.WriteLine("Counter Offer confirmed");
			});
			blocked.Set();
			await Task.WhenAll(offerConfirmation, counterOfferConfirmation).ConfigureAwait(false);
			blocked.Reset();

			txId = await swapper.TakeOffer(counterOffer).ConfigureAwait(false);
			Console.WriteLine("Counter offer taken " + txId);
			Console.WriteLine();
			Console.WriteLine("Exchange complete!");
		}

		const string Separator = "---------------------------";
		private SwapFacade CreateSwapper()
		{
			return new SwapFacade(Configuration.SupportedChains, Repository);
		}

		private bool Test(TestOptions o)
		{
			var chain = GetSupportedChain(o.ChainName);
			try
			{
				chain.EnsureIsSetup(true);
				return true;
			}
			catch(ConfigException ex)
			{
				if(!string.IsNullOrEmpty(ex.Message))
					Logs.Configuration.LogError(ex.Message);
				return false;
			}
		}

		private SupportedChain GetSupportedChain(string chainName)
		{
			var chain = chainName == null ? null : Configuration.SupportedChains.FirstOrDefault(c => c.Information.Names.Contains(chainName, StringComparer.OrdinalIgnoreCase));
			if(chain == null)
			{
				var allNames = String.Join(",", Configuration.SupportedChains
					.Select(c => c.Information.Names[0])
					.ToArray());
				Console.WriteLine("Specify the name of the RPC chain you want to test among those " + allNames);
				throw new FormatException();
			}
			return chain;
		}
	}
}
