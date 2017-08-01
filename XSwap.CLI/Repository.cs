using System;
using System.Collections.Generic;
using System.Text;
using NBitcoin;

namespace XSwap.CLI
{
	public class Repository : IDisposable
	{
		DBreezeRepository _Repo;
		public Repository(string directory)
		{
			if(directory == null)
				throw new ArgumentNullException(nameof(directory));
			_Repo = new DBreezeRepository(directory);
		}

		internal void SaveKey(Key key)
		{
			_Repo.UpdateOrInsert<Key>("1", key.PubKey.Hash.ToString(), key, (o, n) => n);
		}

		internal Key GetPrivateKey(PubKey key)
		{
			return _Repo.Get<Key>("1", key.Hash.ToString());
		}

		internal void SavePreimage(Preimage preimage)
		{
			_Repo.UpdateOrInsert("1", preimage.GetHash().ToString(), preimage.Bytes, (o, n) => n);
		}

		public Preimage GetPreimage(uint160 hash)
		{
			return new Preimage(_Repo.Get<byte[]>("1", hash.ToString()));
		}

		public void SaveOffer(Script scriptPubKey, OutPoint outpoint)
		{
			_Repo.UpdateOrInsert("1", scriptPubKey.Hash.ToString(), outpoint, (o, n) => n);
		}

		public OutPoint GetOffer(Script scriptPubKey)
		{
			return _Repo.Get<OutPoint>("1", scriptPubKey.Hash.ToString());
		}

		public void Dispose()
		{
			_Repo.Dispose();
		}
	}
}
