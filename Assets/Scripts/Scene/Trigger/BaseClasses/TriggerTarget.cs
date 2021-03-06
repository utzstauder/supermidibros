﻿using UnityEngine;
using System.Collections;

public class TriggerTarget : MonoBehaviour {

	[SerializeField]
	protected Trigger[] m_triggers;

	public bool lookForTriggersOnParent = true;

	private Color m_gizmosColor = Color.yellow;

	protected virtual void Awake () {
		if (lookForTriggersOnParent){
			if ((m_triggers = transform.parent.GetComponents<Trigger>()) != null){
				foreach (Trigger trigger in m_triggers){
					trigger.OnTrigger += Action;
				}
			}
		} else {
			if (m_triggers.Length <= 0){
				m_triggers = new Trigger[1];
				if ((m_triggers[0] = GetComponent<Trigger>()) == null){
					Debug.LogError("No Trigger attached to this TriggerTarget");
					Destroy(this);
				} else {
					m_triggers[0].OnTrigger += Action;
				}
			} else {
				foreach (Trigger trigger in m_triggers){
					trigger.OnTrigger += Action;
				}
			}
		}
	}

	/**
	 * Is called every time a trigger in m_triggers is triggered.
	 */
	protected virtual void Action(Trigger scriptReference, bool success){
		//Debug.Log("Triggered!");
		if (success){
			ActionSuccess(scriptReference);
		} else {
			ActionFailure(scriptReference);
		}
	}

	protected virtual void ActionSuccess(Trigger scriptReference){
		
	}

	protected virtual void ActionFailure(Trigger scriptReference){

	}

	protected virtual void OnDrawGizmos(){
		Gizmos.color = m_gizmosColor;

//		if (m_triggers.Length > 0){
//			foreach(Trigger trigger in m_triggers){
//				Gizmos.DrawLine(transform.position, trigger.transform.position);
//			}
//		}
		for (int i = 0; i < m_triggers.Length; i++){
			Gizmos.DrawLine(transform.position, m_triggers[i].transform.position);
		}
	}
}
