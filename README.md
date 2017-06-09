# XSwap

A utility for making trustless, peer to peer, cross chain exchange.

# How to use ?

Alice want to send 1 BTC  to Bob against 2 LTC

```
# Alice asks pubkey to Bob

# Bob run
dotnet run new key
>> <BobPubKey>

# Alice run 
dotnet run new offer 1.0 btc 2.0 ltc <BobPubKey>
>> <BobPubKey><AlicePubkey><Hash>

# Bob run
dotnet run take offer 2.0 ltc 1.0 btc <BobPubKey><AlicePubkey><Hash>
>> Waiting BTC Escrow...

					# Alice run
					dotnet run broadcast offer <BobPubKey><AlicePubkey><Hash>
					>> BTC Escrow broadcasted
>> BTC Escrow broadcasted
>> Waiting confirmation...
					>> Waiting confirmation...
					>> BTC Escrow Confirmed
>> BTC Escrow Confirmed					
					>> Waiting LTC Escrow
>> LTC Escrow broadcasted
					>> LTC Cashout broadcasted
>> LTC Cashout broadcasted
>> BTC Cashout broadcasted
					>> BTC Cashout broadcasted
					>> Waiting LTC Cashout confirmation....
					>> LTC Cashout confirmed
>> Waiting BTC Cashout confirmation....
>> BTC Cashout confirmed
```
