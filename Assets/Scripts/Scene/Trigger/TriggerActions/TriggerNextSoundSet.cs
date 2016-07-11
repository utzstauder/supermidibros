using UnityEngine;
using System.Collections;

public class TriggerNextSoundSet : TriggerTarget {

	public bool keepPlaying;
	private AudioManager audioManager;

	// Use this for initialization
	protected override void Awake () {
		base.Awake();
	
		audioManager = GameObject.Find("AudioManager").GetComponent<AudioManager>();
		if (audioManager == null){
			Debug.LogError("No AudioManager found in scene!");
			this.enabled = false;
		} 
	}
	
	protected override void ActionSuccess(Trigger _reference){
		base.ActionSuccess (_reference);
		audioManager.NextSoundSet(keepPlaying);
	}

	protected override void ActionFailure(Trigger _reference){
		base.ActionFailure (_reference);

	}
}
