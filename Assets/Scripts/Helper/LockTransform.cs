using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class LockTransform : MonoBehaviour {

	public bool m_lockPosition	= true;
	public bool m_lockRotation	= true;
	public bool m_lockScale		= true;

	public bool m_lockInPlayMode	= false;

	void Update () {
		if ((!Application.isPlaying) || (Application.isPlayer && m_lockInPlayMode)){
			if (m_lockPosition) transform.localPosition = Vector3.zero;
			if (m_lockRotation) transform.localRotation = Quaternion.identity;
			if (m_lockScale)	transform.localScale	= Vector3.one;
		}
	}
}
