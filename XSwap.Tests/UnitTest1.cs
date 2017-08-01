using NBitcoin;
using System;
using System.Threading.Tasks;
using XSwap.CLI;
using Xunit;

namespace XSwap.Tests
{
	public class UnitTest1
	{
		/// <summary>
		/// Alice sending 1.0 BTC1 to Bob against 2.0 BTC2 using Tier Nolan protocol
		/// </summary>
		[Fact]
		public void Test1()
		{

			using(var tester = XSwapTester.Create())
			{
				var pubKey = tester.Bob.Facade.NewPubkey();

				var offer = tester.Alice.Facade.ProposeOffer(Proposal.Parse("1BTC1=>2BTC2"), pubKey).Result;

				var waitingOffer = tester.Alice.Facade.WaitOfferAsync(offer);
				tester.Alice.Facade.BroadcastOffer(offer, false).Wait();
				waitingOffer.Wait();

				tester.Bob.Facade.BroadcastOffer(offer.CreateCounterOffer(), true).Wait();

				var bobWait =
					Task.WhenAll
					(tester.Bob.Facade.WaitOfferConfirmationAsync(offer.CreateCounterOffer()),
					 tester.Bob.Facade.WaitOfferConfirmationAsync(offer));

				var aliceWait =
					Task.WhenAll
					(tester.Alice.Facade.WaitOfferConfirmationAsync(offer.CreateCounterOffer()),
					 tester.Alice.Facade.WaitOfferConfirmationAsync(offer));

				tester.Bob.Chain2.CreateRPCClient().Generate(1);
				tester.Alice.Chain1.CreateRPCClient().Generate(1);

				aliceWait.Wait();
				bobWait.Wait();

				var waitingTake = tester.Bob.Facade.WaitOfferTakenAsync(offer.CreateCounterOffer());

				tester.Alice.Facade.TakeOffer(offer.CreateCounterOffer()).Wait();

				waitingTake.Wait();
				tester.Bob.Facade.TakeOffer(offer).Wait();

				var bob = tester.Bob.Chain1.CreateRPCClient().ListUnspent(0, 0);
				var alice = tester.Alice.Chain2.CreateRPCClient().ListUnspent(0, 0);
				Assert.Equal(1, bob.Length);
				Assert.Equal(1, alice.Length);
			}
		}

		[Fact]
		public void TestCommandLineFlow()
		{

			using(var tester = XSwapTester.Create())
			{
				tester.Bob.Interactive.Process("newkey");
				var proposing = Task.Run(()=> tester.Alice.Interactive.Process($"propose 1BTC1=>2BTC2 {tester.Bob.Interactive.DataToTransfer}"));
				//Alice waits the counter offer
				tester.Alice.Interactive.WaitBlocked();
				var taking = Task.Run(() => tester.Bob.Interactive.Process($"take {tester.Alice.Interactive.DataToTransfer}"));

				//Bob and Alice wait for new blocks
				tester.Bob.Interactive.WaitBlocked();
				tester.Alice.Interactive.WaitBlocked();
				tester.Alice.Chain1.CreateRPCClient().Generate(1);
				tester.Bob.Chain2.CreateRPCClient().Generate(1);

				//Done
				proposing.Wait();
				taking.Wait();
			}
		}

		[Fact]
		public void CanParseProposal()
		{
			var a = Proposal.Parse("1.0ABC=>2.0DEf");
			var b = Proposal.Parse("1.0ABC=>2.0DEF");
			var c = Proposal.Parse("1ABC=>2DEF");
			Assert.Equal("1.00ABC=>2.00DEF", a.ToString());
			Assert.Equal(a.ToString(), b.ToString());
			Assert.Equal(b.ToString(), c.ToString());
			a = Proposal.Parse("1.0ABC1=>2.0DEf2");
			Assert.Equal("1.00ABC1=>2.00DEF2", a.ToString());
			Assert.Throws<FormatException>(() => Proposal.Parse("ABC=>DEF"));
			Assert.Throws<FormatException>(() => Proposal.Parse("1.0=>1.0"));
		}
	}
}
