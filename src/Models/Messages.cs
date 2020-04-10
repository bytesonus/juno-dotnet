using System.Collections.Generic;
using JunoSDK.utils;

namespace JunoSDK.Models
{
	public abstract class BaseMessage
	{
		public static BaseMessage RegisterModuleRequest = new RegisterModuleRequestMessage();
		public static BaseMessage RegisterModuleResponse = new RegisterModuleResponseMessage();
		public static BaseMessage FunctionCallRequest = new FunctionCallRequestMessage();
		public static BaseMessage FunctionCallResponse = new FunctionCallResponseMessage();
		public static BaseMessage RegisterHookRequest = new RegisterHookRequestMessage();
		public static BaseMessage RegisterHookResponse = new RegisterHookResponseMessage();
		public static BaseMessage TriggerHookRequest = new TriggerHookRequestMessage();
		public static BaseMessage TriggerHookResponse = new TriggerHookResponseMessage();
		public static BaseMessage DeclareFunctionRequest = new DeclareFunctionRequestMessage();
		public static BaseMessage DeclareFunctionResponse = new DeclareFunctionResponseMessage();
		public static BaseMessage Error = new ErrorMessage();
		public static BaseMessage Unknown = new UnknownMessage();

		public string RequestId { get; set; }
		private readonly ulong messageType;

		protected BaseMessage(ulong messageType)
		{
			this.messageType = messageType;
		}

		public ulong GetMessageType()
		{
			return messageType;
		}
	}

	public class RegisterModuleRequestMessage : BaseMessage
	{
		public string ModuleId { get; set; }
		public string Version { get; set; }
		public Dictionary<string, string> Dependencies { get; set; } = new Dictionary<string, string>();

		public RegisterModuleRequestMessage() : base(Constants.RequestTypes.RegisterModuleRequest) { }
	}

	public class RegisterModuleResponseMessage : BaseMessage
	{
		public RegisterModuleResponseMessage() : base(Constants.RequestTypes.RegisterModuleResponse) { }
	}

	public class FunctionCallRequestMessage : BaseMessage
	{
		public string Function { get; set; }
		public Dictionary<string, MessageItem> Arguments { get; set; } = new Dictionary<string, MessageItem>();

		public FunctionCallRequestMessage() : base(Constants.RequestTypes.FunctionCallRequest) { }
	}

	public class FunctionCallResponseMessage : BaseMessage
	{
		public MessageItem Data { get; set; }

		public FunctionCallResponseMessage() : base(Constants.RequestTypes.FunctionCallResponse) { }
	}

	public class RegisterHookRequestMessage : BaseMessage
	{
		public string Hook { get; set; }

		public RegisterHookRequestMessage() : base(Constants.RequestTypes.RegisterHookRequest) { }
	}

	public class RegisterHookResponseMessage : BaseMessage
	{
		public RegisterHookResponseMessage() : base(Constants.RequestTypes.RegisterHookResponse) { }
	}

	public class TriggerHookRequestMessage : BaseMessage
	{
		public string Hook { get; set; } = string.Empty;
		public MessageItem Data { get; set; }

		public TriggerHookRequestMessage() : base(Constants.RequestTypes.TriggerHookRequest) { }
	}

	public class TriggerHookResponseMessage : BaseMessage
	{
		public TriggerHookResponseMessage() : base(Constants.RequestTypes.TriggerHookResponse) { }
	}

	public class DeclareFunctionRequestMessage : BaseMessage
	{
		public string Function { get; set; }

		public DeclareFunctionRequestMessage() : base(Constants.RequestTypes.DeclareFunctionRequest) { }
	}

	public class DeclareFunctionResponseMessage : BaseMessage
	{
		public string Function { get; set; }

		public DeclareFunctionResponseMessage() : base(Constants.RequestTypes.DeclareFunctionResponse) { }
	}

	public class ErrorMessage : BaseMessage
	{
		public ErrorMessage() : base(Constants.RequestTypes.Error) { }
	}

	public class UnknownMessage : BaseMessage
	{
		public UnknownMessage() : base(Constants.RequestTypes.Error) { }
	}
}