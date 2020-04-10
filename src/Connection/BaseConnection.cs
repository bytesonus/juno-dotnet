using System.Threading.Tasks;

namespace JunoSDK.Connection
{
	public abstract class BaseConnection
	{
		private DataHandler? dataHandler;
		
		public abstract Task SetupConnection();
		public abstract Task CloseConnection();
		public abstract Task Send(byte[] data);

		public void SetOnDataListener(DataHandler dataHandler) => this.dataHandler = dataHandler;

		protected void OnData(byte[] data) => dataHandler?.Invoke(data);
	}

	public delegate void DataHandler(byte[] data);
}