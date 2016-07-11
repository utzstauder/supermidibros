using UnityEngine;
using System.Collections;

public class TriggerChangeSkyboxMaterial : TriggerTarget {

	public Material targetSkyboxMaterial;

	// Use this for initialization
	protected override void Awake () {
		base.Awake();

	}
	
	protected override void ActionSuccess(Trigger _reference){
		base.ActionSuccess (_reference);
		if (targetSkyboxMaterial != null){
			Debug.Log("changing material");
			RenderSettings.skybox = targetSkyboxMaterial;
			DynamicGI.UpdateEnvironment();
		}
	}

	protected override void ActionFailure(Trigger _reference){
		base.ActionFailure (_reference);

	}
}
