/* Ben Scott * bescott@andrew.cmu.edu * 2015-07-09 * Item */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PathwaysEngine.Inventory {
	public class Item : MonoBehaviour, IItem {
		public AudioClip sound;
		internal Backpack playerPack;
		internal Player player;
		public Texture2D icon;

		public bool seen {
			get { return _seen; }
			set { _seen = value; }
		} bool _seen = false;

		public bool held {
			get { return _held; }
			set { _held = value; }
		} bool _held = false;

		public string uuid {
			get { return _uuid; }
			private set { _uuid = value; }
		} string _uuid;

		new public string @name {
			get { return _name; }
			set { _name = value; }
		} string _name = "Item";

		public string desc {
			get { return _desc; }
			set { _desc = value; }
		} string _desc = "This is an Item.";

		public void Awake() { player = Pathways.player; }

		public virtual void Take() {
			if (!playerPack) return;
			GetComponent<AudioSource>().PlayOneShot(sound);
			transform.parent = playerPack.transform;
			transform.localPosition = Vector3.down;
			transform.localRotation = Quaternion.identity;
#if OLD
			foreach (Transform Child in transform)
				Child.gameObject.SetActive(false);
			cd.enabled = false;
			rb.isKinematic = true;
			rb.Sleep();
			au.enabled = false;
			held = false;
#endif
//			playerPack.invItems = ArrayF.Push(playerPack.invItems, cItem);
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
			GetComponent<Rigidbody>().AddForce(Quaternion.identity.eulerAngles*4,ForceMode.VelocityChange);
			transform.parent = null;
		}

		public virtual void Find() { }
		public virtual void View() { print (desc); }

		//return (this==(Item)obj); // scary != hash
		public override bool Equals(System.Object obj) { return (base.Equals(obj)); }
		public override int GetHashCode() { return (base.GetHashCode()); }

		public static bool operator !(Item i) { return (i==null); }
		public static bool operator ==(Item a, Item b) {
			return (!(a.GetType()!=b.GetType() || a.uuid!=b.uuid)); }
		public static bool operator !=(Item a, Item b) { return (!(a==b)); }
//		public static void operator >>(Item i, Bag c) { c.Insert(i); }
//		public static void operator <<(Item i, Bag c) { c.Remove(i); }
	}
}
