using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JunoSDK.Connection
{
    public class UnixSocketConnection : BaseConnection
    {
		private readonly string socketPath;
		private Socket? socket;
		private readonly List<byte> buffer = new List<byte>();

        public UnixSocketConnection(string socketPath)
        {
			this.socketPath = socketPath;
        }

		public override async Task SetupConnection()
		{
			socket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);
			await socket.ConnectAsync(new UnixDomainSocketEndPoint(socketPath));
#pragma warning disable 4014
			Task.Run(PollForData);
#pragma warning restore 4014
		}

		public override Task CloseConnection()
		{
			socket?.Close();
			socket?.Dispose();
			return Task.CompletedTask;
		}
		public override async Task Send(byte[] data)
		{
			if (socket != null)
			{
				await socket.SendAsync(data, SocketFlags.None);
			}
		}
		
		private void PollForData()
		{
			while (socket?.Connected == true)
			{
				var dataAvailableToRead = socket.Available;

				if (dataAvailableToRead == 0)
				{
					Thread.Sleep(5);
					continue;
				}

				// Fill read buffer with data coming from the socket
				var readBuffer = new byte[dataAvailableToRead];
				socket.Receive(readBuffer, dataAvailableToRead, SocketFlags.None);

				buffer.AddRange(readBuffer);
				var input = Encoding.Default.GetString(buffer.ToArray());
				if (!input.Contains('\n'))
					continue;

				// Split the incoming data by \n to separate requests
				var jsons = input.Split('\n', StringSplitOptions.RemoveEmptyEntries);

				// If the last request didn't end with a \n, then it's probably an incomplete one
				// So, don't process the last request (iterate to length - 1, allowing it to fill the buffer)
				var didReceiveCompleteRequest = input.EndsWith('\n');
				var requestCount = didReceiveCompleteRequest ? jsons.Length : jsons.Length - 1;

				for (var i = 0; i < requestCount; i++)
				{
					var request = jsons[i];
					OnData(Encoding.Default.GetBytes(request));
				}
				buffer.Clear();

				// if you didn't receive a complete request, keep the last data
				// to allow the new data to append to it
				if (!didReceiveCompleteRequest)
					buffer.AddRange(Encoding.Default.GetBytes(jsons[^1]));
			}
		}
    }
}