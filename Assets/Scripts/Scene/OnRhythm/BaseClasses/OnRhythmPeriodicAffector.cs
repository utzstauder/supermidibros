using UnityEngine;
using System.Collections;

public class OnRhythmPeriodicAffector : MonoBehaviour {

	protected bool active = false;
	protected float affectorAmount = 0;
	public float lerpSpeed = 10.0f;

	// Use this for initialization
	protected virtual void Awake () {
	
	}
	
	// Update is called once per frame
	protected virtual void Update () {
		if (active){
			affectorAmount = Mathf.Lerp(affectorAmount, 1.0f, Time.deltaTime * lerpSpeed);
		} else {
			affectorAmount = Mathf.Lerp(affectorAmount, 0, Time.deltaTime * lerpSpeed);
		}
	}

	public virtual float GetMultiplier(){
		return affectorAmount;
	}
}
