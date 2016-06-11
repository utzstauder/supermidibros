using UnityEngine;
using System.Collections;

public class RotateOnRhythmPeriodic : OnRhythmPeriodic {

	public Vector3 m_rotation		= Vector3.zero;

	private Quaternion m_initialRotation;

	// Use this for initialization
	void OnEnable () {
		m_initialRotation = transform.localRotation;
	}

	protected override void Action (float _timer)
	{
		base.Action (_timer);

		transform.localRotation = m_initialRotation * Quaternion.Euler(m_rotation * _timer);
	}
}
