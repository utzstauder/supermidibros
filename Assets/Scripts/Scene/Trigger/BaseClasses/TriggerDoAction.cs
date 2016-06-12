using UnityEngine;
using System.Collections;

[RequireComponent(typeof(TriggerTarget))]
public class TriggerDoAction : MonoBehaviour {

	private TriggerTarget m_triggerTarget;

	protected virtual void Awake () {
		m_triggerTarget = GetComponent<TriggerTarget>();
	}

	protected virtual void OnEnable (){
		m_triggerTarget.OnReceiveTrigger += Action;
	}

	protected virtual void OnDisable(){
		m_triggerTarget.OnReceiveTrigger -= Action;
	}
		
	protected virtual void Action(){
		
	}
}
