using UnityEngine;
using System.Collections;

public class UtcViewer : MonoBehaviour {
	public const float MILS2ANGLE = 360f / 1000f;
	
	public GUIText text;
	public Transform clock;

	void Start () {
		if (text == null)
			text = guiText;
		UpdateTime();
	}

	// Update is called once per frame
	void Update () {
		UpdateTime();
	}

	void UpdateTime() {
		var now = HighResTime.UtcNow;
		text.text = now.ToString();
		var angle = MILS2ANGLE * now.Millisecond;
		clock.localRotation = Quaternion.AngleAxis(-angle, Vector3.forward);
		//Debug.Log(string.Format("{0}ms {1}angle", now.Millisecond, angle));
	}
	
}
