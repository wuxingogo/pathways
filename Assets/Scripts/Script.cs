/*_( [Ben Scott - bescott@andrew.cmu.edu] | [2014.0601 - TUES 1330] | [CS15.0112 - SS14.TR03] | {[Player Inventory: Main]} )_*/

//#define DEBUG

namespace PathwaysEngine {
	using UnityEngine;
	using S = UnityEngine.SerializeField;						using N = System.NonSerializedAttribute;

	internal class Script : MonoBehaviour {						// Deriving one class up from MonoBehaviour to add all the base
		[N] internal GameObject pl;								// and boilerplate code I typically use in the Engine. Unity is
		[N] internal Transform tr;								// likely not storing references to all the internal components
		[N] internal Collider cl;								// I typically use, so I will just store a reference to them at
		[N] internal Rigidbody rb;								// the outset, though all this might be worse for cached memory
		[N] internal Animation am;
		[N] internal AudioSource au;
		[N] internal Camera cm;
		[N] internal Renderer rn;

		internal void Awake() {
			tr = gameObject.GetComponent<Transform>();
			cl = gameObject.GetComponent<Collider>();
			rb = gameObject.GetComponent<Rigidbody>();
			am = gameObject.GetComponent<Animation>();
			au = gameObject.GetComponent<AudioSource>();
			cm = gameObject.GetComponent<Camera>();
			rn = gameObject.GetComponent<Renderer>();
			pl = GameObject.FindGameObjectWithTag("Player");
		}
	} //*/
}