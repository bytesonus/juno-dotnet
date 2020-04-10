using System;
using System.Collections.Generic;
using JunoSDK.Models;

namespace JunoSDK.Protocol
{
	public abstract class BaseProtocol
	{
		private string? moduleId;

		public BaseMessage Initialize(string moduleId, string version, Dictionary<string, string> dependencies)
		{
			this.moduleId = moduleId;
			return new RegisterModuleRequestMessage()
			{
				RequestId = GenerateRequestId(),
				ModuleId = moduleId,
				Version = version,
				Dependencies = dependencies,
			};
		}

		public BaseMessage RegisterHook(string hook)
		{
			return new RegisterHookRequestMessage()
			{
				RequestId = GenerateRequestId(),
				Hook = hook,
			};
		}

		public BaseMessage TriggerHook(string hook)
		{
			return new TriggerHookRequestMessage()
			{
				RequestId = GenerateRequestId(),
				Hook = hook,
			};
		}

		public BaseMessage DeclareFunction(string function)
		{
			return new DeclareFunctionRequestMessage()
			{
				RequestId = GenerateRequestId(),
				Function = function,
			};
		}

		public BaseMessage CallFunction(string function, Dictionary<string, MessageItem> arguments)
		{
			return new FunctionCallRequestMessage()
			{
				RequestId = GenerateRequestId(),
				Function = function,
				Arguments = arguments
			};
		}

		public abstract byte[] Encode(BaseMessage request);

		public abstract BaseMessage Decode(byte[] data);

		private string GenerateRequestId()
		{
			return $"{moduleId ?? "undefined"}-{DateTime.Now.Ticks}";
		}
	}
}