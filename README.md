# dotnetcoreDynamicJSON-RPC
## A .Net Core class intended to enable simple, dynamic wrapping of JSON RPC calls to Bitcoin, Elements, Lightning daemons etc.

Runs on Windows, Linux, Mac using the .Net Core cross-platform application framework.

**Status: Work in progress.** Currently tidying up code comments and adding examples of use for [Bitcoin](https://github.com/bitcoin/bitcoin) and [Elements](https://elementsproject.org/) then i'll see how it works with [c-lightning](https://github.com/ElementsProject/lightning).

### Overview

There are a few great C# .Net based RPC wrappers for the Bitcoin daemon (bitcoind) available. As far as I have found they are all strongly typed/are .NET as opposed to .NET Core based. 

.NET Core is a free and open source application framework that runs on Linux, Windows and Mac OS. When combined with the free and open source Visual Studio Code IDE it is a very developer-friendly solution to multi-platform app development.

Strongly typed code is great to work with as it means you are not going to call a method name incorrectly as it will get highlighted at compile time. However, it does mean that for an API like Bitcoin's, the code you reference in your project will be quite sizeable and will need updating when new methods are added to the daemon. I have recently worked with the Python based [AuthServiceProxy](https://github.com/jgarzik/python-bitcoinrpc) when writing tests for an [Elements](https://github.com/ElementsProject/elements) blockchain and sidechain [tutorial](https://elementsproject.org/elements-code-tutorial/overview) I recently wrote and liked the fact it was a small and flexible tool for making RPC calls. Indeed, it was written for Bitcoin but because the method calls are dynamic it was easy to point it at an Elements daemon and make calls to new methods that Bitcoin's API does not contain without having to change the code at all. So I thought I'd give writing a similar tool in C# a go. This is still a work in progress and is currently at 'working' status with features to be added, mostly around the way it handles errors and the fact that you currently need to pass parameters in as the correct primitive type.

The code in dotnetcoreDynamicJSON-RPC.cs contains the dotnetcoreDynamicJSON_RPC class plus a helper class and that's all you need to copy into your code to use it. Declare an instance of it using the dynamic keyword and you are ready to go: dynamic dynamicJSON = new dotnetcoreDynamicJSON_RPC(url, port, user, pword);

The dotnetcoreDynamicJSON_RPC class inherits from the System.Dynamic.DynamicObject class and also uses System.Reflection to allow methods to be evaluated at runtime. This means you can add new methods to your code as they are added to Bitcoin, Elements, some-other-rpc-daemon without having to update any references your project has. The new method calls will be evaluated at runtime and sent off to the daemon as RPC calls. If the method is avaiable in the daemon it will get executed.

There is of course a caveat with runtime binding: if you call a method name incorrectly you wont find out until it runs, so type and test carefully! ;-)

The dotnetcoreDynamicJSON_RPC class has been tested with the Bitcoin daemon (bitcoind) and Elements daemon (elementsd) but there is no reason it can't be pointed at any similar daemon, such as Blockstream's [c-lightning](https://github.com/ElementsProject/lightning). I'll test this next when I have finished the examples and documentation for Bitcoin and Elements.

### Example

Let's say Bitcoin's daemon has methods availabe now called "getsomevalue" and "getsomeothervalue". You would call these by creating an instance of the dotnetcoreDynamicJSON_RPC class using the late-bound dynamic object type and calling them in your code:

~~~~
dynamic dynamicJSON = new dotnetcoreDynamicJSON_RPC(url, port, user, pword);
dynamicJSON.getsomevalue();
dynamicJSON.getsomeothervalue(someparam);
~~~~

Easy enough.

Now if a new version of bitcoind is released with a new method called "getsomenewvalue" added all you need to to use it is call:

~~~~
dynamicJSON.getsomenewvalue();
~~~~

There is no need to wait for me to add that method to the class or for you to change the code in any way.

**See Program.cs for example use.**

* * * 

If you want to use it in your project: just take the code from the dotnetcoreDynamicJSON-RPC.cs file and drop that in your project. That's it. Declare an instance of it using the dynamic keyword and you are ready to go: dynamic dynamicJSON = new dotnetcoreDynamicJSON_RPC(url, port, user, pword);

* * * 

To run the code in this repository (uses Bitcoin regtest to send a few transactions, create a block and loop the transaction outputs): 

### If you already have the .NET Core SDK and Visual Studio Code with C# set up:

Clone this repository and then open the folder using Visual Studio Code's 'File/Open folder' option.

You will see two prompts:

"Required assets to build and debug are missing. Add them?"
Click the 'Yes' buton.

"There are unresolved dependancies. Please execute the restore command to continue"
Click the 'Restore' button.

### If you don't have the .Net Core SDK and Visual Studio Code:

The Visual Studio Code IDE: https://code.visualstudio.com 

You don't need the IDE as you can edit in a text editor and compile using the 'dotnet run' command from within the directory... but it is a nice IDE and debugging in it is easy. After installing Visual Studio Code you will need to add the C# language extension: open Visual Studio Code and click the "Tools and languages" tab on the welcome screen. Select C# from the available extensions. Prerequisites and set up guides are listed and linked to here: https://docs.microsoft.com/en-us/dotnet/core/tutorials/with-visual-studio-code

The .NET Core SDK is here: https://www.microsoft.com/net/download

Questions? https://twitter.com/wintercooled

Issues? https://github.com/wintercooled/dotnetcoreDynamicJSON-RPC/issues
