using System;

namespace dotnetcoreDynamicJSON_RPC 
{
    class Program
    {
        static void Main(string[] args)
        {   
            // Example of use with bitcoind
            // ****************************
            // Before this example will work you will need to have bitcoind running on regtest!
            // In this example we will query bitcoind on regtest and sum the vout values for each transaction in a block.
            // We'll be using regtest so we can create a block with known transactions in but you can skip that
            // part and just use a main or testnet block if you like (go to the "string blockHash = " line if you do).
            
            // Example RPC credentials used by bitcoind (likely set in bitcoin.conf). Up to you how you load these.
            string rpcUrl = "http://127.0.0.1";
            string rpcPort = "8332";
            string rpcUsername = "yourrpcuser";
            string rpcPassword = "yourrpcpassword";

            // Initialise an instance of the dynamic class
            dynamic dynamicRPC = new dotnetcoreDynamicJSON_RPC(rpcUrl, rpcPort, rpcUsername, rpcPassword);

            try {
                // Mine some regtest blocks and mature the coinbase so we have funds to send.
                dynamicRPC.generate(101);

                // Get a new address so we can create a few transactions
                string newAddress = dynamicRPC.getnewaddress();
                //    That will return results in a JSON formatted string similar to:
                //    "{\"result\":\"2N2RH4toZArdRMjthBJc3x3sqpeF24yLs3G\",\"error\":null,\"id\":null}\n"
                
                // The value we want is held in the first 'result' property: "2N2RH...etc"
                // There is a helper method in dotnetcoreDynamicJSON_RPC that extends string so that we can get 'result' 
                // values from JSON strings without needing to do any string manipulation here. 
                // (There is another way to get values that are not 'result' data and we'll look at that later)
                // For now, we'll use GetResult to get the address value from the 'result' data.
                newAddress = newAddress.GetResult();
                // newaddress now contains just the address as a string that we can now use.

                // Send some bitcoin to the new address to create some transactions.
                dynamicRPC.sendtoaddress(newAddress, 13);
                dynamicRPC.sendtoaddress(newAddress, 7);

                // Mine a block with our transactions in.
                dynamicRPC.generate(1);
                
                // Now we have a block that we know contains transactions, we can get the block and loop through each transaction
                // summing the vout amounts. You can enter your own block number here if you are not using regtest and the steps above.
                // Here we will be using the GetResult string helper again as the result we want is within the 'result' JSON data.
                string blockNumber = dynamicRPC.getblockcount();
                blockNumber = blockNumber.GetResult();

                // Get the block hash results. This will again return a JSON formatted string.
                string blockHash = dynamicRPC.getblockhash(Convert.ToInt16(blockNumber));
                //  blockHash now has something like the following stored in it:
                //  "{\"result\":\"5895ee9dbb419645bc3a09969decf4929b572b9aa733c32e9d098d364a265e15\",\"error\":null,\"id\":null}\n"
                
                // We can pull the 'result' value out using the 'GetResult' string extension method again.
                blockHash = blockHash.GetResult();
                // blockHash will now have the value for the 'result' JSON property stored in it:
                //  "5895ee9dbb419645bc3a09969decf4929b572b9aa733c32e9d098d364a265e15"
   
                // We can now use this as a parameter for the 'getblock' command
                string block = dynamicRPC.getblock(blockHash);
                // Which returns something like:
                //  {"result":{"hash":"5895ee9dbb419645bc3a09969decf4929b572b9aa733c32e9d098d364a265e15","confirmations":1,... etc.
                
                // Now we will use the 'GetStringList' string extension method to return the transactions as a List object. 
                // We need to do this as within the results stored in 'block' there is a JSON array of tx entries that we want to access. 
                // We can navigate to this data using the notation "result.tx" as a parameter to GetStringList to tell it where to get its data
                var transactions = block.GetStringList("result.tx");
                
                // We'll need somewhere to save our running total
                decimal voutTotal = 0;
                
                // Iterate through each transaction id in the List of transactions so we can total the outputs
                foreach (var txid in transactions)
                {
                    string rawTransaction = dynamicRPC.getrawtransaction(txid);

                    // Now we will use the 'GetProperty' string extension method to pull out a given property value.
                    // We could use the GetResult extension method again but we'll demonstrate how GetProperty can be used:
                    // (GetProperty allows you to pick any property from the JSON returned)
                    string transactionHex = rawTransaction.GetProperty("result"); 

                    // Deconde the raw tx data by making another method call to the daemon
                    string decodedTransaction = dynamicRPC.decoderawtransaction(transactionHex);
                    
                    // No we'll use the other List based helper method - 'GetObjectList'
                    // This is used to return complex objects within JSON data using the same access notation as GetStringList
                    var vouts = decodedTransaction.GetObjectList("result.vout"); 

                    // Iterating through the returned vouts List we can sum the 'value' property of each vout
                    foreach (var vout in vouts)
                    {
                        string voutString = vout.ToString();
                        // Using the GetProperty helper again to get the data value we want
                        // (There is a GetValue helper that does what we need but we are demonstrating the use of GetProperty here)
                        string valueString = voutString.GetProperty("value");
                        decimal value = Convert.ToDecimal(valueString);
                        voutTotal += value;
                    }
                }

                //Use these to force an error at runtime (as the methods are only evaluated at runtime they will not
                // produce a compile time error, so test what you write ;-)
                //string oops = dynamicRPC.thismethoddoesnotexist(1);
                //string oops = dynamicRPC.getblockhash("method exists but this parameter is dodgy");                
            }
            catch (Exception e) {
                Console.WriteLine(e.Message);
            }
        }
    }
}
