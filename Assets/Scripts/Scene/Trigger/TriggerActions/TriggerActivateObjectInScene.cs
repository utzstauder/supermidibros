using UnityEngine;
using System.Collections;

public class TriggerActivateObjectInScene : TriggerTarget {

	public string targetObjectName = "";
	private GameObject targetObject;

	public bool setActive = false;

	// Use this for initialization
	protected override void Awake () {
		base.Awake();

		targetObject = GameObject.Find(targetObjectName);
	}
	
	protected override void ActionSuccess(Trigger _reference){
		base.ActionSuccess (_reference);

		if (targetObject == null){
			targetObject = GameObject.Find(targetObjectName);

			if (targetObject == null){
				return;
			}
		}
		Debug.Log(targetObject.name);

		targetObject.SetActive(setActive);
		Debug.Log(setActive);

	}

	protected override void ActionFailure(Trigger _reference){
		base.ActionFailure (_reference);

	}
}
