using System;

namespace JunoSDK
{
	public class Utils
	{
		public static Exception GetExceptionForError(string? error)
		{
			return new ApplicationException();
		}
	}
}