using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class LockPosInEditor : MonoBehaviour {

	public bool lockX = false;
	public bool lockY = false;
	public bool lockZ = false;

	public Vector3 coordinates = Vector3.zero;

	public bool snapX = false;
	public bool snapY = false;
	public bool snapZ = false;

	public Vector3 snapInterval = Vector3.zero;
	public Vector3 snapOffset = Vector3.zero;

	private float newX = 0;
	private float newY = 0;
	private float newZ = 0;

	// Update is called once per frame
	void Update () {
		if (!Application.isPlaying){
			//
			if (snapX) lockX = false;
			if (snapY) lockY = false;
			if (snapZ) lockZ = false;

			// calculate new coordinates
			if (lockX) newX = coordinates.x;
			else if (snapX) newX = Mathf.RoundToInt(this.transform.position.x / snapInterval.x) * snapInterval.x + snapOffset.x;
			else newX = this.transform.position.x;

			if (lockY) newY = coordinates.y;
			else if (snapY) newY = Mathf.RoundToInt(this.transform.position.y / snapInterval.y) * snapInterval.y + snapOffset.y;
			else newY = this.transform.position.y;

			if (lockZ) newZ = coordinates.z;
			else if (snapZ) newZ = Mathf.RoundToInt(this.transform.position.z / snapInterval.z) * snapInterval.z + snapOffset.z;
			else newZ = this.transform.position.z;

			this.transform.position = new Vector3(newX, newY, newZ);
		}
	}
}
