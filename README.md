# Good luck getting this working after any .NET updates - I've adandoned .NET/MVC for Python/Flask now.

## A .NET Core class intended to enable simple, dynamic wrapping of JSON RPC calls to [Bitcoin](https://github.com/bitcoin/bitcoin), [Elements](https://elementsproject.org/), [Liquid](https://blockstream.com/liquid/) and other RPC enabled daemons.

## Runs on Linux, Windows and Mac OS, letting you quickly write node-connected applications, automate the set up of blockchain states, test RPC commands, or whatever else you may find it useful for.

## An example of how to use the dotnetcoreDynamicJSON-RPC class within a .NET Core MVC app can be found [here](https://github.com/wintercooled/dotnetcoreDynamicJSON-RPC_MVCExample).

### Overview

Uses dynamic types to send RPC commands, meaning the code is brief and you can call whatever is listed here: https://bitcoin.org/en/developer-reference#bitcoin-core-apis, the extra RPC calls used by Elements/Liquid, and any future RPC commands, with little effort and no need to change the code within dotnetcoreDynamicJSON-RPC in the future.

Program.cs contains a working example using Bitcoin (bitcoind) and Elements/Liquid (elementsd) as the target daemons.

**dotnetcoreDynamicJSON-RPC.cs** - the actual code you can copy into your own project to wrap RPC calls.

**Program.cs** - an example program using the code with comments explaning how it works.

