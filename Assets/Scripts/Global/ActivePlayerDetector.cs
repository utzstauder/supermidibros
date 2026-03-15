using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Tracks which input channels have moved recently. Used only when variable player count is enabled.
/// A slot is "active" if its fader has moved within the last inactiveThresholdSeconds.
/// </summary>
public class ActivePlayerDetector : MonoBehaviour {

	public static ActivePlayerDetector instance;

	[Tooltip("Seconds without movement before a channel is considered inactive.")]
	[Range(3f, 30f)]
	public float inactiveThresholdSeconds = 8f;

	private float[] lastActivityTime = new float[Constants.NUMBER_OF_PLAYERS];
	private float[] lastKnownPosition = new float[Constants.NUMBER_OF_PLAYERS];
	private int[] cachedActiveSlotIndices = new int[Constants.NUMBER_OF_PLAYERS];
	private int cachedActiveCount = Constants.NUMBER_OF_PLAYERS;
	private bool cacheValid = false;

	const int MIN_ACTIVE_PLAYERS = 2;
	const int MAX_ACTIVE_PLAYERS = Constants.NUMBER_OF_PLAYERS;

	void Awake() {
		if (Application.isPlaying) {
			if (instance == null) {
				instance = this;
			} else {
				Destroy(gameObject);
			}
		}
	}

	void Start() {
		// Start with all 8 considered active (so all planes visible until they stop moving)
		float t = Time.time;
		for (int i = 0; i < Constants.NUMBER_OF_PLAYERS; i++) {
			lastActivityTime[i] = t;
			lastKnownPosition[i] = MIDIInputManager.instance != null ? MIDIInputManager.instance.GetInputOfPlayer(i) : 0.5f;
		}
		cacheValid = false;
	}

	void Update() {
		if (MIDIInputManager.instance == null) return;

		for (int i = 0; i < Constants.NUMBER_OF_PLAYERS; i++) {
			float current = MIDIInputManager.instance.GetInputOfPlayer(i);
			if (current != lastKnownPosition[i]) {
				lastActivityTime[i] = Time.time;
				lastKnownPosition[i] = current;
				cacheValid = false;
			}
		}
	}

	/// <summary>
	/// Number of slots that have moved recently (clamped 2-8). Only meaningful when variable player mode is on.
	/// </summary>
	public int GetActivePlayerCount() {
		if (!cacheValid) RebuildCache();
		return cachedActiveCount;
	}

	/// <summary>
	/// Sorted list of slot indices that are currently active (length = GetActivePlayerCount()). Only meaningful when variable player mode is on.
	/// </summary>
	public int[] GetActiveSlotIndices() {
		if (!cacheValid) RebuildCache();
		int[] result = new int[cachedActiveCount];
		for (int i = 0; i < cachedActiveCount; i++)
			result[i] = cachedActiveSlotIndices[i];
		return result;
	}

	void RebuildCache() {
		float now = Time.time;
		int count = 0;
		for (int i = 0; i < Constants.NUMBER_OF_PLAYERS; i++) {
			if ((now - lastActivityTime[i]) <= inactiveThresholdSeconds) {
				cachedActiveSlotIndices[count] = i;
				count++;
			}
		}
		cachedActiveCount = Mathf.Clamp(count, MIN_ACTIVE_PLAYERS, MAX_ACTIVE_PLAYERS);

		// If we clamped down (e.g. only 1 was active), we still need to fill with the most recently active
		if (count < MIN_ACTIVE_PLAYERS) {
			// Sort all slots by lastActivityTime descending and take first MIN_ACTIVE_PLAYERS
			List<KeyValuePair<int, float>> list = new List<KeyValuePair<int, float>>();
			for (int i = 0; i < Constants.NUMBER_OF_PLAYERS; i++)
				list.Add(new KeyValuePair<int, float>(i, lastActivityTime[i]));
			list.Sort((a, b) => b.Value.CompareTo(a.Value));
			cachedActiveCount = MIN_ACTIVE_PLAYERS;
			for (int i = 0; i < MIN_ACTIVE_PLAYERS; i++)
				cachedActiveSlotIndices[i] = list[i].Key;
			System.Array.Sort(cachedActiveSlotIndices, 0, cachedActiveCount);
		} else if (count > MAX_ACTIVE_PLAYERS) {
			cachedActiveCount = MAX_ACTIVE_PLAYERS;
		}
		cacheValid = true;
	}
}
