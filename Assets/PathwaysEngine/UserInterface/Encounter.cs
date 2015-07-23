/* Ben Scott * bescott@andrew.cmu.edu * 2015-07-23 * Encounter */

using UnityEngine;
using System.Collections;
using util=PathwaysEngine.Utilities;

namespace PathwaysEngine.UserInterface {
	public class Encounter : MonoBehaviour {
		public enum Inputs : byte { Trigger, Click, Elapsed, Sight };
		public Inputs input = Inputs.Trigger;
		public enum Outputs : byte { Message, Alert };
		public Outputs output = Outputs.Message;
		public bool reuse = false;
		public float t = 120f;
		Collider cl;

		public string uuid { get; set; }

		void Awake() {
			cl = GetComponent<Collider>();
			cl.isTrigger = true;
		}

		void Start() { if (input==Inputs.Elapsed) TimedEncounter(); }

		void OnTriggerEnter(Collider other) {
			if (input==Inputs.Trigger && other.gameObject.tag=="Player")
				StartCoroutine(BeginEncounter());
		}

		IEnumerator BeginEncounter() {
			Terminal.Log(uuid);
			if (reuse) {
				cl.enabled = false;
				yield return new WaitForSeconds(1f);
				cl.enabled = true;
			} else gameObject.SetActive(false);
		}

		IEnumerator TimedEncounter() {
			yield return new WaitForSeconds(t);
			StartCoroutine(BeginEncounter());
		}
	}
}

