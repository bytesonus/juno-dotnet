using System.Collections.Generic;
using Newtonsoft.Json;

namespace JunoSDK.Models
{
	public class ModuleRegistration : BaseModel
	{
		[JsonProperty(Constants.ModuleId)]
		public string ModuleID { get; set; }

		[JsonProperty(Constants.Version)]
		public string ModuleVersion { get; set; }

		public ModuleRegistration(string moduleId, string moduleVersion, Dictionary<string, string>? dependencies)
		{
			RequestType = Constants.ModuleRegistration;
			ModuleID = moduleId;
			ModuleVersion = moduleVersion;
		}
	}
}