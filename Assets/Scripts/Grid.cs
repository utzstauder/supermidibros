﻿using UnityEngine;
using System.Collections;

public class Grid : MonoBehaviour {

	[Header("References")]
	public AudioManager m_audioManager;			// will override time signature settings, if set

	[Header("Grid Settings")]
	[Range(1,8)]
	public int 	m_unitsPerBeat 			= 4;
	[Range(1,32)]
	public int m_lineWidth				= 8;
	public int 	m_drawRange				= 1024;

	[Header("Time Signature")]
	[Range(1,8)]
	public int	m_timeSignatureUpper	= 4;
	[Range(1,8)]
	public int 	m_timeSignatureLower	= 4;

	[Header("Grid Colors")]
	public Color m_colorBars 			= Color.red;
	public Color m_colorBeats			= Color.green;

	void OnDrawGizmos(){
		if (m_audioManager){
			m_timeSignatureUpper = m_audioManager.GetTimeSignatureUpper();
			m_timeSignatureLower = m_audioManager.GetTimeSignatureLower();
			m_unitsPerBeat = m_audioManager.GetUnitsPerBeat();
			m_drawRange = (int)(m_audioManager.GetClipLength() / 60.0f * (float)m_audioManager.m_bpm * (float)m_unitsPerBeat);
		}

		// draw beats and bars
		int units = m_unitsPerBeat * m_timeSignatureUpper/m_timeSignatureLower;
		for (int x = 0; x < m_drawRange; x += units){
			Gizmos.color = m_colorBeats;
			if ((x/units)%m_timeSignatureUpper == 0){
				Gizmos.color = m_colorBars;
			}
			Gizmos.DrawLine(new Vector3(x, 0, -m_lineWidth/2), new Vector3(x, 0, m_lineWidth/2));
		}
	}
}
