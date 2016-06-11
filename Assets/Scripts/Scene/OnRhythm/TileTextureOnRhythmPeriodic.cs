using UnityEngine;
using System.Collections;

[RequireComponent(typeof(MeshRenderer))]
public class TileTextureOnRhythmPeriodic : OnRhythmPeriodic {

	private MeshRenderer m_meshRenderer;

	public Vector2 m_textureOffset = Vector2.zero;

	// Use this for initialization
	protected override void Awake () {
		base.Awake();

		m_meshRenderer = GetComponent<MeshRenderer>();
	}

	protected override void Action (float _timer)
	{
		base.Action (_timer);

		m_meshRenderer.material.mainTextureOffset = m_textureOffset * _timer;
	}
}
