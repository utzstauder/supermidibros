using UnityEngine;
using System.Collections;

public class TriggerTarget : MonoBehaviour {

	public Trigger[] m_trigger;

	public delegate void TriggerTargetDelegate();
	public event TriggerTargetDelegate OnReceiveTrigger;

	private Color m_gizmosColor = Color.yellow;

	void Awake () {
		if (m_trigger.Length <= 0){
			m_trigger = new Trigger[1];
			if ((m_trigger[0] = GetComponent<Trigger>()) == null){
				Debug.LogError("No Trigger attached to this TriggerTarget");
				Destroy(this);
			}
		}

		foreach (Trigger trigger in m_trigger){
			trigger.OnTrigger += OnTriggerEvent;
		}
	}
	
	// Update is called once per frame
	void OnDestroy () {
		OnReceiveTrigger = null;
	}

	void OnTriggerEvent(Trigger _reference){
		// do stuff here
//		Debug.Log("Triggered by " + _reference.gameObject.name + "!");

		if (OnReceiveTrigger != null){
			OnReceiveTrigger();
		}

		//Destroy(this); // once we are done
	}

	void OnDrawGizmos(){
		Gizmos.color = m_gizmosColor;
		if (m_trigger.Length > 0){
			foreach(Trigger trigger in m_trigger){
				Gizmos.DrawLine(transform.position, trigger.transform.position);
			}
		}
	}
}
