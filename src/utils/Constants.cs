namespace JunoSDK.utils
{
	public static class Constants
	{
		public static class RequestTypes
		{
			public const ulong Error = 0;

			public const ulong RegisterModuleRequest = 1;
			public const ulong RegisterModuleResponse = 2;

			public const ulong FunctionCallRequest = 3;
			public const ulong FunctionCallResponse = 4;

			public const ulong RegisterHookRequest = 5;
			public const ulong RegisterHookResponse = 6;

			public const ulong TriggerHookRequest = 7;
			public const ulong TriggerHookResponse = 8;

			public const ulong DeclareFunctionRequest = 9;
			public const ulong DeclareFunctionResponse = 10;
		}
	}
}