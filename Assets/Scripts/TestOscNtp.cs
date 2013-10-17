using UnityEngine;
using System.Collections;
using System.Net.Sockets;
using System.Net;

public class TestOscNtp : MonoBehaviour {
	public int remotePort;
	
	private UdpClient _udp;
	private IPEndPoint _remoteEndpoint;
	private System.AsyncCallback _callback;
	private keijiro.Osc.Parser _oscParser;

	// Use this for initialization
	void Start () {
		_oscParser = new keijiro.Osc.Parser();
		_remoteEndpoint = new IPEndPoint(IPAddress.Loopback, remotePort);
		_udp = new UdpClient();
		_callback = new System.AsyncCallback(HandleReceived);
		_udp.BeginReceive(_callback, null);			
		
		StartCoroutine("Request");
	}
	
	void HandleReceived(System.IAsyncResult ar) {
		try {
			if (_udp == null)
				return;
			var remoteEndpoint = new IPEndPoint(0, 0);
			byte[] receivedData = _udp.EndReceive(ar, ref remoteEndpoint);
			_oscParser.FeedData(receivedData);
			while (_oscParser.MessageCount > 0) {
				var m = _oscParser.PopMessage();
				if (m.path != "/ntp/response")
					continue;
				
				var t3 = HighResTime.UtcNow;
				var t0 = System.DateTime.FromBinary(IPAddress.NetworkToHostOrder(System.BitConverter.ToInt64((byte[])m.data[0], 0)));
				var t1 = System.DateTime.FromBinary(IPAddress.NetworkToHostOrder(System.BitConverter.ToInt64((byte[])m.data[1], 0)));
				var t2 = System.DateTime.FromBinary(IPAddress.NetworkToHostOrder(System.BitConverter.ToInt64((byte[])m.data[2], 0)));
			}
		} catch (System.Exception e) {
			Debug.Log(e);
		}
	}
	
	IEnumerator Request() {
		while (true) {
			yield return new WaitForSeconds(1f);
			var oscEnc = new nobnak.OSC.MessageEncoder("/ntp/request");
			var t0 = System.BitConverter.GetBytes(IPAddress.HostToNetworkOrder(HighResTime.UtcNow.ToBinary()));
			oscEnc.Add(t0);
			var bytedata = oscEnc.Encode();
			_udp.Send(bytedata, bytedata.Length, _remoteEndpoint);
		}
	}
	
	void OnDestroy() {
		if (_udp != null) {
			_udp.Close();
			_udp = null;
		}
	}
}
