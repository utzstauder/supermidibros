using UnityEngine;
using System.Collections;

public class ToggleChildrenOnRhythm : OnRhythm {

	// Use this for initialization
	protected override void Awake () {
		base.Awake();
	}
	
	public override void Action ()
	{
		base.Action();

		for (int i = 0; i < transform.childCount; i++){
			transform.GetChild(i).gameObject.SetActive(!transform.GetChild(i).gameObject.activeSelf);
		}
	}
}
