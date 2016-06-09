using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class Trigger : MonoBehaviour {

	private bool m_inEditMode = true;

	private float m_targetX;
	private float m_targetY	= 10.0f;
	private float m_targetZ = 0;

	private Trigger[] m_triggerChildren;
	private int m_childCount;
	private int m_childrenTriggered;

	public delegate void TriggerDelegate(Trigger _reference);
	public event TriggerDelegate OnTrigger;

	private AudioManager m_audioManager;

	private Color m_gizmosColor = Color.yellow;

	// Use this for initialization
	void Awake () {
		if (Application.isPlaying){
			m_inEditMode = false;
		} else {
			m_inEditMode = true;
		}

		m_audioManager = GameObject.Find("AudioManager").GetComponent<AudioManager>();
		if (m_audioManager == null){
			Debug.LogError("No AudioManager found in scene!");
		} else {
			m_audioManager.OnStop += Reset;
		}

		m_triggerChildren = transform.GetComponentsInChildren<Trigger>();	// will also 'get' every trigger component on the parent object!
		m_childCount = m_triggerChildren.Length;

		// if there are children attached, this object will serve as a triggergroup
		if (m_childCount > 1){
//			Debug.Log("Found " + m_childCount + " Trigger in children");

			m_childrenTriggered = 0;

			foreach (Trigger child in m_triggerChildren){
				if (child != this){
					child.OnTrigger += OnReceiveTrigger;
				} else {
//					Debug.Log("found (and skipped) self reference");
				}
			}
		} else {
			// otherwise this is a solo trigger
//			Debug.Log("Solo trigger");


		}
	}

	void Update(){
		if (m_inEditMode){
			m_triggerChildren = transform.GetComponentsInChildren<Trigger>();
			m_childCount = m_triggerChildren.Length;

			if (m_childCount > 1){
				// update position of parent
				// TODO: this causes unwanted behaviour, fix or delete this part

//				m_targetX = 0;
//				for (int i = 1; i < m_childCount; i++){
//					m_targetX += m_triggerChildren[i].transform.position.x;
//				}
//				m_targetX /= m_childCount;

				//transform.position = new Vector3(m_targetX, m_targetY, m_targetZ);
			}
		}
	}

	void OnDestroy(){
		OnTrigger = null;
	}

	/**
	 * This will check if every child has been triggered
	 */ 
	void OnReceiveTrigger(Trigger _reference){
		m_childrenTriggered++;
		if (m_childrenTriggered >= m_childCount - 1){
			BroadcastTrigger();
		}
	}


	/**
	 * This function will be called everytime the player stops audio playback
	 * used for debugging only
	 */
	void Reset(){
		m_childrenTriggered = 0;
	}


	#region public functions

	/**
	 * This will send a trigger event to every target this trigger is attached to
	 */
	public void BroadcastTrigger(){
		if (OnTrigger != null) {
			OnTrigger(this);
			//Destroy(this);
		}
	}

	/**
	 * This gets called whenever a player object collides with this trigger
	 */
	public void OnCollision(int _playerId){
		Debug.Log("Collision with Player " + _playerId);

		BroadcastTrigger();
	}

	#endregion


	#region gizmos

	void OnDrawGizmos(){
		Gizmos.color = m_gizmosColor;

		if (m_inEditMode){
			foreach (Trigger child in m_triggerChildren){
				if (child != this){
					Gizmos.DrawLine(transform.position, child.transform.position);
				}
			}
		}
	}

	#endregion
}
