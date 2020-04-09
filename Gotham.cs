using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace JunoSDK
{
	public static class Juno
	{
		public const string UnixSocket = "../juno.sock";
		private static Socket socket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);
		private static JObject EmptyJObject = JObject.Parse("{}");

		public static bool Registered { get; internal set; } = false;
		public static string ModuleID { get; internal set; } = "undefined";
		public static string Version { get; internal set; } = "0.0.0";
		public static Dictionary<string, string>? Dependencies { get; internal set; } = null;

		private static Dictionary<string, TaskCompletionSource<JObject>> responseTasks = new Dictionary<string, TaskCompletionSource<JObject>>();
		private static bool initialized = false;
		private static StringBuilder buffer = new StringBuilder();
		private static Dictionary<string, Func<JObject, JObject>> RegisteredFunctions = new Dictionary<string, Func<JObject, JObject>>();

		public static void Initialize(string moduleId, string version, Dictionary<string, string>? dependencies = null)
		{
			ModuleID = moduleId;
			Version = version;
			Dependencies = dependencies;
			initialized = true;
		}

		public static Task Register()
		{
			CheckInitialized();

			socket.Connect(new UnixDomainSocketEndPoint(UnixSocket));

			var requestId = GetRequestID();
			var registrationTask = new TaskCompletionSource<JObject>();
			responseTasks.Add(requestId, registrationTask);

			Send(
				new JObject
				{
					[Constants.RequestId] = requestId,
					[Constants.Type] = Constants.ModuleRegistration,
					[Constants.ModuleId] = ModuleID,
					[Constants.Version] = Version
				}
			);

			Task.Run(PollForData);

			return registrationTask.Task;
		}

		public static Task DeclareFunction(string functionName, Func<JObject, JObject> function)
		{
			var requestId = GetRequestID();
			var functionDeclaration = new TaskCompletionSource<JObject>();
			responseTasks.Add(requestId, functionDeclaration);

			RegisteredFunctions.Add(functionName, function);

			Send(
				new JObject
				{
					[Constants.RequestId] = requestId,
					[Constants.Type] = Constants.DeclareFunction,
					[Constants.Function] = functionName
				}
			);

			return functionDeclaration.Task;
		}

		private static void PollForData()
		{
			while (socket.Connected)
			{
				var dataAvailableToRead = socket.Available;

				if (dataAvailableToRead == 0)
				{
					Thread.Sleep(5);
					continue;
				}

				// Fill read buffer with data coming from the socket
				var readBuffer = new byte[dataAvailableToRead];
				socket.Receive(readBuffer, dataAvailableToRead, SocketFlags.None);

				buffer.Append(Encoding.UTF8.GetString(readBuffer));
				var input = buffer.ToString();
				if (!input.Contains('\n'))
					continue;

				// Split the incoming data by \n to separate requests
				var jsons = input.Split('\n', StringSplitOptions.RemoveEmptyEntries);

				// If the last request didn't end with a \n, then it's probably an incomplete one
				// So, don't process the last request (iterate to length - 1, allowing it to fill the buffer)
				var didRecieveCompleteRequest = input.EndsWith('\n');
				var requestCount = didRecieveCompleteRequest ? jsons.Length : jsons.Length - 1;

				for (var i = 0; i < requestCount; i++)
				{
					var request = jsons[i];
					Task.Run(() => HandleRequest(request));
				}
				buffer.Clear();

				// if you didn't recieve a complete request, keep the last data
				// to allow the new data to append to it
				if (!didRecieveCompleteRequest)
					buffer.Append(jsons[jsons.Length - 1]);
			}
		}

		private static string GetRequestID()
		{
			CheckInitialized();

			return ModuleID + DateTime.Now.Ticks;
		}

		private static void CheckInitialized()
		{
			if (!initialized)
				throw new ApplicationException("The SDK is not initialized yet. The Juno.Initialize function must be called first");
		}

		private static void HandleRequest(string data)
		{
			try
			{
				if (data == null)
					return;

				var request = JObject.Parse(data);

				var requestId = request[Constants.RequestId]?.ToObject<string>();

				if (requestId == null)
				{
					// Invalid response. No requestId recieved. Ignore data
					return;
				}

				var requestType = request[Constants.Type]?.ToObject<string>();
				switch (requestType)
				{
					case Constants.ModuleRegistered:
						responseTasks[requestId].TrySetResult(EmptyJObject);
						return;
					case Constants.FunctionResponse:
						responseTasks[requestId].TrySetResult(request[Constants.Data]?.ToObject<JObject>() ?? EmptyJObject);
						break;
					case Constants.FunctionCall:
						var functionName = request[Constants.Function]?.ToObject<string>()?.Replace($"{ModuleID}.", "");
						if (functionName == null)
						{
							SendError(requestId, Constants.Errors.UnknownFunction);
							return;
						}
						if (!RegisteredFunctions.ContainsKey(functionName))
						{
							SendError(requestId, Constants.Errors.UnknownFunction);
							return;
						}

						var function = RegisteredFunctions[functionName];
						var args = request[Constants.Arguments]?.ToObject<JObject>() ?? EmptyJObject;
						var response = function.Invoke(args);

						Send(
							new JObject
							{
								[Constants.RequestId] = requestId,
								[Constants.Type] = Constants.FunctionResponse,
								[Constants.Data] = response
							}
						);
						break;
				}

				if (responseTasks.ContainsKey(requestId))
				{
					switch (request[Constants.Type]?.ToObject<string>())
					{
						case Constants.ModuleRegistration:
							responseTasks[requestId].TrySetResult(EmptyJObject);
							break;
						case Constants.FunctionResponse:
							responseTasks[requestId].TrySetResult(request[Constants.Data]?.ToObject<JObject>() ?? EmptyJObject);
							break;
						case Constants.Error:
							var error = request[Constants.Error]?.ToObject<string>();
							responseTasks[requestId].TrySetException(Utils.GetExceptionForError(error));
							break;
					}
				}
				else
				{
					switch (request[Constants.Type]?.ToObject<string>())
					{
						case Constants.FunctionCall:
							var functionName = request[Constants.Function]?.ToObject<string>()?.Replace($"{ModuleID}.", "");
							Console.WriteLine("Looking for function " + functionName);
							if (functionName == null || !RegisteredFunctions.ContainsKey(functionName))
							{
								Console.WriteLine("Invalid request");
								return;
							}
							var response = RegisteredFunctions[functionName].Invoke(request[Constants.Arguments]?.ToObject<JObject>() ?? new JObject());
							Send(
								new JObject
								{
									[Constants.RequestId] = requestId,
									[Constants.Type] = Constants.FunctionResponse,
									[Constants.Data] = response
								}
							);
							break;
					}
					// Dude. Dafuq? You just recieved a response that didn't have a known request ID
					// What do I do now?

					// Oh wait....this is possible if Juno is trying to invoke a function of this module from another module
					// TODO handle that
					return;
				}
			}
			catch (System.Exception)
			{
				// Log this shit.
			}
		}

		private static void SendError(string requestId, string error)
		{
			Send(
				new JObject
				{
					[Constants.RequestId] = requestId,
					[Constants.Type] = Constants.Error,
					[Constants.Error] = error
				}
			);
		}

		private static void Send(JObject data)
		{
			var stringified = data.ToString(Constants.JsonFormatting) + "\n";
			var writeBuffer = Encoding.UTF8.GetBytes(stringified);
			socket.Send(writeBuffer, 0, writeBuffer.Length, SocketFlags.None);
		}
	}

	internal delegate void RequestHandler(JObject request);
}
