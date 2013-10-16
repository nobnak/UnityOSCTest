using UnityEngine;
using System.Collections;

using Encoder = nobnak.OSC.MessageEncoder;
using Decoder = keijiro.Osc.Parser;

public class OscTest : MonoBehaviour {
	
	void Start() {
		Test01();
		Test02();
		Test03();
	}

	static void Test01 () {
		var enc = new Encoder("/screen/texture");
		enc.Add(0);
		enc.Add("Image/Alpha.png");
		
		Dump(enc);
	}

	static void Test02 () {
		var enc = new Encoder("/screen/position");
		enc.Add(0);
		enc.Add(1.1f);
		enc.Add(2.2f);
		enc.Add(3.3f);
		
		Dump(enc);
	}

	static void Test03 () {
		var enc = new Encoder("/screen/blob");
		enc.Add (0);
		var bytedata = System.Text.Encoding.UTF8.GetBytes("0123456789abcdef");
		enc.Add(bytedata, 0, bytedata.Length);
		Dump (enc);
	}

	static void Dump (Encoder enc) {
		var dec = new Decoder();
		dec.FeedData(enc.Encode());
		while (dec.MessageCount > 0) {
			var m = dec.PopMessage();
			Debug.Log(Message2String(m));
		}
	}
	static string Message2String(keijiro.Osc.Message m) {
		var temp = m.path + ":";
		foreach (var o in m.data) {
			if (o.GetType() == typeof(System.Byte[])) {
				temp += System.Text.Encoding.UTF8.GetString((byte[])o) + ":";
			} else {
				temp += o + ":";
			}
		}
		return temp;
	}
}
