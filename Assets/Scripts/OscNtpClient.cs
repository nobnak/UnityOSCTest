using UnityEngine;
using System.Collections;
using System.Net.Sockets;
using System.Net;

public class OscNtpClient : MonoBehaviour {
	public const string NTP_REQUEST = "/ntp/request";
	public const string NTP_RESPONSE = "/ntp/response";
	
	public string remoteHost = "localhost";
	public int remotePort = 10000;
	public float interval = 1f;
	public int nSamplesPOT = 64;
	public RingBuffer<NtpStat> stats;
	
	private UdpClient _udp;
	private IPEndPoint _remoteEndpoint;
	private System.AsyncCallback _callback;
	private keijiro.Osc.Parser _oscParser;

	// Use this for initialization
	void Start () {
		stats = new RingBuffer<NtpStat>(nSamplesPOT);
		_oscParser = new keijiro.Osc.Parser();
		var address = Dns.GetHostAddresses(remoteHost)[0];
		_remoteEndpoint = new IPEndPoint(address, remotePort);
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
				if (m.path != NTP_RESPONSE)
					continue;
				
				var t3 = HighResTime.UtcNow;
				var t0 = System.DateTime.FromBinary(IPAddress.NetworkToHostOrder(System.BitConverter.ToInt64((byte[])m.data[0], 0)));
				var t1 = System.DateTime.FromBinary(IPAddress.NetworkToHostOrder(System.BitConverter.ToInt64((byte[])m.data[1], 0)));
				var t2 = System.DateTime.FromBinary(IPAddress.NetworkToHostOrder(System.BitConverter.ToInt64((byte[])m.data[2], 0)));
				var roundtrip = NtpUtil.Roundtrip(t0, t1, t2, t3);
				var delay = NtpUtil.Delay(t0, t1, t2, t3);
				stats.Add(new NtpStat() { delay = delay, roundtrip = roundtrip });
				//Debug.Log(string.Format("NTP average delay={0:E}sec", AverageDelay()));
			}
		} catch (System.Exception e) {
			Debug.Log(e);
		}
		_udp.BeginReceive(_callback, null);
	}
	
	IEnumerator Request() {
		while (true) {
			yield return new WaitForSeconds(interval);
			try {
				var oscEnc = new nobnak.OSC.MessageEncoder(NTP_REQUEST);
				var t0 = System.BitConverter.GetBytes(IPAddress.HostToNetworkOrder(HighResTime.UtcNow.ToBinary()));
				oscEnc.Add(t0);
				var bytedata = oscEnc.Encode();
				_udp.Send(bytedata, bytedata.Length, _remoteEndpoint);
			} catch (System.Exception e) {
				Debug.Log(e);
			}
		}
	}
	
	void OnDestroy() {
		if (_udp != null) {
			_udp.Close();
			_udp = null;
		}
	}
	
	public struct NtpStat {
		public double delay;
		public double roundtrip;
	}
	
	public double AverageDelay() {
		var count = 0;
		var total = 0.0;
		foreach (var s in stats) {
			if (s.roundtrip <= 0)
				continue;
			count++;
			total += s.delay;
		}
		if (count > 0)
			total /= count;
		return total;
	}
}
