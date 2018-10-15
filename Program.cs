using System;

namespace dotnetcoreDynamicJSON_RPC 
{
    class Program
    {
        static void Main(string[] args)
        {   
            // Example of use with bitcoind
            // ****************************
            // In this example we will query bitcoind and sum the vout values for each transaction in a block.
            // We'll be using regtest so we can create a block with know transactions in but you can skip that
            // part and just use a main or testnet block if you like. 

            // Example RPC credentials used by bitcoind and set in bitcoin.conf
            string rpcUrl = "http://127.0.0.1";
            string rpcPort = "8332";
            string rpcUsername = "yourrpcuser";
            string rpcPassword = "yourrpcpassword";

            // Initialise an instance of the dynamic class
            dynamic dynamicRPC = new dotnetcoreDynamicJSON_RPC(rpcUrl, rpcPort, rpcUsername, rpcPassword);

            try {
                // You can skip ahead to the line where we set the blockNumber if you are not using your own
                // regtest setup and are instead using bitcoin main or testnet.

                // Mine some blocks and mature the coinbase so we have funds to send.
                dynamicRPC.generate(101);

                // Get a new address so we can create a few transactions
                string newAddress = dynamicRPC.getnewaddress();
                // That will return results in a JSON formatted string similar to:
                // "{\"result\":\"2N2RH4toZArdRMjthBJc3x3sqpeF24yLs3G\",\"error\":null,\"id\":null}\n"
                // The value we want is held in the 'result' property.
                // There is a helper method that extends strings that we can use to get 'result' values from JSON strings
                // returned from bitcoind without needing to do any string manipulation. We'll use that to get the address value.
                newAddress = newAddress.GetResult();
                // newaddress now contains just the address as a string that we can use.

                // Send some bitcoin to the new address to create some transactions.
                dynamicRPC.sendtoaddress(newAddress, 13);
                dynamicRPC.sendtoaddress(newAddress, 7);

                // Mine a block with our new transactions in.
                dynamicRPC.generate(1);
                
                // Now we have a block we know contains transactions we can get the block and loop through each transaction
                // summing the vout amounts. You can enter your own block number here if you are not using regtest and the steps above.
                // Here we are using the GetResult helper again as the result we want is within JSON formatted data.
                string blockNumber = dynamicRPC.getblockcount();
                blockNumber = blockNumber.GetResult();

                // Get the block hash results. This will return a JSON formatted string.
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
                
                // Now we can use the 'GetStringList' string extension method to retrun the transactions as a List object. 
                // Within results stored in 'block' there is a JSON array of tx entries, we can access this using the notation "result.tx"
                var transactions = block.GetStringList("result.tx");
                
                // Iterate through each transaction id in the List of transactions so we can total the outputs
                decimal voutTotal = 0;

                foreach (var txid in transactions)
                {
                    string rawTransaction = dynamicRPC.getrawtransaction(txid);

                    // Use the 'GetProperty' string extension method to pull out a given property value
                    // We could use the GetResult extension method again but we'll demonstrate how GetProperty can be used:
                    // GetProperty allows you to pick any property from the JSON returned.
                    string transactionHex = rawTransaction.GetProperty("result"); 

                    string decodedTransaction = dynamicRPC.decoderawtransaction(transactionHex);
                    
                    // We can use the 'GetObjectList' string extension method to return complex objects
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

                //Use these to force an error:
                //string oops = dynamicRPC.thismethoddoesnotexist(1);
                //string oops = dynamicRPC.getblockhash("method exists but this parameter is wrong");                
            }
            catch (Exception e) {
                Console.WriteLine(e.Message);
            }
        }
    }
}