/* Ben Scott * bescott@andrew.cmu.edu * 2015-07-31 * Item */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PathwaysEngine.Inventory {
	[RequireComponent(typeof(Rigidbody))]
	[RequireComponent(typeof(AudioSource))]
	public partial class Item : MonoBehaviour, IGainful {
		public AudioClip sound;
		public Texture2D icon;

		public bool seen {
			get { return _seen; }
			set { _seen = value; }
		} bool _seen = false;

		public bool held {
			get { return _held; }
			set { _held = value; }
		} bool _held = false;

		public int cost { get; set; }

		public string uuid {
			get { return _uuid; }
			private set { _uuid = value; }
		} string _uuid;

		public float mass {
			get { return GetComponent<Rigidbody>().mass; }
			set { GetComponent<Rigidbody>().mass = value; }}

		public string desc {
			get { return string.Format(
				"{0} It weighs {1}kg, and is worth {2}z.",_desc,mass,cost); }
			set { _desc = value; }
		} string _desc = "This is an Item.";

		public virtual void Awake() {
			GetComponent<Collider>().isTrigger = true;
			GetComponent<Collider>().enabled = true;
			GetComponent<Rigidbody>().isKinematic = false;
			GetComponent<Rigidbody>().useGravity = true;
		}

		public virtual void Take() {
			if (gameObject.activeInHierarchy)
				GetComponent<AudioSource>().PlayOneShot(sound);
			transform.localPosition = Vector3.zero;
			transform.localRotation = Quaternion.identity;
			gameObject.SetActive(false);
			GetComponent<Collider>().enabled = false;
			GetComponent<Rigidbody>().isKinematic = true;
			GetComponent<AudioSource>().enabled = false;
			held = true;
		}

		public virtual void Drop() {
#if FAIL
			if (!held) {
				transform.position = player.transform.position;
//				transform.localPosition = Vector3.down;
				transform.rotation = Quaternion.identity;
			}
#endif
			GetComponent<Animator>().enabled = false;
			GetComponent<SphereCollider>().enabled = true;
			GetComponent<Rigidbody>().isKinematic = false;
			GetComponent<AudioSource>().enabled = true;
			held = false;
			GetComponent<Rigidbody>().AddForce(
				Quaternion.identity.eulerAngles*4,
				ForceMode.VelocityChange);
			transform.parent = null;
		}

		public virtual void Find() { }
		public virtual void View() { Terminal.Log(desc); }
		public virtual void Buy() { }
		public virtual void Sell() { }

		//return (this==(Item)obj); // scary != hash
		public override bool Equals(System.Object obj) { return (base.Equals(obj)); }
		public override int GetHashCode() { return (base.GetHashCode()); }

		public static bool operator !(Item i) { return (i==null); }
		public static bool operator ==(Item a, Item b) {
			return (!(a.GetType()!=b.GetType() || a.uuid!=b.uuid)); }
		public static bool operator !=(Item a, Item b) { return (!(a==b)); }
	}
}
