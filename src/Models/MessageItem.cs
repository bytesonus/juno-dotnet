using System.Collections;
using System.Collections.Generic;

namespace JunoSDK.Models
{
	public class MessageItem
	{
		public static MessageItem Empty = new MessageItem(new object());
		public static MessageItem True = new MessageItem(true);
		public static MessageItem False = new MessageItem(false);

		private readonly object? value;

		private MessageItem(object? value)
		{
			this.value = value;
		}

		public bool IsNull() => value == null;
		public bool IsBool() => value is bool;
		public bool IsNumber() =>
			value is byte ||
			value is short ||
			value is ushort ||
			value is int ||
			value is uint ||
			value is long ||
			value is ulong ||
			value is float ||
			value is double;
		public bool IsString() => value is string;
		public bool IsArray() => value is IEnumerable;
		public bool IsObject() => value is IDictionary<string, MessageItem>;

		public object? AsNull() => value;
		public bool? AsBool() => value as bool?;
		public long? AsSigned() => value as long?;
		public ulong? AsUnsigned() => value as ulong?;
		public double? AsDecimal() => value as double?;
		public string? AsString() => value as string;
		public IEnumerable? AsArray() => value as IEnumerable;
		public IDictionary<string, MessageItem>? AsObject() => value as IDictionary<string, MessageItem>;

		public static MessageItem FromObject(object? value) => new MessageItem(value);
	}
}