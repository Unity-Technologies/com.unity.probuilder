using UnityEngine;
using System.Collections;

public class PBScriptTest : MonoBehaviour {
	
	//the object to animate when this trigger is hit
	public Animation animationTarget;
	
	//when this trigger is hit...
	void OnTriggerEnter () {
		//...make an object animate
		animationTarget.Play();
	}
}
