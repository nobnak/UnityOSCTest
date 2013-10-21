using UnityEngine;
using System.Collections;
using System.Net.Sockets;
using System.Net;

public class OscNtpClient : MonoBehaviour {
	public string remoteHost = "localhost";
	public int remotePort = 10000;
	public float interval = 1f;
	public int nSamplesPOT = 64;
	public RingBuffer<NtpStat> stats;
	
	private nobnak.OSC.OscClient _client;

	// Use this for initialization
	void Start () {
		stats = new RingBuffer<NtpStat>(nSamplesPOT);
		var address = Dns.GetHostAddresses(remoteHost)[0];
		var serverEndpoint = new IPEndPoint(address, remotePort);
		_client = new nobnak.OSC.OscClient(serverEndpoint);
		_client.OnReceive += HandleReceived;;
		_client.OnError += delegate(System.Exception obj) {
			Debug.Log(obj);
		};
		
		StartCoroutine("Request");
	}
	
	void HandleReceived(keijiro.Osc.Message m) {
		if (m.path != OscNtpServer.NTP_RESPONSE)
			return;
			
		var t3 = HighResTime.UtcNow;
		var t0 = System.DateTime.FromBinary(IPAddress.NetworkToHostOrder(System.BitConverter.ToInt64((byte[])m.data[0], 0)));
		var t1 = System.DateTime.FromBinary(IPAddress.NetworkToHostOrder(System.BitConverter.ToInt64((byte[])m.data[1], 0)));
		var t2 = System.DateTime.FromBinary(IPAddress.NetworkToHostOrder(System.BitConverter.ToInt64((byte[])m.data[2], 0)));
		var roundtrip = NtpUtil.Roundtrip(t0, t1, t2, t3);
		var delay = NtpUtil.Delay(t0, t1, t2, t3);
		stats.Add(new NtpStat() { delay = delay, roundtrip = roundtrip });
	}
	
	IEnumerator Request() {
		while (true) {
			yield return new WaitForSeconds(interval);
			var oscEnc = new nobnak.OSC.MessageEncoder(OscNtpServer.NTP_REQUEST);
			var t0 = System.BitConverter.GetBytes(IPAddress.HostToNetworkOrder(HighResTime.UtcNow.ToBinary()));
			oscEnc.Add(t0);
			var bytedata = oscEnc.Encode();
			_client.Send(bytedata);
		}
	}
	
	void OnDestroy() {
		if (_client != null) {
			_client.Dispose();
			_client = null;
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
