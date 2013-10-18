using UnityEngine;
using System.Collections;

public class UtcClock : MonoBehaviour {
	public const float MILS2ANGLE = 360f / 1000f;
	
	public OscNtpClient client;
	public GUIText text;
	public Transform clock;
	public AudioSource beat;
	
	private bool _synch = true;

	void Start () {
		if (text == null)
			text = guiText;
	}

	// Update is called once per frame
	void Update () {
		UpdateTime();
	}
	
	void OnGUI() {
		_synch = GUILayout.Toggle(_synch, "Sync");
	}

	void UpdateTime() {
		var now = HighResTime.UtcNow;
		var delay = client.AverageDelay();
		if (_synch)
			now = now.AddSeconds(delay);
		
		var playPos = now.Millisecond * 1.0e-3f;
		if (playPos < beat.clip.length)
			Play(playPos);
		
		text.text = string.Format("{0}\nDelay {1:E3}", now, delay);
		var angle = MILS2ANGLE * now.Millisecond;
		clock.localRotation = Quaternion.AngleAxis(-angle, Vector3.forward);
	}
	
	void Play(float seek) {
		if (beat.isPlaying)
			return;
		
		beat.time = seek;
		beat.Play();
	}
}
