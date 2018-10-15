# .NetCoreDynamicJSON-RPC
## .Net Core class intended to enable simple, dynamic wrapping of JSON RPC calls to Bitcoin, Elements, Lightning daemons etc.

Similar in concept to [python-bitcoinrpc AuthServiceProxy](https://github.com/jgarzik/python-bitcoinrpc)

Runs on Windows, Linux, Mac using the .Net Core cross-platform application framework.

There are a few C# .Net based RPC wrappers for the Bitcoin daemon (bitcoind) available. As far as I have found they are all strongly typed and are .NET as opposed to .NET Core based.

Strongly typed code is great to work with as it means you are not going to call a method name incorrectly. It does mean that for an API like Bitcoin's the code you reference in your project will be quite sizeable. I have recently worked with the Python based [AuthServiceProxy](https://github.com/jgarzik/python-bitcoinrpc) when writing tests for an [Elements](https://github.com/ElementsProject/elements) blockchain and sidechain [tutorial](https://elementsproject.org/elements-code-tutorial/overview) I recently wrote and liked the fact it was a small and flexible tool for making RPC calls. Indeed, it was written for Bitcoin but because the method calls are dynamic it was easy to point it at an Elements daemon and make calls to new methods that Bitcoin's API does not contain without having to change the code at all. So I thought I'd give writing a similar tool in C# a go. 

The code in dotnetcoreDynamicJSON-RPC.cs contains the dotnetcoreDynamicJSON-RPC class which does most of the work. It inherits from the System.Dynamic.DynamicObject class and also uses System.Reflection to allow methods to be evaluated at runtime. This means you can add new methods to your code as they are added to Bitcoin, Elements, some-other-rpc-daemon without having to update any references your project has. The new method calls will be evaluated at runtime and sent off to the daemon as RPC calls. If the method is avaiable in the daemon it will get executed.

Example:

Let's say Bitcoin's daemon has methods availabe now called "getsomevalue" and "getsomeothervalue". You would call these by creating an instance of the dotnetcoreDynamicJSON-RPC class using the late-bound dynamic object type and calling them in your code:

dynamic dynamicJSON = new dotnetcoreDynamicJSON(url, port, user, pword);
dynamicJSON.getsomevalue();
dynamicJSON.getsomeothervalue();

Now if a new version of bitcoind is released with a new method called "getsomenewvalue" added all you need to to use it is call:

dynamicJSON.getsomenewvalue();

There is no need to change the reference to the dotnetcoreDynamicJSON-RPC or update it or anything like that. 

There is of course a caveat with runtime binding - if you call a method name incorrectly you wont find out until it runs, so type and test carefully! ;-)

dotnetcoreDynamicJSON-RPC has been tested with the Bitcoin daemon bitcoind and Elements daemon (elementsd) but there is no reason it can't be pointed at any similar daemon, such as Blockstream's [c-lightnining](https://github.com/ElementsProject/lightning). I'll test this next when I have finished the examples and documentation for Bitcoin and Elements.

Status: Work in progress. Currently tidying up code comments and adding examples of use for Bitcoin and Elements.

**See Program.cs for example use.**

* * * 

To run: 

### If you already have Visual Studio Code and C# set up:

Clone and then open the folder using Visual Studio Code.

You will see two prompts:

"Required assets to build and debug are missing. Add them?"
Click the 'Yes' buton.

"There are unresolved dependancies. Please execute the restore command to continue"
Click the 'Restore' button.

### If you don't have .Net Core and an IDE yet:

The Visual Studio Code IDE: https://code.visualstudio.com 

Don't forget to then install the .Net Core SDK and add the C# language extension. The prerequisites are listed and linked to here: https://docs.microsoft.com/en-us/dotnet/core/tutorials/with-visual-studio-code

e.g. For Ubuntu 18.04:

Install Visual Studio Code: https://code.visualstudio.com 

Open Visual Studio Code and click the "Tools and languages" tab on the welcome screen. Select C# from the available extensions.

Install the .Net SDK: https://www.microsoft.com/net/download/linux-package-manager/ubuntu18-04/sdk-current


