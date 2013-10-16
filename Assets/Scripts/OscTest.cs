using UnityEngine;
using System.Collections;

using Encoder = nobnak.OSC.MessageEncoder;
using Decoder = keijiro.Osc.Parser;

public class OscTest : MonoBehaviour {
	
	void Start() {
		Test01();
		Test02();
	}

	static void Test01 () {
		var enc = new Encoder("/screen/texture");
		enc.Add(0);
		enc.Add("Image/Alpha.png");
		
		Dump(enc);
	}

	static void Test02 () {
		var enc = new Encoder("screen/position");
		enc.Add(0);
		enc.Add(1.1f);
		enc.Add(2.2f);
		enc.Add(3.3f);
		
		Dump(enc);
	}

	static void Dump (Encoder enc) {
		var dec = new Decoder();
		dec.FeedData(enc.Encode());
		while (dec.MessageCount > 0) {
			var m = dec.PopMessage();
			Debug.Log(m);
		}
	}
}
