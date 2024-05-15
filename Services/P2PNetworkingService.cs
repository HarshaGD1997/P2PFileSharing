using System.Net;
using System.Net.Sockets;
using System.Text;

namespace P2PFileSharing.Services
{
	public class P2PNetworkingService
	{
		public void StartServer(int port)
		{
			TcpListener server = new TcpListener(IPAddress.Any, port);
			server.Start();

			while (true)
			{
				TcpClient client = server.AcceptTcpClient();
				NetworkStream stream = client.GetStream();

				byte[] buffer = new byte[client.ReceiveBufferSize];
				stream.Read(buffer, 0, buffer.Length);

				string request = Encoding.UTF8.GetString(buffer);
				// Process the request
			}
		}

		public void StartClient(string ip, int port, string message)
		{
			TcpClient client = new TcpClient(ip, port);
			NetworkStream stream = client.GetStream();

			byte[] buffer = Encoding.UTF8.GetBytes(message);
			stream.Write(buffer, 0, buffer.Length);
		}
	}
}