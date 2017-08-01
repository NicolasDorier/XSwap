using NBitcoin;
using NBitcoin.RPC;
using System;
using System.Collections.Generic;
using System.Text;

namespace XSwap.CLI
{
    public class ChainAsset
    {
		public ChainAsset()
		{

		}
		public ChainAsset(Money amount, string chain)
		{
			Amount = amount;
			Chain = chain.ToUpperInvariant();
		}
		public string Chain
		{
			get; set;
		}
		public Money Amount
		{
			get; set;
		}

		public override string ToString()
		{
			return Amount.ToString(false, true) + Chain.ToUpperInvariant();
		}
	}
}
