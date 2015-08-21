/* Ben Scott * bescott@andrew.cmu.edu * 2015-08-13 * Lamp */

using UnityEngine;
using System.Collections;
using mvmt=PathwaysEngine.Movement;
using util=PathwaysEngine.Utilities;

namespace PathwaysEngine.Inventory {
	public class Lamp : Item, IEquippable {
		bool wait = false;
		public AudioClip auSwitch;
		Animator animator;
		mvmt::Hand hand;
		public util::key dash, lamp;

		bool sprint {
			get { return _sprint; }
			set { _sprint = value;
				if (gameObject.activeSelf)
					LateSetBool("sprint",_sprint); }
		} bool _sprint = false;

		public bool on {
			get { return _on; }
			set { if (!held) return;
				_on = value;
				StartCoroutine(LateSetBool("on",_on));
				StartCoroutine(On()); }
		} bool _on;

		public bool worn {
			get { return _worn; }
			set { _worn = value;
				if (hand) hand.ikActive = _worn;
				if (_worn) Equip();
				else Stow(); }
		} bool _worn = false;

		public bool used { get; set; }
		public uint uses { get; set; }
		public float time { get; set; }

		public Lamp() {
			dash = new util::key((n)=>sprint=n);
			lamp = new util::key((n)=>{
				if (!wait && n && on) on = !on;
				else if (!wait && n) worn = true;});
		}

		public override void Awake() {
			base.Awake();
			animator = GetComponent<Animator>();
			foreach (Light elem in GetComponentsInChildren<Light>())
				elem.enabled = false;
			GetComponent<AudioSource>().clip = auSwitch;
		}

		public void Start() {
			hand = Player.left;
			if (!hand) Debug.Log("astasrdt");
		}

		public void Use() { if (worn) StartCoroutine(On()); }

		IEnumerator On() {
			wait = true;
			yield return new WaitForSeconds(0.125f);
//			while (on && time>0) time -= Time.deltaTime;
			foreach (var elem in GetComponentsInChildren<Light>())
				elem.GetComponent<Light>().enabled = on;
			yield return new WaitForSeconds(0.25f);
			wait = false;
			if (!on) worn = false;
		}

		IEnumerator LateSetBool(string s, bool t) {
			yield return new WaitForEndOfFrame();
			if (animator.enabled) animator.SetBool(s,t);
		}

		public void Equip() {
			if (gameObject) gameObject.SetActive(true);
			Player.Equip(this);
			gameObject.SetActive(true);
			StartCoroutine(LateSetBool("worn",_worn));
			on = true;
		}

		public void Stow() {
			StartCoroutine(LateSetBool("worn",_worn));
			gameObject.SetActive(false);
		}
	}
}

