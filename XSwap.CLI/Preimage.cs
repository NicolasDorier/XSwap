using NBitcoin;
using NBitcoin.Crypto;
using System;
using System.Collections.Generic;
using System.Text;

namespace XSwap.CLI
{
	public class Preimage
	{
		public Preimage()
		{
			Bytes = RandomUtils.GetBytes(32);
		}
		public Preimage(byte[] bytes)
		{
			Bytes = bytes;
		}

		public byte[] Bytes
		{
			get; set;
		}

		public uint160 GetHash()
		{
			return new uint160(Hashes.Hash160(Bytes, Bytes.Length));
		}
	}
}
