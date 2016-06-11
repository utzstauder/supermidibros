using UnityEngine;
using System.Collections;

public class TranslateOnRhythmPeriodic : OnRhythmPeriodic {

	public Vector3 m_translation	= Vector3.zero;

	private Vector3 m_initialPosition;

	// Use this for initialization
	void OnEnable () {
		m_initialPosition = transform.localPosition;
	}

	protected override void Action (float _timer)
	{
		base.Action (_timer);

		transform.localPosition = m_initialPosition + (m_translation * _timer);
	}
}
