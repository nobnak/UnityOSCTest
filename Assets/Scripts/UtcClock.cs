using UnityEngine;
using System.Collections;

public class UtcClock : MonoBehaviour {
	public const float MILS2ANGLE = 360f / 1000f;
	public const long SECOND_IN_TICKS = 10000000L;
	public const long MILLISECOND_IN_TICKS = 10000L;
	public const double TICKS2SECOND = 1e-7;
	
	public OscNtpClient client;
	public GUIText text;
	public Transform clock;
	public BeatInfo[] beats;
	
	private bool _synch = true;

	void Start () {
		if (text == null)
			text = guiText;
		
		var dspEndTime = AudioSettings.dspTime;
		foreach (var bi in beats) {
			bi.source = gameObject.AddComponent<AudioSource>();
			bi.source.bypassEffects = true;
			bi.source.clip = bi.clip;
			bi.dspEndTime = dspEndTime;
		}
	}
	
	void OnGUI() {
		_synch = GUILayout.Toggle(_synch, "Sync");
	}

	void Update() {
		var now = HighResTime.UtcNow;
		var delay = client.AverageDelay();
		if (_synch)
			now = now.AddSeconds(delay);
		
		text.text = string.Format("{0}\nDelay {1:E3}", now, delay);
		var angle = MILS2ANGLE * now.Millisecond;
		clock.localRotation = Quaternion.AngleAxis(-angle, Vector3.forward);
		
		var dspTime = AudioSettings.dspTime;
		foreach (var bi in beats) {
			if (bi.dspEndTime < dspTime) {
				var intervalInTicks = bi.intervalInMills * MILLISECOND_IN_TICKS;
				var dt = (intervalInTicks - (now.Ticks % intervalInTicks)) *TICKS2SECOND;
				var startDspTime = dspTime + dt;
				bi.dspEndTime = startDspTime + bi.source.clip.length;
				bi.source.PlayScheduled(startDspTime);
			}
		}
	}
	
	[System.Serializable]
	public class BeatInfo {
		public int intervalInMills = 1000;
		public AudioClip clip;
		[HideInInspector]
		public AudioSource source;
		[HideInInspector]
		public double dspEndTime;
	}
}
