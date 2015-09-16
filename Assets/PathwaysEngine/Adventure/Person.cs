/* Ben Scott * bescott@andrew.cmu.edu * 2015-08-04 * Person */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using type=System.Type;
using invt=PathwaysEngine.Inventory;
using mvmt=PathwaysEngine.Movement;
using util=PathwaysEngine.Utilities;
using maps=PathwaysEngine.Adventure.Setting;
using stat=PathwaysEngine.Statistics;

namespace PathwaysEngine.Adventure {
	public class Person : Creature {
		Body body;
		public override stat::Set stats { get; set; }
		public proper_name fullName;
		public maps::Area area;
		public maps::Room room;
		public Vector3 position {get { return tpc.transform.position; }}
		internal mvmt::ThirdPersonController tpc;

		public override bool dead {
			get { return tpc.dead; }
			set { _dead = value; if (_dead) Kill(); } }

		public List<invt::Item> nearbyItems {
			get { //if (_nearbyItems==null)
				_nearbyItems = GetNearbyItems();
				return _nearbyItems; }
		} List<invt::Item> _nearbyItems;

		public invt::ItemSet holdall {
			get { if (_holdall==null)
				_holdall = new invt::ItemSet();
				return _holdall; }
			set { foreach (var item in _holdall)
				value.Add(item); _holdall = value; }
		} invt::ItemSet _holdall;

		public override void Awake() {
			var temp = new util::Anchor[(int) Corpus.All];
			foreach (var elem in GetComponentsInChildren<util::Anchor>())
				temp[(int) elem.bodyPart] = elem;
			body = new Person.Body(temp);
		}

		public override void ApplyDamage(float damage) {
			if (stats!=null) return; // set dead to true
		}

		public virtual void Take() { Take(nearbyItems); }

		public virtual void Take(List<invt::Item> list) {
			foreach (var item in list) Take(item); }

		public virtual void Take(invt::Item item) {
			if (holdall.Contains(item))
				throw new System.Exception("Person already has item");
			item.transform.parent = transform;
			holdall.Add(item);
			item.Take();
		}

		public virtual void Drop() {
			foreach (var item in holdall) Drop(item); }

		public virtual void Drop(invt::Item item) {
			if (!holdall.Contains(item)) return;
			item.transform.parent = null;
			holdall.Remove(item);
			item.Drop();
		}

		public virtual void Wear() {
			foreach (var item in holdall)
				if (item is invt::IWearable)
					Wear((invt::IWearable) item); }

		public virtual void Stow() {
			foreach (var item in holdall)
				if (item is invt::IWearable)
					Stow((invt::IWearable) item); }

		public virtual void Wear(invt::IWearable item) {
			body[item.GetType()] = item; item.Wear(); }
		public virtual void Stow(invt::IWearable item) { item.Stow(); }

		public void Goto(maps::Area tgt) {
			StartCoroutine(Goto(tgt.level)); }
		IEnumerator Goto(int n) {
			util::CameraFade.StartAlphaFade(Color.black,false,1f);
			yield return new WaitForSeconds(1.1f);
			Application.LoadLevel(n);
		}

		public virtual List<invt::Item> GetNearbyItems() {
			var temp = Physics.OverlapSphere(
				tpc.transform.position,
				4f,LayerMask.NameToLayer("Items"));
			List<invt::Item> list = new List<invt::Item>();
			foreach (var elem in temp) {
				if (elem.attachedRigidbody==null) continue;
				var item = elem.attachedRigidbody.GetComponent<invt::Item>();
				if (item && !list.Contains(item) && !item.held) list.Add(item);
			} return list;
		}

		public override void Kill() { Terminal.Log("oh no!"); base.Kill(); }

		public void AddCondition(stat::Condition cond) { }
		public void AddCondition(stat::Condition cond,stat::Severity svrt) { }

		class Body {
			invt::IWearable[] list;
			util::Anchor[] anchors;

			public Body(util::Anchor[] anchors) {
				this.list = new invt::IWearable[(int) Corpus.All];
				this.anchors = anchors;
			}

			public invt::IWearable this[Corpus n] {
				get { return list[(int) n]; }
				set { var temp = (invt::IWearable) list[(int) n];
					if (temp!=null && temp!=value) Player.Stow(temp);
					var item = (invt::Item) value;
					item.transform.parent = anchors[(int) n].transform;
					item.transform.localPosition = Vector3.zero;
					item.transform.localRotation = Quaternion.identity;
					list[(int) n] = value; }}

			public invt::IWearable this[type T] {
				get { return list[Type_Index(T)]; }
				set { var temp = list[Type_Index(T)];
					if (temp!=null && temp!=value) Player.Stow(temp);
					var item = (invt::Item) value;
					item.transform.parent =
						anchors[(int) Type_Index(T)].transform;
					item.transform.localPosition = Vector3.zero;
					item.transform.localRotation = Quaternion.identity;
					list[Type_Index(T)] = value; } }

