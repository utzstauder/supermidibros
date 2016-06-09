using UnityEngine;
using System.Collections;

public class TransformOnRhythmContinuous : OnRhythmContinuous {

	public bool m_enableTranslation	= false;
	public Vector3 m_translation	= Vector3.zero;

	public bool m_enableRotation	= false;
	public Vector3 m_rotation		= Vector3.zero;

	public bool m_enableScaling		= false;
	public Vector3 m_scale			= Vector3.zero;

	private Vector3 m_initialPosition;
	private Vector3 m_initialRotation;
	private Vector3 m_initialScale;

	// Use this for initialization
	protected override void Awake () {
		base.Awake();

		m_initialPosition	= transform.localPosition;
		m_initialRotation	= transform.localRotation.eulerAngles;
		m_initialScale		= transform.localScale;
	}

	protected override void Action (float _timer)
	{
		base.Action (_timer);

		if (m_enableTranslation){
			transform.localPosition = m_initialPosition + (m_translation * _timer);
		}

		if (m_enableRotation){
			transform.localRotation = Quaternion.Euler(m_initialRotation + (m_rotation * _timer));
		}

		if (m_enableScaling){
			transform.localScale = m_initialScale + (m_scale * _timer);
		}
	}
}
