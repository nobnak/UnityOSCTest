using UnityEngine;
using nobnak.OSC;
using System.Net;
using System.Collections.Generic;

public class Communicator : MonoBehaviour {
	public const string PATH = "/transform";
	
	public int portNumber = 10000;
	
	private OscServer _server;
	private int _appId;
	private IPEndPoint _clientEndpoint;
	private Dictionary<int, Tracker> _id2tracker;
	private Queue<ReceivedMatrix> _matrixQueue;
	
	public void Send(Tracker tracker) {
		var enc = new MessageEncoder(PATH);
		enc.Add(_appId);
		enc.Add(tracker.uniqueId);
		enc.Add(Pack(tracker.transform));
		_server.Send(enc.Encode(), _clientEndpoint);
	}
	
	public void BindTracker(Tracker tr) {
		_id2tracker.Add(tr.uniqueId, tr);
	}

	void Awake() {
		_id2tracker = new Dictionary<int, Tracker>();
		_matrixQueue = new Queue<ReceivedMatrix>();
		_appId = GetInstanceID();
		_clientEndpoint = new IPEndPoint(IPAddress.Broadcast, portNumber);
		_server = new OscServer(new IPEndPoint(IPAddress.Any, portNumber));
		
		_server.OnError += delegate(System.Exception obj) {
			Debug.Log("Error on Send : " + obj);
		};
		_server.OnReceive += delegate(keijiro.Osc.Message obj, IPEndPoint end) {
			try {
				var appId = (int)obj.data[0];
				var trackerId = (int)obj.data[1];
				var data = (byte[])obj.data[2];
				lock (_matrixQueue) {
					_matrixQueue.Enqueue(new ReceivedMatrix(appId, trackerId, data));				
				}
			} catch (System.Exception e) {
				Debug.Log(e);
			}
		};
	}
	
	void Update() {
		lock (_matrixQueue) {
			while (_matrixQueue.Count > 0) {
				var rm = _matrixQueue.Dequeue();
				Tracker tracker;
				if (rm.appId == _appId || !_id2tracker.TryGetValue(rm.trackerId, out tracker))
					continue;
				Unpack(rm.data, tracker.transform);
			}
		}
	}
	
	void OnDestroy() {
		if (_server != null) {
			_server.Dispose();
			_server = null;
		}
	}
	
	byte[] Pack(Transform transform) {
		var data = new byte[10 * 4];
		var union32 = new MessageEncoder.Union32();
		
		var pos = transform.localPosition;
		var scl = transform.localScale;
		var rot = transform.localRotation;
		for (var i = 0; i < 3; i++) {
			union32.floatdata = pos[i];
			union32.Pack(data, i * 4);
		}
		for (var i = 0; i < 3; i++) {
			union32.floatdata = scl[i];
			union32.Pack(data, 12 + i * 4);
		}
		for (var i = 0; i < 4; i++) {
			union32.floatdata = rot[i];
			union32.Pack(data, 24 + i * 4);
		}
		
		return data;
	}
	
	void Unpack(byte[] data, Transform outTransform) {
		var union32 = new MessageEncoder.Union32();
		var pos = Vector3.zero;
		var scl = Vector3.zero;
		var rot = Quaternion.identity;
		
		for (var i = 0; i < 3; i++) {
			union32.Unpack(data, i * 4);
			pos[i] = union32.floatdata;
		}
		for (var i = 0; i < 3; i++) {
			union32.Unpack(data, 12 + i * 4);
			scl[i] = union32.floatdata;
		}
		for (var i = 0; i < 4; i++) {
			union32.Unpack(data, 24 + i * 4);
			rot[i] = union32.floatdata;
		}
		outTransform.localPosition = pos;
		outTransform.localScale = scl;
		outTransform.localRotation = rot;
	}
	
	struct ReceivedMatrix {
		public int appId;
		public int trackerId;
		public byte[] data;
		
		public ReceivedMatrix(int appId, int trackerId, byte[] data) {
			this.appId = appId;
			this.trackerId = trackerId;
			this.data = data;
		}
	}
}
