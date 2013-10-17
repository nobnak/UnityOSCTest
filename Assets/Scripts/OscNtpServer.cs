using UnityEngine;
using System.Net.Sockets;
using System.Net;
using System.Collections.Generic;

public class OscNtpServer : MonoBehaviour {
	public int listenPort;
	
	private UdpClient _udp;
	private System.AsyncCallback _callback;
	private keijiro.Osc.Parser _oscParser;
	private IPEndPoint _endpoint;
	private Queue<NtpRequest> _requests;

	// Use this for initialization
	void Start () {
		_requests = new Queue<NtpRequest>();
		_endpoint = new IPEndPoint(IPAddress.Any, listenPort);
		_udp = new UdpClient(_endpoint);
		_callback = new System.AsyncCallback(HandleReceive);
		_oscParser = new keijiro.Osc.Parser();
		
		_udp.BeginReceive(_callback, null);			
	}
	
	// Update is called once per frame
	void Update () {
		try {
			lock (_requests) {
				while (_requests.Count > 0) {
					var req = _requests.Dequeue();
					var oscEnc = new nobnak.OSC.MessageEncoder("/ntp/response");
					oscEnc.Add(req.t0);
					oscEnc.Add(req.t1);
					var t2 = System.BitConverter.GetBytes(IPAddress.HostToNetworkOrder(HighResTime.UtcNow.ToBinary()));
					oscEnc.Add(t2);
					var bytedata = oscEnc.Encode();
					_udp.Send(bytedata, bytedata.Length, req.remote);
				}
			}
		} catch (System.Exception e) {
			Debug.Log(e);
		}
	}
		
	void HandleReceive(System.IAsyncResult ar) {
		if (_udp == null)
			return;
		
		try {
			Debug.Log("Request received");
			var remoteEndpoint = new IPEndPoint(0, 0);
			byte[] receivedData = _udp.EndReceive(ar, ref remoteEndpoint);
			_oscParser.FeedData(receivedData);
			while (_oscParser.MessageCount > 0) {
				var m = _oscParser.PopMessage();
				if (m.path != "/ntp/request")
					continue;
				
				lock (_requests) {
					_requests.Enqueue(new NtpRequest() {
						remote = remoteEndpoint,
						t0 = (byte[])m.data[0],
						t1 = System.BitConverter.GetBytes(IPAddress.HostToNetworkOrder(HighResTime.UtcNow.ToBinary())),
					});
				}
			}
		} catch (System.Exception e) {
			Debug.Log(e);
		}
		_udp.BeginReceive(_callback, null);
	}
	
	void OnDestroy() {
		if (_udp != null) {
			_udp.Close();
			_udp = null;
		}
	}
	
	public struct NtpRequest {
		public IPEndPoint remote;
		public byte[] t0;
		public byte[] t1;
	}
}
