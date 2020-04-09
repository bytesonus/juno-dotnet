using Newtonsoft.Json;

namespace JunoSDK.Models
{
	public abstract class BaseModel
	{
		[JsonProperty(Constants.RequestId)]
		public string RequestID { get; set; } = string.Empty;

		[JsonProperty(Constants.Type)]
		public string RequestType { get; set; } = string.Empty;
	}
}