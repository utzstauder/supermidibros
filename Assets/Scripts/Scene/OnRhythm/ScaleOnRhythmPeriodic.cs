using UnityEngine;
using System.Collections;

public class ScaleOnRhythmPeriodic : OnRhythmPeriodic {

	public Vector3 m_scale			= Vector3.zero;

	private Vector3 m_initialScale;

	// Use this for initialization
	void OnEnable () {
		m_initialScale = transform.localScale;
	}

	protected override void Action (float _timer)
	{
		base.Action (_timer);

		transform.localScale = m_initialScale + (m_scale * _timer);
	}
}
