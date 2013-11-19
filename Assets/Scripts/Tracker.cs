using UnityEngine;
using System.Collections;
using TouchScript.Gestures;

public class Tracker : MonoBehaviour {
	public int uniqueId;
	public Communicator communicator;
	
	private PanGesture _pan;

	// Use this for initialization
	void Start () {
		communicator.BindTracker(this);
		
		_pan = GetComponent<PanGesture>();
		_pan.StateChanged += delegate(object sender, TouchScript.Events.GestureStateChangeEventArgs e) {
			switch (e.State) {
			case Gesture.GestureState.Changed:
				transform.position += _pan.WorldDeltaPosition;
				communicator.Send(this);
				break;
			}
		};
	}
}
