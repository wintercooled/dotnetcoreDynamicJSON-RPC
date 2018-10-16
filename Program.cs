using System;

namespace dotnetcoreDynamicJSON_RPC 
{
    class Program
    {
        static void Main(string[] args)
        {   
            // Example of use with bitcoind
            // ****************************
            // Before this example will work you will need to have bitcoind [running in regtest mode](https://bitcoin.org/en/developer-examples#regtest-mode).
            // 
            // We'll be using regtest so we can create a block with known transactions in it and then query bitcoind 
            // and sum the vout values for each transaction in the block.
            // 
            // We'll demonstrate how simple it is to call methods on bitcoind using dotnetcoreDynamicJSON_RPC and also
            // take a look at how its 3 string extension methods let us access the JSON data returned without the need 
            // for complex classes to represent a block, transaction etc.
            
            // Set up your own RPC credentials as used by your own bitcoind instance (likely set in bitcoin.conf). 
            // Up to you how you load these, we'll just hard code them here for now.
            string rpcUrl = "http://127.0.0.1";
            string rpcPort = "8332";
            string rpcUsername = "yourrpcuser";
            string rpcPassword = "yourrpcpassword";

            // Initialise an instance of the dynamic dotnetcoreDynamicJSON_RPC class
            dynamic dynamicRPC = new dotnetcoreDynamicJSON_RPC(rpcUrl, rpcPort, rpcUsername, rpcPassword);

            try {
                // Mine some regtest blocks and mature the coinbase so we have funds to send.
                dynamicRPC.generate(101);

                // Get a new address so we can create a few transactions
                string newAddress = dynamicRPC.getnewaddress();
                
                // That call to getnewaddress will have return results in a JSON formatted string similar to this:
                // "{\"result\":\"2N2RH4toZArdRMjthBJc3x3sqpeF24yLs3G\",\"error\":null,\"id\":null}\n"
                
                // The value we want ("2N2RH...etc") is held in the 'result' property of the JSON.
                // dotnetcoreDynamicJSON_RPC contains 3 helper methods (string extensions in actual fact) that let you get 
                // easy access to values within JSON data so you don't need to do any string manipulation in your code. They are:
                // GetProperty
                // GetStringList
                // GetObjectList
                // Each of these accept a 'path' parameter which lets you specify the location of the data within the JSON you are after.
                // We'll look at the other two later but for now we'll use GetProperty to pull out the value of the 'result' property.
                newAddress = newAddress.GetProperty("result");
                // newaddress now contains just the address as a string that we can now use.

                // Send some bitcoin to the new address to create some transactions.
                dynamicRPC.sendtoaddress(newAddress, 13);
                dynamicRPC.sendtoaddress(newAddress, 7);

                // Mine a block with our transactions in.
                dynamicRPC.generate(1);
                
                // Now we have a block that we know contains transactions, we can get the block and loop through each transaction
                // summing the vout amounts.
                string blockNumber = dynamicRPC.getblockcount();
                blockNumber = blockNumber.GetProperty("result");

                // Get the block hash results. This will again return a JSON formatted string.
                // Note here that we have to pass the correct type in to the getblockhash method or bitcoind will throw an error.
                string blockHash = dynamicRPC.getblockhash(Convert.ToInt16(blockNumber));
                //  blockHash now has something like the following stored in it:
                //  "{\"result\":\"5895ee9dbb419645bc3a09969decf4929b572b9aa733c32e9d098d364a265e15\",\"error\":null,\"id\":null}\n"
                
                blockHash = blockHash.GetProperty("result");
                // blockHash will now have the result value in it:
                //  "5895ee9dbb419645bc3a09969decf4929b572b9aa733c32e9d098d364a265e15"
   
                // We can now use this as a parameter for the getblock command
                string block = dynamicRPC.getblock(blockHash);
                // Which returns something like:
                //  {"result":{"hash":"4088c837fc10d2cd2e6c35312c5e239fb6c437d91109eae2cbd0bbd46ed84f72","confirmations":1,
                //   "strippedsize":552,"size":697,"weight":2353,"height":102, ..." etc

                // Now we will use the GetStringList string extension method to return the transactions as a List of objects. 
                // We need to do this as within the results stored in 'block' there is a JSON array of tx entries that we want to access. 
                // We can navigate to this data using the notation "result.tx" as a parameter to GetStringList to tell it where to get its data.
                var transactions = block.GetStringList("result.tx");
                
                // We'll need somewhere to save our running total
                decimal voutTotal = 0;
                
                // Iterate through each transaction id in the List of transaction objects so we can total the outputs
                foreach (var txid in transactions)
                {
                    string rawTransaction = dynamicRPC.getrawtransaction(txid);
    
                    string transactionHex = rawTransaction.GetProperty("result"); 

                    // Deconde the raw tx data by making another method call to the daemon
                    string decodedTransaction = dynamicRPC.decoderawtransaction(transactionHex);
                    
                    // Now we'll use the other List based extension method GetObjectList
                    // This is used to return complex objects within JSON data and uses the same access notation as GetProperty and GetStringList
                    var vouts = decodedTransaction.GetObjectList("result.vout"); 

                    // Iterating through the returned vouts List we can sum the 'value' property of each vout
                    foreach (var vout in vouts)
                    {
                        string voutString = vout.ToString();
                        string valueString = voutString.GetProperty("value");
                        decimal value = Convert.ToDecimal(valueString);
                        voutTotal += value;
                    }
                }
                
                Console.WriteLine("Total sum of vouts in block " + blockNumber + ": " + Convert.ToString(voutTotal));

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
