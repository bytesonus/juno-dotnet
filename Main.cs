using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace JunoSDK
{
	public class Program
	{
		public static async Task Main(string[] args)
		{
			Juno.Initialize("test", "0.0.0");
			await Juno.Register();
			Console.WriteLine("Registered");
			await Juno.DeclareFunction("FunctionNameHere", FunctionNameHere);
			while(true)
				await Task.Delay(1000);
		}

		public static JObject FunctionNameHere(JObject args)
		{
			Console.WriteLine("Function called");
			return new JObject();
		}
	}
}