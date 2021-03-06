﻿using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class PatternChildControll : MonoBehaviour {

	public GameObject[] audioCategoryObjects = new GameObject[Constants.AUDIO_CATEGORIES];
	private Renderer[] rendererInChildren;
	private ParticleSystem particleSystem;
	private ParticleSystemRenderer particleRenderer;
	public Material particlesMaterialSuccess;
	public Material particlesMaterialFailure;

	public int particlesToEmit = 100;

	void Awake(){
		rendererInChildren = GetComponentsInChildren<Renderer>();
		particleSystem = GetComponentInChildren<ParticleSystem>();
		particleRenderer = GetComponentInChildren<ParticleSystemRenderer>();
	}

	public void SetLocalPositionInGrid(int horizontal, int vertical){
		transform.localPosition = SnapToGrid.GridToWorldCoord(0, vertical, horizontal);
	}

	public void SetActiveMeshObject(int index){
		for (int i = 0; i < audioCategoryObjects.Length; i++){
			audioCategoryObjects[i].SetActive(i == index);
		}
	}

	public void SetMeshColor(Color color){
		for (int i = 0; i < rendererInChildren.Length; i++){
			if (Application.isPlaying){
				rendererInChildren[i].material.color = color;
			} else {
				rendererInChildren[i].sharedMaterial.color = color;
			}
		}
	}

	public void EmitParticles(){
		particleSystem.Emit(particlesToEmit);
	}

	public void EmitParticles(Color color){
		particleSystem.startColor = color;
		EmitParticles();
	}

	public void SetParticlesMaterial(bool success){
		particleRenderer.material = (success) ? particlesMaterialSuccess : particlesMaterialFailure;
	}

	public void SetParticleGravityScale(float value){
		particleSystem.gravityModifier = value;
	}

	public void SetRendererActive(bool active){
		for (int i = 0; i < audioCategoryObjects.Length; i++){
			audioCategoryObjects[i].SetActive(active);
		}
	}
}
