/* Ben Scott * bescott@andrew.cmu.edu * 2015-07-08 * Flashlight */

using UnityEngine;
using System.Collections;
using anim=PathwaysEngine.StateMachine;
using term=PathwaysEngine.UserInterface;

namespace PathwaysEngine.Inventory {
	public class Lamp : Item, IEquippable {
		bool wait = false;
		public AudioClip auSwitch;
		Animator animator;
		anim::IKControl handIK;
		public term::Controls.InputKey OnDash, OnLamp;

		public Lamp() {
			OnDash = new term::Controls.InputKey((n)=>sprint=n);
			OnLamp = new term::Controls.InputKey((n)=>{
				if (!wait && n && on) on = !on;
				else if (!wait && n) worn = true;});
		}

		bool sprint {
			get { return _sprint; }
			set { _sprint = value;
				if (gameObject.activeSelf)
					LateSetBool("sprint",_sprint); }
		} bool _sprint = false;

		public bool on {
			get { return _on; }
			set { _on = value;
				StartCoroutine(LateSetBool("on",_on));
				StartCoroutine(On()); }
		} bool _on;

		public bool worn {
			get { return _worn; }
			set { _worn = value;
				handIK.ikActive = _worn;
				if (_worn) Equip();
				else Stow(); }
		} bool _worn = true;

		public bool used { get; set; }
		public uint uses { get; set; }
		public float time { get; set; }

		new void Awake() {
			base.Awake();
			animator = GetComponent<Animator>();
			foreach (Light elem in GetComponentsInChildren<Light>())
				elem.enabled = true; //GetComponent<Light>()
			foreach (var elem in
			(Object.FindObjectsOfType<anim::IKControl>() as anim::IKControl[]))
				if (elem.hand==anim::IKControl.Hands.Left) handIK = elem;
			GetComponent<AudioSource>().clip = auSwitch;
			worn = true;
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
			StartCoroutine(LateSetBool("worn",_worn));
			on = true;
		}

		public void Stow() {
			StartCoroutine(LateSetBool("worn",_worn));
			gameObject.SetActive(false);
		}
	}
}

