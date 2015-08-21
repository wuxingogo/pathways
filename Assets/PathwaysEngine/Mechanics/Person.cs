/* Ben Scott * bescott@andrew.cmu.edu * 2015-08-04 * Person */

using UnityEngine;
using System.Collections;
using type=System.Type;
using invt=Inventory;
using mvmt=Movement;
using util=PathwaysEngine.Utilities;
using maps=PathwaysEngine.Mechanics.Setting;

namespace PathwaysEngine.Mechanics {
	public class Person : MonoBehaviour, ILiving {
		Body body;
		public proper_name uuid = new proper_name("Adaline","Braun");
		public StatSet stats { get; set; }
		public proper_name fullName;
		internal mvmt::ThirdPersonController tpc;

		public bool dead {
			get { return tpc.dead; }
			set { _dead = value; if (_dead) Kill(); }
		} bool _dead = false;

		public invt::IItemSet holdall {
			get { if (_holdall==null)
				_holdall = new Player.Holdall();
				return _holdall; }
			set { foreach (var item in _holdall)
				value.Add(item); _holdall = value; }
		} invt::IItemSet _holdall;

		public virtual void Awake() { // public Person() { body = new Body(); }
			var temp = new util::Anchor[(int) Corpus.All];
			foreach (var elem in GetComponentsInChildren<util::Anchor>())
				temp[(int) elem.bodyPart] = elem;
			body = new Person.Body(temp);
		}

		public void ApplyDamage(float damage) {
			if (stats!=null) return; // set dead to true
		}

		public virtual bool Take(invt::Item item) {
			item.transform.parent = transform;
			holdall.Add(item); // return holdall.Add(item);
			item.Take(); return true; }
		public virtual bool Drop(invt::Item item) {
			if (!holdall.Contains(item)) return false;
			item.transform.parent = null;
			holdall.Remove(item);
			item.Drop(); return true; }
		public virtual void Equip(invt::IEquippable item) { //Debug.Log(item);
			body[item.GetType()] = item; }
		public virtual void Stow(invt::IEquippable item) {
			body[item.GetType()] = item; item.Stow(); }

		public void Travel(maps::Area tgt) {
			StartCoroutine(Travel(tgt.level)); }
		IEnumerator Travel(int n) {
			util::CameraFade.StartAlphaFade(Color.black,false,1f);
			yield return new WaitForSeconds(1.1f);
			Application.LoadLevel(n);
		}

		public void Kill() { }

		public void AddCondition(Condition cond) { }
		public void AddCondition(Condition cond,Severity svrt) { }

		class Body {
			invt::IEquippable[] list;
			util::Anchor[] anchors;

			public Body(util::Anchor[] anchors) {
				this.list = new invt::IEquippable[(int) Corpus.All];
				this.anchors = anchors;
			}

			public invt::IEquippable this[Corpus n] {
				get { return list[(int) n]; }
				set { if (list[(int) n]!=null) list[(int) n].Stow();
					((invt::Item) value).transform.parent =
						anchors[(int) n].transform;//Debug.Log("faaaakk");
					((invt::Item) value).transform.localPosition = Vector3.zero;
					((invt::Item) value).transform.localRotation = Quaternion.identity;
					list[(int) n] = value; }}

			public invt::IEquippable this[type T] {
				get { return list[Type_Index(T)]; }
				set { if (list[Type_Index(T)]!=null) list[Type_Index(T)].Stow();
					((invt::Item) value).transform.parent =
						anchors[(int) Type_Index(T)].transform;
					((invt::Item) value).transform.localPosition = Vector3.zero;
					((invt::Item) value).transform.localRotation = Quaternion.identity;
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
				if (T==typeof(invt::Lamp)
				|| T.IsSubclassOf(typeof (invt::Lamp)))
					return (int) Corpus.HandL;
				if (T==typeof(invt::Flashlight))
					return (int) Corpus.Other;
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


