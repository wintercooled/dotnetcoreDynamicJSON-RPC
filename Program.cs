using System;
using System.Linq; //Only needed if you are using Linq directly in your project code
using Newtonsoft.Json.Linq; //Only needed if you are using Linq directly in your project code

namespace dotnetcoreDynamicJSON_RPC 
{
    class Program
    {
        static void Main(string[] args)
        {   
            // Example of use with bitcoind or elementsd
            // ******************************************
            // Before this example will work you will need to have bitcoind or elementsd running in regtest mode.
            //
            // If you are using Bitcoin as the target daemon see: https://bitcoin.org/en/developer-examples#regtest-mode
            // 
            // If you are using Elements, please refer to the tutorial on https://elementsproject.org for details
            // of how to set up Elements. The tutorial also covers how to use Confidential Transactions and Asset Issuance etc. 
            // Note: Because Elements uses Confidential Transactions the vout total we calculate will actually just sum the 
            // total of the fees paid by the transactions, the other values will be blinded.
            //
            // In the code below we will be using regtest so we can create a block with known transactions in it and then query the daemon 
            // and sum the vout values for each transaction in the block.
            // 
            // We'll demonstrate how simple it is to call methods on the daemon using dotnetcoreDynamicJSON_RPC and also
            // take a look at how its 3 string extension methods let us access the JSON data returned without the need 
            // for complex classes to represent a block, transaction etc.
            //
            // Note: Where example return values are shown below they have been taken from bitcoind responses, but will be similar for elementsd.
            
            // Set up your own RPC credentials as used by your own bitcoind or elementsd instance (likely set in bitcoin.conf/elements.conf). 
            // It's up to you how you load these, we'll just hard code them here for now.
            
            string rpcUrl = "http://127.0.0.1";
            string rpcPort = "8332";
            string rpcUsername = "yourrpcuser";
            string rpcPassword = "yourrpcpassword";
            
            // Initialise an instance of the dynamic dotnetcoreDynamicJSON_RPC class.
            dynamic dynamicRPC = new dotnetcoreDynamicJSON_RPC(rpcUrl, rpcPort, rpcUsername, rpcPassword);

            if (dynamicRPC.DaemonIsRunning())
                try {
                    // Mine some regtest blocks and mature the coinbase so we have funds to send.
                    dynamicRPC.generate(101);

                    // Get a new address so we can create a few transactions.
                    string newAddress = dynamicRPC.getnewaddress();
                    // That call to getnewaddress will have return results in a JSON formatted string similar to this:
                    // "{\"result\":\"2N2RH4toZArdRMjthBJc3x3sqpeF24yLs3G\",\"error\":null,\"id\":null}\n"
                    
                    // The value we want (the address, "2N2RH...etc") is held in the 'result' property of the JSON.
                    // dotnetcoreDynamicJSON_RPC contains 3 helper methods (string extensions in actual fact) that let you get 
                    // easy access to values within JSON data so you don't need to do any string manipulation in your code. They are:
                    // GetProperty
                    // GetStringList
                    // GetObjectList
                    // Each of these accept a 'path' parameter which lets you specify the location of the data within the JSON you want back.
                    // We'll look at the other two later and for now we'll use GetProperty to pull out the value of the 'result' property.
                    newAddress = newAddress.GetProperty("result");
                    // newaddress now contains the address itself as a string that we can now use.

                    // Send some bitcoin to the new address to create some transactions.
                    dynamicRPC.sendtoaddress(newAddress, 13);
                    dynamicRPC.sendtoaddress(newAddress, 7);

                    // Mine a block with our transactions in.
                    dynamicRPC.generate(1);
                    
                    // Now we have a block that we know contains transactions, we can get the block and then loop through each transaction
                    // summing the vout amounts. First we need to get the right block data. We need the last block mined.
                    string blockNumber = dynamicRPC.getblockcount();
                    blockNumber = blockNumber.GetProperty("result");

                    // Get the block hash results.
                    // Note here that we have to pass the correct type in to the getblockhash method or the daemon will throw an error.
                    string blockHash = dynamicRPC.getblockhash(Convert.ToInt16(blockNumber));
                    //  blockHash now has something like the following stored in it:
                    //  "{\"result\":\"5895ee9dbb419645bc3a09969decf4929b572b9aa733c32e9d098d364a265e15\",\"error\":null,\"id\":null}\n"
                    
                    blockHash = blockHash.GetProperty("result");
                    // blockHash will now have the result value in it:
                    //  "5895ee9dbb419645bc3a09969decf4929b572b9aa733c32e9d098d364a265e15"
    
                    // We can now use this as a parameter for the getblock command which will return the block data including transaction data.
                    string block = dynamicRPC.getblock(blockHash);

                    // Which returns something like:
                    //  {"result":{"hash":"4088c837fc10d2cd2e6c35312c5e239fb6c437d91109eae2cbd0bbd46ed84f72","confirmations":1,
                    //   "strippedsize":552,"size":697,"weight":2353,"height":102, ..." etc, including the transaction data.

                    // Now we will use the GetStringList string extension method to return the transactions as a List of objects. 
                    // We need to do this as within the results stored in 'block' there is a JSON array of tx entries that we want to access. 
                    // We can get to this data using the 'dot' path notation "result.tx" as a parameter to GetStringList.
                    var transactions = block.GetStringList("result.tx");
                    
                    // Using Linq on the JSON results
                    // ******************************
                    // The string extension methods (like GetStringList) actually use System.Linq and Newtonsoft.Json.Linq to get the 
                    // results and convert them to the required data type for you. 
                    // You can do that in your code yourself should you want to using similar syntax to the example below. 
                    // Here's an example of using Linq to select the transactions from the result.tx JSON array:
                    JObject jObj = JObject.Parse(block);  
                    var transactionsLinq = 
                        from tx in jObj["result"]["tx"]
                        select tx;

                    // You can then iterate through them like this:
                    foreach (var tx in transactionsLinq)
                    {
                        Console.WriteLine(tx);
                    }
                    // That's what the string extension methods are doing, executing Linq and returning the results as 
                    // the required type (string, IList<string>, IList<object>) for convenience. Anyway, you can use Linq in your
                    // code should you want to. If not, you do not need to include the Newtonsoft.Json.Linq and System.Linq using statements
                    // that are at the top of this file within your own code.
                    // Leaving Linq and geting back to the main example...

                    // We'll need somewhere to save our running total
                    decimal voutTotal = 0;
                    
                    // Iterate through each transaction id in the List of transaction objects so we can total the outputs
                    foreach (var txid in transactions)
                    {
                        // Get the raw transaction data itself using the txid
                        string rawTransaction = dynamicRPC.getrawtransaction(txid);
                        
                        // We'll need the hex of the tx 
                        string transactionHex = rawTransaction.GetProperty("result"); 

                        // Decode the raw tx data so we can read it
                        string decodedTransaction = dynamicRPC.decoderawtransaction(transactionHex);
                        
                        // Now we'll use the other List based extension method GetObjectList to get the array of vouts from the JSON
                        // This is used to return complex objects within JSON data and uses the same access notation as GetProperty and GetStringList
                        var vouts = decodedTransaction.GetObjectList("result.vout"); 

                        // Iterating through the returned vouts List we can sum the 'value' property of each vout
                        foreach (var vout in vouts)
                        {
                            // Note that if you are targeting the Elements daemon, only the fee vout will have a 'value' property as the other 
                            // vouts will be blinded as Confidential Transactions and 'value' will be replaced by 'value-minimum' and 'value-maximum' 
                            // until unblinded. In the code below 'Convert.ToDecimal()' will therefore default to zero for blinded Elements vouts.
                            // See the tutorial on https://elementsproject.org for details of how to unblind the values if needed. 
                            string voutString = vout.ToString();
                            string valueString = voutString.GetProperty("value");
                            decimal value = Convert.ToDecimal(valueString);
                            voutTotal += value;
                        }
                    }
                    
                    // That's it! You've used dynamic method calls to send requests to the daemon and the 3 string extension methods to 
                    // read and handle the returned JSON formatted data. 
                    Console.WriteLine("Total sum of vouts in block " + blockNumber + ": " + Convert.ToString(voutTotal));

                    // Use the following to force an error at runtime for testing etc:
                    // As the method calls to dotnetcoreDynamicJSON_RPC are only evaluated at runtime they will not
                    // produce a compile time error, so test what you write ;-)
                    // dynamicRPC.thismethoddoesnotexist(1);
                    // dynamicRPC.getblockhash("method exists but this parameter is dodgy");     
                    // dynamicRPC.getblockhash("1"); //Wrong parameter type, should be int.                
                }
                catch (Exception e) {
                    Console.WriteLine(e.Message);
                }
            else
            {
                Console.WriteLine("Could not communicate with daemon");
            }
        }
    }
}
