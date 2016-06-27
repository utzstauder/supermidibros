using UnityEngine;
using System.Collections;

public class TriggerChangeMaterial : TriggerTarget {

	public Renderer m_renderer;
	public Material m_targetMaterialSuccess;
	public Material m_targetMaterialFailure;

	// Use this for initialization
	protected override void Awake () {
		base.Awake();

		if (m_renderer == null &&
			(m_renderer = GetComponent<Renderer>()) == null){
			Debug.LogError("No Renderer component found!");
		}

		// check if assigned renderer is in same hierarchy for security reasons
		foreach (Renderer renderer in transform.GetComponentsInChildren<Renderer>()){
			
		}
	}
	
	protected override void ActionSuccess(Trigger _reference){
		base.ActionSuccess (_reference);

		m_renderer.material = m_targetMaterialSuccess;
	}

	protected override void ActionFailure(Trigger _reference){
		base.ActionFailure (_reference);

		m_renderer.material = m_targetMaterialFailure;
	}

	/**
	 * Will compare the assigned renderer with every renderer found in the hierarchy of this object
	 */
	bool RendererInHierarchy(){
		foreach (Renderer renderer in transform.GetComponentsInChildren<Renderer>()){
			if (renderer == m_renderer){
				return true;
			}
		}
		Debug.LogWarning("Renderer not in hierarchy");
		return false;
	}
}
