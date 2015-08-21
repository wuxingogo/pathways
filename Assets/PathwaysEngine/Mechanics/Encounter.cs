/* Ben Scott * bescott@andrew.cmu.edu * 2015-07-23 * Encounter */

using UnityEngine;
using System.Collections;
using util=PathwaysEngine.Utilities;

namespace PathwaysEngine.Mechanics {
	public partial class Encounter : MonoBehaviour {
		public enum Inputs : byte { Trigger, Click, Elapsed, Sight };
		Inputs input;
		public enum Outputs : byte { Message, Alert };
		Outputs output;
		public string uuid;
		bool reuse;
		float time;
		Collider cl;

		public string desc {
			get { return string.Format("{1}{0}",_desc,
				"This is the default modifier for encounters."); }
			set { _desc = value; }
		} string _desc = "This is a special Encounter.";

		void Awake() {
			cl = GetComponent<Collider>();
			if (cl) cl.isTrigger = true;
			this.GetYAML();
		}

		void Start() { if (input==Inputs.Elapsed) TimedEncounter(time); }

		void OnTriggerEnter(Collider other) {
			if (input==Inputs.Trigger && other.gameObject.tag=="Player")
				StartCoroutine(BeginEncounter());
		}

		IEnumerator BeginEncounter() {
			this.Log();
			if (output==Outputs.Alert) Terminal.WaitForInput();
			if (reuse) {
				if (cl) cl.enabled = false;
				yield return new WaitForSeconds(1f);
				if (cl) cl.enabled = true;
			} else gameObject.SetActive(false);
		}

		IEnumerator TimedEncounter(float t) {
			Debug.Log(time);
			yield return new WaitForSeconds(t);
			StartCoroutine(BeginEncounter());
		}
	}
}

