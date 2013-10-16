using UnityEngine;
using System.Collections;
using System.Net.Sockets;
using System.Net;

public class SensorDataSender : MonoBehaviour {
	public int remotePort;
	public Transform handle;
	
	private UdpClient _udp;
	private IPEndPoint _endpoint;
	
	// Use this for initialization
	void Start () {
		_endpoint = new IPEndPoint(IPAddress.Broadcast, remotePort);
		_udp = new UdpClient();
	}
	
	// Update is called once per frame
	void Update () {
		var enc = new nobnak.OSC.MessageEncoder("/sensor/accelerometer");
		var accel = Input.acceleration;
		enc.Add(accel.x);
		enc.Add(accel.y);
		enc.Add(accel.z);
		
		try {
			var bytedata = enc.Encode();
			_udp.Send(bytedata, bytedata.Length, _endpoint);
		} catch (System.Exception e) { 
			Debug.Log(e);
		}
		handle.localRotation = Quaternion.FromToRotation(Vector3.down, accel);
	}
}
