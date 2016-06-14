using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class PrefabGroup : MonoBehaviour {

	public GameObject m_prefab;
	public int m_instances		= 0;

	public Vector3 m_offsetPosition		= Vector3.zero;
	public Vector3 m_offsetRotation		= Vector3.zero;

	/**
	 * Instantiates _numberOfInstances prefabs as children of this transform
	 */
	public void InstantiatePrefabs(int _numberOfInstances){
		if (m_prefab == null){
			Debug.LogError("No prefab defined");
		} else {
			for (int i = 0; i < m_instances; i++){
				GameObject instance = Instantiate(m_prefab,
					transform.position + (m_offsetPosition * i),
					Quaternion.identity * Quaternion.Euler(m_offsetRotation * i)) as GameObject;
				instance.transform.parent = this.transform;
			}
		}
	}

	/**
	 * Deletes every child in hierarchy
	 */
	public void ClearPrefabs(){
		while (transform.childCount > 0){
			DestroyImmediate(transform.GetChild(0).gameObject);
		}
	}
}
