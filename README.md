# Juno Dotnet

This is a library that provides you with helper methods for interfacing with the microservices framework, [juno](https://github.com/bytesonus/juno).

## How to use:

There is a lot of flexibility provided by the library, in terms of connection options and encoding protocol options. However, in order to use the library, none of that is required.

In case you are planning to implement a custom connection option, you will find an example in `src/Connection/UnixSocketConnection.cs`.

For all other basic needs, you can get away without worrying about any of that.

### A piece of code is worth a thousand words

```csharp
using System;
using JunoSDK;
using JunoSDK.Models;
using System.Threading.Tasks;
using System.Collections.Generic;

public class Program
{
	public static async Task Main(string[] args)
	{
		var module = JunoModule.FromUnixSocket("./path/to/juno.sock");
		await module.Initialize("module-name", "1.0.0");
		Console.WriteLine("Initialized!");
		await module.DeclareFunction("print_hello", async (args) => {
			Console.WriteLine("Hello");
			return MessageItem.Empty;
		});
		await module.CallFunction("module2.print_hello_world", new Dictionary<string, MessageItem>());
		while(true)
		{
		    await Task.Delay(1000);
		}
	}
}
```