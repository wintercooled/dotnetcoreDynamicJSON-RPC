# dotnetcoreDynamicJSON-RPC
## A .NET Core class intended to enable simple, dynamic wrapping of JSON RPC calls to Bitcoin, Elements and other RPC enabled daemons.

### Status

Runs on Windows, Linux, Mac using the .NET Core cross-platform application framework.

Working example using [Bitcoin](https://github.com/bitcoin/bitcoin) as the RPC target.

Currently working on adding [Elements](https://elementsproject.org/) example use.

Future work includes trying to find an easy way to access [c-lightning](https://github.com/ElementsProject/lightning) daemon using RPC.

**dotnetcoreDynamicJSON-RPC.cs** - the actual code you can copy into your own project to wrap RPC calls.

**Program.cs** - an example program using the code with comments explaning how it works.

* * * 

### Overview

There are a few great C# .NET based RPC wrappers for the Bitcoin daemon (bitcoind) available. As far as I have found they are all strongly typed/are .NET based. 

.NET Core is a free and open source application framework that runs on Linux, Windows and Mac OS. When combined with the free and open source Visual Studio Code IDE it is a very developer-friendly solution to multi-platform app development.

Strongly typed code is great to work with as it means you are not going to call a method name incorrectly as it will get highlighted at compile time. However, it does mean that for an API like Bitcoin's, the code you reference in your project will be quite sizeable and will need updating when new methods are added to the daemon. 

I recently worked with the Python based [AuthServiceProxy](https://github.com/jgarzik/python-bitcoinrpc) when developing sample applications and tests for an [Elements](https://github.com/ElementsProject/elements) blockchain and sidechain [tutorial](https://elementsproject.org/elements-code-tutorial/overview). I liked the fact that it was a small and flexible tool for making RPC calls. Indeed, although it was written for Bitcoin, the method calls are dynamic and so it was easy to point it at an Elements daemon and make calls to new methods that Bitcoin's API does not contain... without having to change the code at all. So I thought I'd try writing a similar tool in C# using .NET Core. This is still a work in progress and is currently at a status of 'It works!' with a few features to be added, mostly around the way it handles errors. Also, you currently need to pass parameters in to RPC method calls using the correct primitive type (which I don't like and will change).

### The code

The code in dotnetcoreDynamicJSON-RPC.cs contains the **dotnetcoreDynamicJSON_RPC class** plus a helper class that lets you manipulate JSON strings easily. The contents of that file is all you need to copy into your code to use it. Then just declare an instance of it using the dynamic keyword and you are ready to go:

```dynamic dynamicJSON = new dotnetcoreDynamicJSON_RPC(url, port, user, pword);```

The **RPCResultExtensions class** within dotnetcoreDynamicJSON-RPC.cs just contains some string extension methods that provide data from the JSON strings the daemons return as easy to handle data types: ```string, IList<string>, IList<object>```. Program.cs shows how to use these extension methods. The extension methods actually use Linq to select the data they return and you can use Linq to directly query the JSON results returned by dotnetcoreDynamicJSON_RPC if you want. An example of how to do this is also shown in Program.cs.

The dotnetcoreDynamicJSON_RPC class inherits from the System.Dynamic.DynamicObject class and also uses System.Reflection to allow methods to be evaluated at runtime. This means you can add new methods to your code as they are added to Bitcoin, Elements, some-other-rpc-daemon without having to update any references your project has. The new method calls will be evaluated at runtime and sent off to the daemon as RPC calls. If the method is avaiable in the daemon it will get executed.

There is of course a caveat with runtime binding: if you call a method name incorrectly you wont find out until it runs, so type and test carefully! ;-)

The dotnetcoreDynamicJSON_RPC class has been tested with the Bitcoin daemon (bitcoind) and Elements daemon (elementsd) but there is no reason it can't be pointed at any similar daemon, such as Blockstream's [c-lightning](https://github.com/ElementsProject/lightning). I'll test this next when I have finished the examples and documentation for Bitcoin and Elements.

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

If you want to use it in your project: just take the code from the dotnetcoreDynamicJSON-RPC.cs file and drop that in your project. That's it. Declare an instance of it using the dynamic keyword and you are ready to go: 

```dynamic dynamicJSON = new dotnetcoreDynamicJSON_RPC(url, port, user, pword);```

### How to run the example code in Program.cs

To run the example code in Program.cs (uses Bitcoin regtest to send a few transactions, create a block and loop the transaction outputs in it): 

**If you already have the .NET Core SDK and Visual Studio Code with C# set up:**

Clone this repository and then open the folder using Visual Studio Code's 'File/Open folder' option.

You will see two prompts:

"Required assets to build and debug are missing. Add them?"
Click the 'Yes' buton.

"There are unresolved dependancies. Please execute the restore command to continue"
Click the 'Restore' button.

**If you don't have the .Net Core SDK and Visual Studio Code:**

The Visual Studio Code IDE: https://code.visualstudio.com 

You don't need the IDE as you can edit in a text editor and compile using the 'dotnet run' command from within the directory, but it is a nice IDE and debugging in it is easy. 

After installing Visual Studio Code you will need to add the C# language extension: 
Open Visual Studio Code and click the "Tools and languages" tab on the welcome screen. Select C# from the available extensions. 

Prerequisites and set up guides are listed and linked to here: https://docs.microsoft.com/en-us/dotnet/core/tutorials/with-visual-studio-code

The .NET Core SDK is here: https://www.microsoft.com/net/download

* * * 

Questions or Issues? https://github.com/wintercooled/dotnetcoreDynamicJSON-RPC/issues

I'm on Twitter: https://twitter.com/wintercooled
