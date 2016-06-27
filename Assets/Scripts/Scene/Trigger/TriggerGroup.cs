using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(LineRenderer))]
public class TriggerGroup : Trigger {

	public TriggerSingle[] m_triggers;
	private List<bool> m_triggerList;
	public bool m_lookForTriggersInChildren = true;
	public bool m_moveChildrenAlongXAxis	= false;

	private LineRenderer m_lineRenderer;

	protected override void Awake () {
		base.Awake();

		m_lineRenderer = GetComponent<LineRenderer>();

		m_triggerList = new List<bool>();

		if (m_lookForTriggersInChildren){
			UpdateTriggers();
		}

		// subscribe to trigger events
		if (m_triggers.Length > 0){
			foreach (Trigger trigger in m_triggers){
				trigger.OnTrigger += OnReceiveTrigger;
			}
		}
	}

	protected override void Update () {
		base.Update();

		if (m_inEditMode){
			if (m_lookForTriggersInChildren){
				UpdateTriggers();
			}
			if (m_moveChildrenAlongXAxis){
				MoveChildren();
			}
			DrawLinesBetweenTriggers();
		}
	}

	#region override functions

	protected override void OnReceiveTrigger (Trigger scriptReference, bool success)
	{
		base.OnReceiveTrigger (scriptReference, success);

		m_triggerList.Add(success);

		if (m_triggerList.Count == m_triggers.Length){
			CheckTriggers();
		}
	}

	protected override void OnStop ()
	{
		base.OnStop ();

		m_triggerList = new List<bool>();
	}

	protected override void OnDrawGizmos ()
	{
		base.OnDrawGizmos ();

		if (m_inEditMode){
			if (m_triggers.Length > 0){
				foreach (Trigger trigger in m_triggers){
					Gizmos.DrawLine(transform.position, trigger.transform.position);
				}
			}
		}
	}

	#endregion


	#region trigger functions

	void CheckTriggers(){
		int success = 0;
		for (int i = 0; i < m_triggerList.Count; i++){
			if (m_triggerList[i]){
				success++;
			}
		}

		//Debug.Log(success);

		if (success > 0 && success < m_triggerList.Count){
			TriggerFailure();
		} else if (success == m_triggerList.Count) {
			TriggerSuccess();
		} else {
			// nothing happens
		}
	}

	void TriggerSuccess(){
		//Debug.Log("Success!");
		BroadcastTriggerSuccess();
	}


	// get's called when at least one child was triggered but not all of them
	void TriggerFailure(){
		//Debug.Log("Failure!");
		BroadcastTriggerFailure();
	}

	#endregion


	#region private functions

	void UpdateTriggers(){
		m_triggers = GetComponentsInChildren<TriggerSingle>();
	}

	void MoveChildren(){
		foreach(SnapToGrid snapToGrid in transform.GetComponentsInChildren<SnapToGrid>()){
			snapToGrid.CalculateFromWorldX(Mathf.Round(transform.position.x));
		}
	}

	void DrawLinesBetweenTriggers(){
		if (m_lineRenderer == null){
			m_lineRenderer = GetComponent<LineRenderer>();
		}

		if (m_triggers.Length > 1){
			m_lineRenderer.enabled = true;

			m_lineRenderer.SetVertexCount(m_triggers.Length);
			for (int i = 0; i < m_triggers.Length; i++){
				m_lineRenderer.SetPosition(i, m_triggers[i].transform.position);
			}
		} else {
			m_lineRenderer.enabled = false;
		}
	}

	#endregion
}
