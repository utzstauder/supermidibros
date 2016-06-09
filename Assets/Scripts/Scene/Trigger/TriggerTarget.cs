using UnityEngine;
using System.Collections;

public class TriggerTarget : MonoBehaviour {

	public Trigger[] m_trigger;

	public delegate void TriggerTargetDelegate();
	public event TriggerTargetDelegate OnReceiveTrigger;

	private Color m_gizmosColor = Color.yellow;

	// Use this for initialization
	void Start () {
		if (m_trigger.Length <= 0){
			Debug.LogError("No Trigger attached to this TriggerTarget");
			Destroy(this);
		} else {
			foreach (Trigger trigger in m_trigger){
				trigger.OnTrigger += OnTriggerEvent;
			}
		}
	}
	
	// Update is called once per frame
	void OnDestroy () {
		OnReceiveTrigger = null;
	}

	void OnTriggerEvent(Trigger _reference){
		// do stuff here
		Debug.Log("Triggered by " + _reference.gameObject.name + "!");

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
