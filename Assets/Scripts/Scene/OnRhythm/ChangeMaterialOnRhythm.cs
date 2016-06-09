using UnityEngine;
using System.Collections;

[RequireComponent(typeof(MeshRenderer))]
public class ChangeMaterialOnRhythm : OnRhythm {
	
	public Material[] m_materials;
	private int 	m_materialCounter	= 0;

	private MeshRenderer m_meshRenderer;


	// Use this for initialization
	protected override void Awake () {
		base.Awake();

		m_meshRenderer = GetComponent<MeshRenderer>();
	}

	protected override void OnEnable(){
		base.OnEnable();

		if (m_materials.Length >= 1){
			m_meshRenderer.material = m_materials[0];
		}

		m_materialCounter = 0;
	}

	protected override void OnDisable(){
		base.OnDisable();
	}
		

	#region private functions

	public override void Action(){
		base.Action();

		m_materialCounter = (m_materialCounter + 1) % m_materials.Length;
		m_meshRenderer.material = m_materials[m_materialCounter];
	}

	#endregion
}
