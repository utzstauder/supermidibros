using UnityEngine;
using System.Collections;

public class EmitParticlesOnRhythm : OnRhythm {

	public int particlesToEmit = 100;

	private ParticleSystem[] particleSystems;

	protected override void Awake () {
		base.Awake();

		particleSystems = GetComponentsInChildren<ParticleSystem>();
	}
	
	public override void Action ()
	{
		base.Action();

		for (int i = 0; i < particleSystems.Length; i++){
			particleSystems[i].Emit(particlesToEmit);
		}
	}
}
