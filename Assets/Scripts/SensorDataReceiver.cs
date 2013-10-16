using UnityEngine;
using System.Collections;
using System.Net.Sockets;
using System.Net;

public class SensorDataReceiver : MonoBehaviour {
	public int portNumber = 10000;
	public Transform handle;
	
	private UdpClient _udp;
	private IPEndPoint _endpoint;
	private keijiro.Osc.Parser _osc;
	private System.AsyncCallback _callback;
	private Quaternion _receivedRotation;
	
	// Use this for initialization
	void Start () {
		_receivedRotation = transform.localRotation;
		
		_osc = new keijiro.Osc.Parser();
		_endpoint = new IPEndPoint(IPAddress.Any, portNumber);
		_udp = new UdpClient(_endpoint);
		
		_callback = new System.AsyncCallback(HandleReceive);
		_udp.BeginReceive(_callback, null);
	}
	
	void Update() {
		handle.localRotation = _receivedRotation;
	}
	
	void HandleReceive(System.IAsyncResult ar) {
		if (_udp == null)
			return;
		
		try { 
			byte[] receivedData = _udp.EndReceive(ar, ref _endpoint);
			_osc.FeedData(receivedData);
			while (_osc.MessageCount > 0) {
				var m = _osc.PopMessage();

				if (m.path == "/sensor/accelerometer") {
					var accel = new Vector3((float)m.data[0], (float)m.data[1], (float)m.data[2]);
					_receivedRotation = Quaternion.FromToRotation(Vector3.down, accel);
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
}
