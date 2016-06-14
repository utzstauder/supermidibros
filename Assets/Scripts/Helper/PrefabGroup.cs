using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif

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
			#if UNITY_EDITOR
			for (int i = 0; i < m_instances; i++){
				GameObject instance = PrefabUtility.InstantiatePrefab(m_prefab) as GameObject;
				instance.transform.position = transform.position + (m_offsetPosition * i);
				instance.transform.rotation = Quaternion.identity * Quaternion.Euler(m_offsetRotation * i);
				instance.transform.parent = this.transform;
			}
			#endif
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
