using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class TileTextureWithScaling : MonoBehaviour {

	private Renderer[] m_renderer;

	private bool m_inEditMode = true;

	void Awake () {
		if (Application.isPlaying){
			m_inEditMode = false;

			m_renderer = GetComponentsInChildren<Renderer>();

			foreach (Renderer renderer in m_renderer){
				foreach (Material material in renderer.sharedMaterials){
					material.mainTextureScale.Set(1.0f, transform.localScale.x);
				}
			}

		} else {
			m_inEditMode = true;
		}
	}
	
	void Update () {
		if (m_inEditMode){
			m_renderer = GetComponentsInChildren<Renderer>();

			foreach (Renderer renderer in m_renderer){
				foreach (Material material in renderer.sharedMaterials){
					material.mainTextureScale.Set(1.0f, transform.localScale.x);
				}
			}
		}
	}
}
