/* Ben Scott * bescott@andrew.cmu.edu * 2015-07-12 * Player */

using UnityEngine; // The main player class, inherits from person
using type=System.Type; // the holdall auto-assigns / creates a
using System.Collections; // singleton-ish thing when the player
using System.Collections.Generic; // has no backpack, to simulate
using invt=Inventory; // coatpockets and hands and whatnot.
using mech=PathwaysEngine.Mechanics; // Interfaces with it's
using maps=PathwaysEngine.Mechanics.Setting; // movement controller
using mvmt=Movement; // and detects and deals with damage and death
using util=PathwaysEngine.Utilities;
using System.Text.RegularExpressions;

namespace PathwaysEngine {
	public class Player : mech::Person {
		static public List<invt::Item> wornItems;
		static public readonly Regex tags;
		static public mvmt::Hand right, left;
		static public new mech::StatSet stats;
		static public util::key menu, term;
		static public Animator animator;

		static public new bool dead {
			get { return Pathways.player.tpc.dead; }
			set { _dead = value;
				if (_dead) {
					Terminal.Log(Pathways.GetYAML("dead"),Formats.Alert);
					Terminal.focus = true;
				}
			}
		} static bool _dead = false;

		public uint massLimit { get; set; }

		static public List<invt::Item> nearbyItems {
			get { if (_nearbyItems==null)
				_nearbyItems = GetNearbyItems();
				return _nearbyItems; }
		} static List<invt::Item> _nearbyItems;

		static Player() { tags = new Regex(@"Player|PlayerBones"); }

		public Player() {
			menu = new util::key((n)=>menu.input=n);
			term = new util::key((n)=>term.input=n);
		}

		public override void Awake() {
			Pathways.player = this;
			base.Awake();
			tpc = GetComponentInChildren<mvmt::ThirdPersonController>();
			animator = tpc.GetComponent<Animator>();
		}

		public void ResetPlayerLocalPosition() {
			tpc.transform.localPosition = Vector3.zero; }

		public static new bool Take(invt::Item item) {
			return ((mech::Person) Pathways.player).Take(item); }
		public static new bool Drop(invt::Item item) {
			return ((mech::Person) Pathways.player).Drop(item); }
		public static new void Equip(invt::IEquippable item) {
			((mech::Person) Pathways.player).Equip(item); }
		public static new void Stow(invt::IEquippable item) {
			((mech::Person) Pathways.player).Stow(item); }

		public static new void Travel(maps::Area tgt) {
			((mech::Person) Pathways.player).Travel(tgt); }

		static List<invt::Item> GetNearbyItems() {
			var temp = Physics.OverlapSphere(
				Pathways.player.transform.position,
				4f,LayerMask.NameToLayer("Items"));
			List<invt::Item> list = new List<invt::Item>();
			foreach (var elem in temp) {
				if (elem.attachedRigidbody==null) continue;
				var item = elem.attachedRigidbody.GetComponent<invt::Item>();
				if (item) list.Add(item);
			} return list;
		}

		public class Holdall : invt::ItemSet {
			public new List<invt::Item> items;
			public uint countLimit { get { return 4; } }

			public new void Add(invt::Item item) {
				if (item.GetType().IsSubclassOf(typeof(invt::Backpack)))
					Pathways.player.holdall = (invt::IItemSet) item;
				else if (items.Count>=countLimit)
					Terminal.Log("Your hands are full.",Formats.Command);
				else base.Add(item);
			}

			public invt::Item GetItem<T>() where T : invt::Item {
				if (items.Count>1) return items[0];
				return default(invt::Item); }
			public invt::Item GetItem<T>(string s) where T : invt::Item {
				foreach (var elem in GetItems<T>())
					if (elem.name==s) return elem;
				return default(invt::Item); }
			public List<invt::Item> GetItems<T>() where T : invt::Item {
				return new List<invt::Item>(items); }
		}
	}
}
