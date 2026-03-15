using UnityEngine;
using System.Collections;

public class ScaleOnRhythmPeriodicUI : OnRhythmPeriodic {

	public Vector3 m_scale			= Vector3.zero;

	private Vector3 m_initialScale;

	private RectTransform rectTransform;


	protected override void Awake ()
	{
		base.Awake ();

		rectTransform = GetComponent<RectTransform>();
	}

	void OnEnable () {
		m_initialScale = rectTransform.localScale;
	}

	void OnDisable(){
		rectTransform.localScale = m_initialScale;
	}

	protected override void Action (float _timer)
	{
		base.Action (_timer);

		rectTransform.localScale = m_initialScale + (m_scale * _timer);
	}
}
