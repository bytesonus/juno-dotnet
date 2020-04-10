using System.Threading.Tasks;

namespace JunoSDK.Connection
{
    public abstract class BaseConnection
    {
        protected DataHandler dataHandler;
        
        public abstract Task SetupConnection();
        public abstract Task CloseConnection();
        public abstract Task Send(byte[] data);

        public void SetOnDataListener(DataHandler dataHandler)
        {
            this.dataHandler = dataHandler;
        }
    }

    public delegate void DataHandler(byte[] data);
}