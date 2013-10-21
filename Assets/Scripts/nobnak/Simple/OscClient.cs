using System.Net;
using System.Net.Sockets;
using System;

namespace nobnak.OSC {
	public class OscClient : IDisposable {
		public event Action<keijiro.Osc.Message> OnReceive;
		public event Action<Exception> OnError;
		
		private IPEndPoint _serverEndpoint;
		private UdpClient _udp;
		private AsyncCallback _callback;
		private keijiro.Osc.Parser _oscParser;
		private bool _disposed = false;
		
		public OscClient(IPEndPoint serverEndpoint) {
			_serverEndpoint = serverEndpoint;
			
			_oscParser = new keijiro.Osc.Parser();
			_udp = new UdpClient();
			_callback = new System.AsyncCallback(HandleReceived);
			
			_udp.BeginReceive(_callback, null);			
		}
		
		public void Send(byte[] oscPacket) {
			try {
				_udp.Send(oscPacket, oscPacket.Length, _serverEndpoint);
			} catch (Exception e) {
				if (OnError != null)
					OnError(e);
			}
		}

		private void HandleReceived(System.IAsyncResult ar) {
			try {
				if (_udp == null)
					return;
				var remoteEndpoint = new IPEndPoint(0, 0);
				byte[] receivedData = _udp.EndReceive(ar, ref remoteEndpoint);
				_oscParser.FeedData(receivedData);
				while (_oscParser.MessageCount > 0) {
					var m = _oscParser.PopMessage();
					if (OnReceive != null)
						OnReceive(m);
				}
				_udp.BeginReceive(_callback, null);
			} catch (Exception e) {
				if (OnError != null)
					OnError(e);
			}
		}

		#region IDisposable implementation
		public void Dispose () {
			if (_disposed)
				return;
			_disposed = true;
			
			if (_udp != null) {
				_udp.Close();
				_udp = null;
			}
		}
		#endregion
		
		~OscClient() { Dispose(); }
	}
}