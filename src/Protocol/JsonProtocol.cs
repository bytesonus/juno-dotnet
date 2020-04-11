using System.Collections.Generic;
using System.Text;
using JunoSDK.Models;
using JunoSDK.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JunoSDK.Protocol
{
	public class JsonProtocol : BaseProtocol
	{
		public override byte[] Encode(BaseMessage request)
		{
			JObject jsonValue;
			switch (request)
			{
				case RegisterModuleRequestMessage message:
				{
					jsonValue = new JObject
					{
						[Constants.RequestKeys.RequestId] = message.RequestId,
						[Constants.RequestKeys.Type] = message.GetMessageType(),
						[Constants.RequestKeys.ModuleId] = message.ModuleId,
						[Constants.RequestKeys.Version] = message.Version,
						[Constants.RequestKeys.Dependencies] = new JObject(message.Dependencies)
					};
					break;
				}
				case RegisterModuleResponseMessage message:
				{
					jsonValue = new JObject
					{
						[Constants.RequestKeys.RequestId] = message.RequestId,
						[Constants.RequestKeys.Type] = message.GetMessageType()
					};
					break;
				}
				case FunctionCallRequestMessage message:
				{
					jsonValue = new JObject
					{
						[Constants.RequestKeys.RequestId] = message.RequestId,
						[Constants.RequestKeys.Type] = message.GetMessageType(),
						[Constants.RequestKeys.Function] = message.Function,
						[Constants.RequestKeys.Arguments] = new JObject(message.Arguments)
					};
					break;
				}
				case FunctionCallResponseMessage message:
				{
					jsonValue = new JObject
					{
						[Constants.RequestKeys.RequestId] = message.RequestId,
						[Constants.RequestKeys.Type] = message.GetMessageType(),
						[Constants.RequestKeys.Data] = JToken.FromObject(message.Data.GetValue())
					};
					break;
				}
				case RegisterHookRequestMessage message:
				{
					jsonValue = new JObject
					{
						[Constants.RequestKeys.RequestId] = message.RequestId,
						[Constants.RequestKeys.Type] = message.GetMessageType(),
						[Constants.RequestKeys.Hook] = message.Hook
					};
					break;
				}
				case RegisterHookResponseMessage message:
				{
					jsonValue = new JObject
					{
						[Constants.RequestKeys.RequestId] = message.RequestId,
						[Constants.RequestKeys.Type] = message.GetMessageType()
					};
					break;
				}
				case TriggerHookRequestMessage message:
				{
					jsonValue = new JObject
					{
						[Constants.RequestKeys.RequestId] = message.RequestId,
						[Constants.RequestKeys.Type] = message.GetMessageType(),
						[Constants.RequestKeys.Hook] = message.Hook
					};
					break;
				}
				case TriggerHookResponseMessage message:
				{
					jsonValue = new JObject
					{
						[Constants.RequestKeys.RequestId] = message.RequestId,
						[Constants.RequestKeys.Type] = message.GetMessageType()
					};
					break;
				}
				case DeclareFunctionRequestMessage message:
				{
					jsonValue = new JObject
					{
						[Constants.RequestKeys.RequestId] = message.RequestId,
						[Constants.RequestKeys.Type] = message.GetMessageType(),
						[Constants.RequestKeys.Function] = message.Function
					};
					break;
				}
				case DeclareFunctionResponseMessage message:
				{
					jsonValue = new JObject
					{
						[Constants.RequestKeys.RequestId] = message.RequestId,
						[Constants.RequestKeys.Type] = message.GetMessageType(),
						[Constants.RequestKeys.Function] = message.Function
					};
					break;
				}
				case ErrorMessage message:
				{
					jsonValue = new JObject
					{
						[Constants.RequestKeys.RequestId] = message.RequestId,
						[Constants.RequestKeys.Type] = message.GetMessageType(),
						[Constants.RequestKeys.Error] = message.ErrorCode
					};
					break;
				}
				default:
				{
					jsonValue = new JObject
					{
						[Constants.RequestKeys.RequestId] = "undefined",
						[Constants.RequestKeys.Type] = Constants.RequestTypes.Error,
						[Constants.RequestKeys.Error] = 0
					};
					break;
				}
			}

			return Encoding.Default.GetBytes($"{jsonValue.ToString(Formatting.None)}\n");
		}

		public override BaseMessage Decode(byte[] data)
		{
			var message = DecodeInternal(data);
			return message ?? new UnknownMessage();
		}

		private BaseMessage? DecodeInternal(byte[] data)
		{
			var jObject = JObject.Parse(Encoding.Default.GetString(data));

			var type = jObject[Constants.RequestKeys.Type]?.ToObject<ulong>();

			switch (type)
			{
				case Constants.RequestTypes.RegisterModuleRequest:
				{
					var requestId = jObject[Constants.RequestKeys.RequestId]?.ToObject<string>();
					if (requestId == null)
						return null;
					
					var moduleId = jObject[Constants.RequestKeys.ModuleId]?.ToObject<string>();
					if (moduleId == null)
						return null;
					
					var version = jObject[Constants.RequestKeys.Version]?.ToObject<string>();
					if (version == null)
						return null;

					var dependencies = jObject[Constants.RequestKeys.Dependencies]?.ToObject<JObject>();
					if (dependencies == null)
						return null;
					var dictionary = new Dictionary<string, string>();
					foreach (var key in jObject.Properties())
					{
						if (key.Type != JTokenType.String ||
							jObject[key.ToObject<string>()].Type != JTokenType.String)
						{
							return null;
						}

						var keyString = key.ToObject<string>();

						dictionary.Add(keyString, jObject[keyString].ToObject<string>());
					}

					return new RegisterModuleRequestMessage
					{
						RequestId = requestId,
						ModuleId = moduleId,
						Version = version,
						Dependencies = dictionary
					};
				}
				case Constants.RequestTypes.RegisterModuleResponse:
				{
					var requestId = jObject[Constants.RequestKeys.RequestId]?.ToObject<string>();
					if (requestId == null)
						return null;
					
					return new RegisterModuleResponseMessage
					{
						RequestId = requestId
					};
				}
				case Constants.RequestTypes.FunctionCallRequest:
				{
					var requestId = jObject[Constants.RequestKeys.RequestId]?.ToObject<string>();
					if (requestId == null)
						return null;

					var function = jObject[Constants.RequestKeys.Function]?.ToObject<string>();
					if (function == null)
						return null;

					var arguments = jObject[Constants.RequestKeys.Arguments]?.ToObject<JObject>()
						?.ToMessageItemDictionary();
					if (arguments == null)
						return null;

					return new FunctionCallRequestMessage
					{
						RequestId = requestId,
						Function = function,
						Arguments = arguments
					};
				}
				case Constants.RequestTypes.FunctionCallResponse:
				{
					var requestId = jObject[Constants.RequestKeys.RequestId]?.ToObject<string>();
					if (requestId == null)
						return null;

					var dataObject = jObject[Constants.RequestKeys.Data]?.ToMessageItem();
					if (dataObject == null)
						return null;
					
					return new FunctionCallResponseMessage
					{
						RequestId = requestId,
						Data = dataObject
					};
				}
				case Constants.RequestTypes.RegisterHookRequest:
				{
					var requestId = jObject[Constants.RequestKeys.RequestId]?.ToObject<string>();
					if (requestId == null)
						return null;

					var hook = jObject[Constants.RequestKeys.Hook]?.ToObject<string>();
					if (hook == null)
						return null;

					return new RegisterHookRequestMessage
					{
						RequestId = requestId,
						Hook = hook
					};
				}
				case Constants.RequestTypes.RegisterHookResponse:
				{
					var requestId = jObject[Constants.RequestKeys.RequestId]?.ToObject<string>();
					if (requestId == null)
						return null;

					return new RegisterHookResponseMessage
					{
						RequestId = requestId
					};
				}
				case Constants.RequestTypes.TriggerHookRequest:
				{
					var requestId = jObject[Constants.RequestKeys.RequestId]?.ToObject<string>();
					if (requestId == null)
						return null;

					var hook = jObject[Constants.RequestKeys.Hook]?.ToObject<string>();
					if (hook == null)
						return null;

					return new TriggerHookRequestMessage
					{
						RequestId = requestId,
						Hook = hook
					};
				}
				case Constants.RequestTypes.TriggerHookResponse:
				{
					var requestId = jObject[Constants.RequestKeys.RequestId]?.ToObject<string>();
					if (requestId == null)
						return null;

					return new TriggerHookResponseMessage
					{
						RequestId = requestId
					};
				}
				case Constants.RequestTypes.DeclareFunctionRequest:
				{
					var requestId = jObject[Constants.RequestKeys.RequestId]?.ToObject<string>();
					if (requestId == null)
						return null;

					var function = jObject[Constants.RequestKeys.Function]?.ToObject<string>();
					if (function == null)
						return null;

					return new DeclareFunctionRequestMessage
					{
						RequestId = requestId,
						Function = function
					};
				}
				case Constants.RequestTypes.DeclareFunctionResponse:
				{
					var requestId = jObject[Constants.RequestKeys.RequestId]?.ToObject<string>();
					if (requestId == null)
						return null;

					var function = jObject[Constants.RequestKeys.Function]?.ToObject<string>();
					if (function == null)
						return null;

					return new DeclareFunctionResponseMessage
					{
						RequestId = requestId,
						Function = function
					};
				}
				case Constants.RequestTypes.Error:
				default:
				{
					return new UnknownMessage();
				}
			}
		}
	}
}