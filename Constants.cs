using Newtonsoft.Json;

namespace JunoSDK
{
	public class Constants
	{
		// Request keys
		public const string Type = "type";
		public const string RequestId = "requestId";
		public const string Function = "function";
		public const string ModuleId = "moduleId";
		public const string Success = "success";
		public const string Dependencies = "dependencies";
		public const string Arguments = "arguments";
		public const string Data = "data";
		public const string Hook = "hook";
		public const string Version = "version";

		// Request types
		public const string Error = "error";
		public const string ModuleRegistration = "moduleRegistration";
		public const string ModuleRegistered = "moduleRegistered";
		public const string FunctionCall = "functionCall";
		public const string FunctionResponse = "functionResponse";
		public const string HookTrigger = "hookTrigger";
		public const string RegisterHook = "registerHook";
		public const string DeclareFunction = "declareFunction";
		
		public static readonly Formatting JsonFormatting = Formatting.None;

		public static class Errors
		{
			public const string InvalidRequestId = "invalidRequestId";
			public const string UnknownRequest = "unknownRequest";
			public const string MalformedRequest = "malformedRequest";
			public const string UnregisteredModule = "unregisteredModule";
			public const string UnknownModule = "unknownModule";
			public const string UnknownFunction = "unknownFunction";
		}
	}
}