Future work includes trying to find an easy way to access [c-lightning](https://github.com/ElementsProject/lightning) daemon using RPC.

* * * 

### Why?

There are a few great C# based RPC wrappers for the Bitcoin daemon (bitcoind) available. As far as I have found they are all strongly typed/are .NET based and only work on Windows. Being strongly typed means they can't easily be pointed at other daemons, such as Elements, without being reworked. That's not ideal if you want a lightweight piece of code that can connect to different types of node.

Strongly typed code is great to work with as it means you are not going to call a method name incorrectly as it will get highlighted at compile time, but it also means that for an API like Bitcoin's, the code you reference in your project will be quite sizeable and will need updating when new methods are added to the daemon. 

I recently worked with the Python based [AuthServiceProxy](https://github.com/jgarzik/python-bitcoinrpc) when developing sample applications and tests for an [Elements](https://github.com/ElementsProject/elements) blockchain and sidechain [tutorial](https://elementsproject.org/elements-code-tutorial/overview). I liked the fact that it was a small and flexible tool for making RPC calls. Indeed, although it was written for Bitcoin, the method calls are dynamic and so it was easy to point it at an Elements daemon and make calls to new methods that Bitcoin's API does not contain, all without having to change the code making the RPC calls. 

So I thought I'd try writing a similar tool in C# using .NET Core. 

.NET Core, a free and open source application framework that runs on Linux, Windows and Mac OS. When combined with the free and open source Visual Studio Code IDE it is a very developer-friendly solution to multi-platform app development.

I chose to develop a solution that was not strongly typed and instead uses Reflection and DynamicObject to resolve methods at runtime. This means the code is contained in just one small class and can be directed at any RPC enabled node with ease.

This is still a work in progress - you currently need to pass parameters in to RPC method calls using the correct primitive type (which I don't like and will change).

There are plenty of Bitcoin programming guides out there that outline the API calls you can make over RPC. If you are looking for one on Elements or Liquid, there is the tutorial on [https://elementsproject.org](https://elementsproject.org) that covers everything from sidechain set up to Confidential Transactions and Issued Assets.

### The code

The code in dotnetcoreDynamicJSON-RPC.cs contains the awkwardly named **dotnetcoreDynamicJSON_RPC class**, plus a helper class that lets you manipulate JSON strings easily. The contents of that file is all you need to copy into your code to use it (plus a [reference to Newtonsoft.Json](https://github.com/wintercooled/dotnetcoreDynamicJSON-RPC/blob/master/dotnetcoreDynamicJSON-RPC.csproj) in your project's .csproj file). Then just declare an instance of it using the dynamic keyword and you are ready to go!

```dynamic dynamicJSON = new dotnetcoreDynamicJSON_RPC(url, port, user, pword);```

The **RPCResultExtensions class** within dotnetcoreDynamicJSON-RPC.cs just contains some string extension methods that provide data from the JSON strings the daemons return as easy to handle data types: ```string``` or ```IList<string>``` or ```IList<object>```. Program.cs shows how to use these extension methods. The extension methods actually use Linq to select the data they return and you can use Linq to directly query the JSON results returned by dotnetcoreDynamicJSON_RPC if you want. An example of how to do this is also shown in Program.cs.

The dotnetcoreDynamicJSON_RPC class inherits from the System.Dynamic.DynamicObject class and also uses System.Reflection to allow methods to be evaluated at runtime. This means you can add new methods to your code as they are added to Bitcoin, Elements, Liquid, some-other-rpc-daemon without having to update any references your project has. The new method calls will be evaluated at runtime and sent off to the daemon as RPC calls. If the method is avaiable in the daemon, it will get executed.

There is of course a caveat with runtime binding: if you call a method name incorrectly or pass the wrong type of parameter in, you wont find out until it runs, so type and test carefully! ;-)

### Example

Let's say Bitcoin's daemon has methods availabe now called "getsomevalue" and "getsomeothervalue". You would call these by creating an instance of the dotnetcoreDynamicJSON_RPC class using the late-bound dynamic object type and call them in your code:

~~~~
dynamic dynamicJSON = new dotnetcoreDynamicJSON_RPC(url, port, user, pword);
string results1 = dynamicJSON.getsomevalue();
string results2 = dynamicJSON.getsomeothervalue(someparam);
~~~~

Easy enough.

Now if a new version of bitcoind is released with a new method called "getsomenewvalue" added all you need to to use it is call:

~~~~
string results3 = dynamicJSON.getsomenewvalue();
~~~~

There is no need to wait for me to add that method to the class or for you to change the code yourself in any way.

### How to use it in your project

If you want to use it in your project: just take the dotnetcoreDynamicJSON-RPC.cs file and drop that in your project (plus add [a reference to Newtonsoft.Json](https://github.com/wintercooled/dotnetcoreDynamicJSON-RPC/blob/master/dotnetcoreDynamicJSON-RPC.csproj) in your project's .csproj file). That's it. Declare an instance of it using the dynamic keyword and you are ready to go: 

```dynamic dynamicJSON = new dotnetcoreDynamicJSON_RPC(url, port, user, pword);```

### How to run the example code in Program.cs

The example code in Program.cs uses Bitcoin or Elements regtest to send a few transactions, create a block and loop the transaction outputs in it.

Make sure bitcoind or elementsd is running in regtest mode and the RPC details in the code match those used by the daemon.

Clone the repository, move into the new folder, run the code...
~~~~
git clone https://github.com/wintercooled/dotnetcoreDynamicJSON-RPC.git
cd dotnetcoreDynamicJSON-RPC 
dotnet run
~~~~

That's it!

**If you don't have the .Net Core SDK:**

The code targets version 2.1 of the .NET Core framework.

Register the Microsoft key and feed by choosing the Linux distribution you are using from the [dotnet download site](https://dotnet.microsoft.com/download/linux-package-manager/ubuntu18-04/sdk-current). It will give you the code to run and will look similar to this:

```
wget -q https://packages.microsoft.com/config/ubuntu/18.04/packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
```

Install the .NET SDK:

```
sudo add-apt-repository universe
sudo apt-get install apt-transport-https
sudo apt-get update
sudo apt-get install dotnet-sdk-2.1
```

**Using Visual Studio Code:**

You don't need Visual Studio Code to edit the code, you can use any text editor. Visual Studio Code is a nice IDE and debugging in it is easy though, so it is easy to recommend. [https://code.visualstudio.com](https://code.visualstudio.com)

After installing Visual Studio Code you will need to add the C# language extension: 

Open Visual Studio Code and click the "Tools and languages" box on the welcome screen. Select C# from the available extensions (id: ms-vscode.csharp). 

Prerequisites and set up guides are listed and linked to here: [https://docs.microsoft.com/en-us/dotnet/core/tutorials/with-visual-studio-code](https://docs.microsoft.com/en-us/dotnet/core/tutorials/with-visual-studio-code)

**If you already have the .NET Core SDK and Visual Studio Code with C# set up:**

Note: The code targets version 2.1 of the .NET Core framework.

Clone this repository and then open the folder using Visual Studio Code's 'File/Open folder' option.

You will see two prompts:

"Required assets to build and debug are missing. Add them?"

- Click the 'Yes' buton.

"There are unresolved dependancies. Please execute the restore command to continue"

- Click the 'Restore' button.

**Visual Studio 2017 (Windows only):**

Clone this repository and then open the .csproj project file using Visual Studio's 'File/Open Project/Solution' option.

* * * 

Questions or Issues? [https://github.com/wintercooled/dotnetcoreDynamicJSON-RPC/issues](https://github.com/wintercooled/dotnetcoreDynamicJSON-RPC/issues)

I'm on Twitter: [https://twitter.com/wintercooled](https://twitter.com/wintercooled)
