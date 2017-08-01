using NBitcoin;
using NBitcoin.JsonConverters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace XSwap.CLI
{
	public class OfferParty
	{
		public PubKey PubKey
		{
			get; set;
		}
		public ChainAsset Asset
		{
			get; set;
		}
	}

	public class OfferData
	{
		public OfferParty Initiator
		{
			get; set;
		}

		public OfferParty Taker
		{
			get; set;
		}

		public uint160 Hash
		{
			get; set;
		}

		public LockTime LockTime
		{
			get; set;
		}

		public LockTime CounterOfferLockTime
		{
			get; set;
		}

		public string ToString(bool pretty)
		{
			JsonSerializerSettings serializer = new JsonSerializerSettings();
			Serializer.RegisterFrontConverters(serializer, Network.Main); //Network does not matter in this case
			if(pretty)
				serializer.Formatting = Formatting.Indented;
			return JsonConvert.SerializeObject(this, serializer);
		}
		public override string ToString()
		{
			return ToString(true);
		}

		public OfferData CreateCounterOffer()
		{
			return new OfferData()
			{
				Initiator = Taker,
				Taker = Initiator,
				Hash = Hash,
				LockTime = CounterOfferLockTime,
				CounterOfferLockTime = LockTime
			};
		}

		public TxOut CreateOffer()
		{
			return new TxOut(Initiator.Asset.Amount, CreateScriptPubkey());
		}

		public Script TakeOffer(TransactionSignature takerSignature, Preimage preimage)
		{
			return new Script(new[] { Op.GetPushOp(takerSignature.ToBytes()), Op.GetPushOp(preimage.Bytes), Op.GetPushOp(CreateRedeemScript().ToBytes()) });
		}

		public Script RedeemOffer(TransactionSignature initiatorSignature)
		{
			return new Script(new[] { Op.GetPushOp(initiatorSignature.ToBytes()) });
		}

		public Script CreateScriptPubkey()
		{
			return CreateRedeemScript().Hash.ScriptPubKey;
		}

		public Script CreateRedeemScript()
		{
			List<Op> ops = new List<Op>();

			ops.Add(OpcodeType.OP_DEPTH);
			ops.Add(OpcodeType.OP_2);
			ops.Add(OpcodeType.OP_EQUAL);
			ops.Add(OpcodeType.OP_IF);
			{
				ops.Add(OpcodeType.OP_HASH160);
				ops.Add(Op.GetPushOp(Hash.ToBytes()));
				ops.Add(OpcodeType.OP_EQUALVERIFY);
				ops.Add(Op.GetPushOp(Taker.PubKey.ToBytes()));
			}
			ops.Add(OpcodeType.OP_ELSE);
			{
				ops.Add(Op.GetPushOp(LockTime));
				ops.Add(OpcodeType.OP_CHECKLOCKTIMEVERIFY);
				ops.Add(Op.GetPushOp(Initiator.PubKey.ToBytes()));
			}
			ops.Add(OpcodeType.OP_ENDIF);
			ops.Add(OpcodeType.OP_CHECKSIG);
			return new Script(ops.ToArray());
		}

		internal static OfferData Parse(string offer)
		{
			return Serializer.ToObject<OfferData>(offer);
		}
	}
}
