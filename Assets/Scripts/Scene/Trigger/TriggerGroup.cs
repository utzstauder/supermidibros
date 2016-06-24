using UnityEngine;
using System.Collections;

[RequireComponent(typeof(LineRenderer))]
public class TriggerGroup : Trigger {

	public TriggerSingle[] m_triggers;
	public bool m_lookForTriggersInChildren = true;
	public bool m_moveChildrenAlongXAxis	= false;
	private int m_childrenTriggered;

	private LineRenderer m_lineRenderer;

	protected override void Awake () {
		base.Awake();

		m_lineRenderer = GetComponent<LineRenderer>();

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

	protected override void OnReceiveTrigger (Trigger _reference)
	{
		base.OnReceiveTrigger (_reference);
		m_childrenTriggered++;
		if (m_childrenTriggered >= m_triggers.Length){
			BroadcastTrigger();
		}
	}

	protected override void OnStop ()
	{
		base.OnStop ();

		m_childrenTriggered = 0;
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
