

using UnityEngine;
using mvmt = PathwaysEngine.Movement;

public class Pause : MonoBehaviour {
	private static Pause rPause = null;
	private static Pause rInstance {
		get {
			if (rPause==null) {
				rPause = GameObject.FindObjectOfType(typeof(Pause)) as Pause;
				if (rPause==null) rPause = new GameObject("Pause").AddComponent<Pause>();
			} return rPause;
		}
	}

	private void Awake() {
		if (rPause==null) {
			rPause = this as Pause;
			rInstance.init();
		}
	}

	mvmt::Look[] allLooks;

	public void init() {
		allLooks = FindObjectsOfType(typeof (mvmt::Look)) as mvmt::Look[];
		PausePlayer(false);
	}

	public static void PausePlayer(bool doPause, int toTime) {
		foreach (mvmt::Look iterLook in rInstance.allLooks)
			if (iterLook) iterLook.enabled = !doPause;
		Cursor.visible = doPause;
		Cursor.lockState = (!doPause)?(CursorLockMode.Locked):(CursorLockMode.None);
		Time.timeScale = doPause ? (toTime) : (1);
	}

	public static void PausePlayer(bool doPause) {
		if (doPause) {
			foreach (mvmt::Look iterLook in rInstance.allLooks)
				iterLook.enabled = false;
			Cursor.visible = true;
			Cursor.lockState = CursorLockMode.None;
			Time.timeScale = .5f;
		} else {
			foreach (mvmt::Look iterLook in rInstance.allLooks)
				iterLook.enabled = true;
			Cursor.visible = false;
			Cursor.lockState = CursorLockMode.Locked;
			Time.timeScale = 1;
		}
	}

	private void Die() { rPause = null; Destroy(gameObject); }
	private void OnApplicationQuit() { rPause = null; }
}
