using UnityEngine;
using System.Collections;

public class TriggerChangeMaterial : TriggerDoAction {

	public Renderer m_renderer;
	public Material m_targetMaterial;

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
	
	protected override void Action ()
	{
		base.Action ();

		m_renderer.material = m_targetMaterial;
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
