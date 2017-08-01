using NBitcoin.Tests;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using XSwap.CLI;

namespace XSwap.Tests
{
	public class XSwapUser : IDisposable
	{
		public XSwapUser(CoreNode a, CoreNode b, string directory)
		{
			Chain1 = a;
			Chain2 = b;
			_Repository = new Repository(directory);
		}

		private readonly Repository _Repository;

		public void CreateFacade()
		{
			var chains = new[]
			{
				new SupportedChain()
				{
					Information = new ChainInformation()
					{
						BlockTime = TimeSpan.FromMinutes(5.0),
						Names = new [] { "BTC1" },
						IsTest = true
					},
					RPCClient = Chain1.CreateRPCClient()
				},
				new SupportedChain()
				{
					Information = new ChainInformation()
					{
						BlockTime = TimeSpan.FromMinutes(10.0),
						Names = new [] { "BTC2" },
						IsTest = true
					},
					RPCClient = Chain2.CreateRPCClient()
				}
			};
			Facade = new SwapFacade(chains, _Repository);
			Interactive = new Interactive()
			{
				Repository = _Repository,
				Configuration = new CLI.Configuration.SwaperConfiguration()
				{
					SupportedChains = chains
				},
				AutoAccept = true
			};
		}

		public void Dispose()
		{
			_Repository.Dispose();
		}

		public Interactive Interactive
		{
			get; set;
		}

		public SwapFacade Facade
		{
			get; set;
		}

		public CoreNode Chain1
		{
			get; set;
		}
		public CoreNode Chain2
		{
			get; set;
		}
	}
    public class XSwapTester : IDisposable
    {
		private readonly string _Directory;
		private readonly NodeBuilder _Builder;

		public XSwapUser Alice
		{
			get;
			private set;
		}
		public XSwapUser Bob
		{
			get;
			private set;
		}

		public XSwapTester(string directory)
		{
			var rootTestData = "TestData";
			directory = rootTestData + "/" + directory;
			_Directory = directory;
			if(!Directory.Exists(rootTestData))
				Directory.CreateDirectory(rootTestData);

			if(!TryDelete(directory, false))
			{
				foreach(var process in Process.GetProcessesByName("bitcoind"))
				{
					if(process.MainModule.FileName.Replace("\\", "/").StartsWith(Path.GetFullPath(rootTestData).Replace("\\", "/"), StringComparison.Ordinal))
					{
						process.Kill();
						process.WaitForExit();
					}
				}
				TryDelete(directory, true);
			}

			Directory.CreateDirectory(directory);
			_Builder = NodeBuilder.Create(directory);

			Alice = new XSwapUser(_Builder.CreateNode(false), _Builder.CreateNode(false), Path.Combine(directory, "Alice"));
			Bob = new XSwapUser(_Builder.CreateNode(false), _Builder.CreateNode(false), Path.Combine(directory, "Bob"));

			_Builder.StartAll();

			Alice.CreateFacade();
			Bob.CreateFacade();

			Alice.Chain1.Sync(Bob.Chain1, true);
			Alice.Chain2.Sync(Bob.Chain2, true);

			Task.WaitAll(
			Alice.Chain1.CreateRPCClient().GenerateAsync(101),
			Bob.Chain2.CreateRPCClient().GenerateAsync(101));
		}

		private static bool TryDelete(string directory, bool throws)
		{
			try
			{
				XSwap.CLI.Utils.DeleteRecursivelyWithMagicDust(directory);
				return true;
			}
			catch(DirectoryNotFoundException)
			{
				return true;
			}
			catch(Exception)
			{
				if(throws)
					throw;
			}
			return false;
		}

		public static XSwapTester Create([CallerMemberNameAttribute]string caller = null)
		{
			return new XSwapTester(caller);
		}

		public void Dispose()
		{
			if(_Builder != null)
				_Builder.Dispose();
			if(Alice != null)
				Alice.Dispose();
			if(Bob != null)
				Bob.Dispose();
		}
	}
}