			static public int Type_Index(type T) {
				if (T==typeof(invt::Helmet)
				|| T.IsSubclassOf(typeof(invt::Helmet)))
					return (int) Corpus.Head;
				if (T==typeof(invt::Necklace)
				|| T.IsSubclassOf(typeof (invt::Necklace)))
					return (int) Corpus.Neck;
				if (T==typeof(invt::Armor)
				|| T.IsSubclassOf(typeof (invt::Armor)))
					return (int) Corpus.Chest;
				if (T==typeof(invt::Cloak)
				|| T.IsSubclassOf(typeof (invt::Cloak)))
					return (int) Corpus.Back;
				if (T==typeof(invt::Backpack)
				|| T.IsSubclassOf(typeof (invt::Backpack)))
					return (int) Corpus.Back;
				if (T==typeof(invt::Belt)
				|| T.IsSubclassOf(typeof (invt::Belt)))
					return (int) Corpus.Waist;
				if (T==typeof(invt::Clothes)
				|| T.IsSubclassOf(typeof (invt::Clothes)))
					return (int) Corpus.Frock;
				if (T==typeof(invt::Bracers)
				|| T.IsSubclassOf(typeof (invt::Bracers)))
					return (int) Corpus.Arms;
				if (T==typeof(invt::Pants)
				|| T.IsSubclassOf(typeof (invt::Pants)))
					return (int) Corpus.Legs;
				if (T==typeof(invt::Gloves)
				|| T.IsSubclassOf(typeof (invt::Gloves)))
					return (int) Corpus.Hands;
				if (T==typeof(invt::Shoes)
				|| T.IsSubclassOf(typeof (invt::Shoes)))
					return (int) Corpus.Feet;
				if (T==typeof(invt::Flashlight))
					return (int) Corpus.Other;
				if (T==typeof(invt::Lamp)
				|| T.IsSubclassOf(typeof (invt::Lamp)))
					return (int) Corpus.HandL;
				if (T==typeof(invt::Weapon)
				|| T.IsSubclassOf(typeof (invt::Weapon)))
					return (int) Corpus.HandR;
				return (int) Corpus.All;
			}
		}
	}
}

#if TODO
	public class Vitality : MonoBehaviour, ILiving {
		public enum DamageStates : byte { Heal, Hurt, Crit, Dead};
		public int weightCurrent, weightCritical;
		public float healthCurrent, healthCritical, RUL, TTF;
		public float bodyMass, bodyTemp, bodyWater;
		public double staminaCurrent, staminaCritical;
		public Faculties faculties { get; set; }
		public Condition condition { get; set; }
		public Diagnosis diagnosis { get; set; }
		public Prognosis prognosis { get; set; }
		public Severity[] severityFaculties { get; set; }
		public Severity[] severityCondition { get; set; }
		public Severity[] severityDiagnosis { get; set; }
		public Severity[] severityPrognosis { get; set; }
		public AudioSource au;
		public AudioClip[] soundsHurt, soundsHeal;

		public Vitality() {
			bodyMass 			= 80;
			weightCurrent 		= 0;					weightCritical		= 40;
			healthCurrent		= 128.0f;				healthCritical		= 128.0f;
			staminaCurrent		= 64.0;					staminaCritical		= 64.0;
			condition 			= Condition.Healthy;	faculties 			= Faculties.All;
			diagnosis 			= Diagnosis.None;		prognosis 			= Prognosis.None;
			severityFaculties 	= new Severity[8];		severityCondition 	= new Severity[8];
			severityDiagnosis	= new Severity[20];		severityPrognosis 	= new Severity[8];
		}

		public void Awake() { au = (GetComponent<AudioSource>()) ?? (gameObject.AddComponent<AudioSource>()); }

		public void ApplyDamage(float damage) {
			if (damage==0) return;
			DamageStates state = (damage>0) ? (DamageStates.Hurt):(DamageStates.Heal);
			byte ind = 0;
			healthCurrent -= damage;
			state = (healthCurrent>0) ? (state):(DamageStates.Dead);
			switch (state) {
				case DamageStates.Heal :
					ind = (byte) Random.Range(0,soundsHeal.Length); break;
				case DamageStates.Hurt :
					ind = (byte) Random.Range(0,soundsHurt.Length>>1); break;
				case DamageStates.Crit :
					ind = (byte) Random.Range(soundsHurt.Length>>1,soundsHurt.Length); break;
				case DamageStates.Dead :
					ind = (byte) soundsHurt.Length; break;
			} au.PlayOneShot(soundsHurt[(int)ind], (Mathf.Abs((int) damage)>>2)/au.volume);
		}

		public void AddCondition(Condition cond) { severityFaculties[((byte) cond)/4]++; }

		public void AddCondition(Condition cond, Severity srvt) { severityFaculties[((byte) cond)/4]+=(byte) srvt; }
	}
}

#endif


