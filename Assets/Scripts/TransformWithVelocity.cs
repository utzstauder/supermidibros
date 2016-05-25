using UnityEngine;
using System.Collections;
using MidiJack;

public class TransformWithVelocity : MonoBehaviour {

	public Vector3 translate = Vector3.zero;
	public Vector3 rotate = Vector3.zero;
	public Vector3 scale = Vector3.zero;

	public int noteNumber = 0;

	private Vector3 initialLocalPosition;
	private Vector3 initialLocalRotation;
	private Vector3 initialLocalScale;

	// Use this for initialization
	void Awake () {
		initialLocalPosition = transform.localPosition;
		initialLocalRotation = transform.localRotation.eulerAngles;
		initialLocalScale = transform.localScale;
	}
	
	// Update is called once per frame
	void Update () {
		transform.localPosition = initialLocalPosition + (new Vector3(translate.x, translate.y, translate.z) * MidiMaster.GetKey(noteNumber));
		transform.localRotation = Quaternion.Euler(initialLocalRotation + (new Vector3(rotate.x, rotate.y, rotate.z) * MidiMaster.GetKey(noteNumber)));
		transform.localScale = initialLocalScale + (new Vector3(scale.x, scale.y, scale.z) * MidiMaster.GetKey(noteNumber));
	}
}
