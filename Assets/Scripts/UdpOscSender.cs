using UnityEngine;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Collections;

public class UdpOscSender : MonoBehaviour {
	public string remoteHost;
	public int remotePort;
	
	private IPEndPoint _endpoint;
	private UdpClient _udp;
	private Queue<byte[]> _oscPackets;

	// Use this for initialization
	void Start () {
		_oscPackets = new Queue<byte[]>();
		_udp = new UdpClient();
		var remoteAddress = Dns.GetHostAddresses(remoteHost)[0];
		_endpoint = new IPEndPoint(remoteAddress, remotePort);
		
		StartCoroutine("Test");
	}
	
	// Update is called once per frame
	void Update () {
		try {
			while (_oscPackets.Count > 0) {
				var bytedata = _oscPackets.Dequeue();
				Debug.Log("Send");
				_udp.Send(bytedata, bytedata.Length, _endpoint);
			}
		} catch(System.Exception e) {
			Debug.Log(e);
		}
	}
	
	public void AddMessage(nobnak.OSC.MessageEncoder me) {
		_oscPackets.Enqueue(me.Encode());
	}
		
	IEnumerator Test() {
		while (true) {
			yield return new WaitForSeconds(1f);
			var enc = new nobnak.OSC.MessageEncoder("/test/path");
			enc.Add(Random.Range(int.MinValue, int.MaxValue));
			AddMessage(enc);
		}
	}
}
