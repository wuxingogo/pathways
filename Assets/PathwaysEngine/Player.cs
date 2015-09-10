/* Ben Scott * bescott@andrew.cmu.edu * 2015-07-12 * Player */

using UnityEngine; // The main player class, inherits from person
using type=System.Type; // the holdall auto-assigns / creates a
using System.Collections; // singleton-ish thing when the player
using System.Collections.Generic; // has no backpack, to simulate
using System.Text.RegularExpressions; // coatpockets and hands
using PathwaysEngine.Adventure; // and whatnot. Interfaces with
using invt=PathwaysEngine.Inventory; // the movement controller
using maps=PathwaysEngine.Adventure.Setting; // and also deals
using mvmt=PathwaysEngine.Movement; // with damage, death, and
using stat=PathwaysEngine.Statistics; // operations on Items.
using util=PathwaysEngine.Utilities;

namespace PathwaysEngine {
	public class Player : Person {
		public bool wait = false;
		static public List<invt::Item> wornItems;
		static public new string uuid = "Amelia Earhart";
		static public mvmt::Hand right, left;
		static public new stat::Set stats;
		static public util::key menu, term, lamp;
		static public new invt::IItemSet holdall {
			get { if (_holdall==null)
					_holdall = new Player.Holdall();
				return _holdall; }
			set { _holdall = value; }
		} static invt::IItemSet _holdall;
		static public new maps::Room room;
		static public new maps::Area area;
		static public Animator animator;

		static public new bool dead {
			get { return Pathways.player.tpc.dead; }
			set { _dead_static = value;
				if (_dead_static) Pathways.Log("dead");
			}
		} static bool _dead_static = false;

		public uint massLimit { get; set; }

		static public new List<invt::Item> nearbyItems {
			get { return ((Person) Pathways.player).nearbyItems; }}

		static Player() { }

		public Player() {
			menu = new util::key((n)=>menu.input=n);
			term = new util::key((n)=>term.input=n);
			lamp = new util::key((n)=>{lamp.input=n;
				if (n && left.heldItem!=null && left.heldItem.held)
					left.heldItem.Use(); });
		}

		public override void Awake() { base.Awake();
			Pathways.player = this;
			tpc = GetComponentInChildren<mvmt::ThirdPersonController>();
			animator = tpc.GetComponent<Animator>();
		}

		public void Start() { Pathways.gameState = GameStates.Game; }

		//void Update() { print("state: "+Pathways.gameState+"\nlast: "+Pathways.lastState); }

		public void ResetPlayerLocalPosition() {
			tpc.transform.localPosition = Vector3.zero; }

		IEnumerator DelayToggle(float t) {
			wait = true;
			yield return new WaitForSeconds(t);
			wait = false;
		}

		public static void Drop(command cmd) {
			var temp = new List<invt::Item>();
			if ((new Regex(@"\ball\b")).IsMatch(cmd.input)) Player.Drop();
			else foreach (var item in holdall)
				if (item.desc.IsSynonymousWith(cmd.input)) temp.Add(item);
			if (temp.Count==1) Player.Drop(temp[0]);
			else if (temp.Count!=0) Terminal.Resolve(cmd,temp);
		}

		public static void Take(command cmd) {
			if (nearbyItems.Count==0) return;
			var temp = new List<invt::Item>();
			if ((new Regex(@"\b(all)\b")).IsMatch(cmd.input)) Player.Take();
			else foreach (var item in nearbyItems)
				if (item.desc.IsSynonymousWith(cmd.input)) temp.Add(item);
			if (temp.Count==1) Player.Take(temp[0]);
			else if (temp.Count!=0) Terminal.Resolve(cmd,temp);
		}

		public static void Wear(command cmd) {
			if (holdall.Count==0) {
				Terminal.Log("You have nothing to wear!",
					Formats.Command); return; }
			var temp = new List<invt::IWearable>();
			if ((new Regex(@"\b(all)\b")).IsMatch(cmd.input))
				Player.Wear();
			else foreach (var item in holdall)
				if (item is invt::IWearable
				&& item.desc.IsSynonymousWith(cmd.input))
					temp.Add((invt::IWearable) item);
			if (temp.Count==1) Player.Wear(temp[0]);
			else if (temp.Count!=0) Terminal.Resolve(cmd,temp);
		}

		public static void Stow(command cmd) {
			if (holdall.Count==0) {
				Terminal.Log("You have nothing to stow!",
					Formats.Command); return; }
			var temp = new List<invt::IWearable>();
			if ((new Regex(@"\b(all)\b")).IsMatch(cmd.input)) Player.Stow();
			else foreach (var item in holdall)
				if (item is invt::IWearable
				&& item.desc.IsSynonymousWith(cmd.input))
					temp.Add((invt::IWearable) item);
			if (temp.Count==1) Player.Stow(temp[0]);
			else if (temp.Count!=0) Terminal.Resolve(cmd,temp);
			else Terminal.Log("You don't have anything you can stow.",
				Formats.Command);
		}

		public static void View(command cmd) {
			Terminal.Log(" > "+cmd.input+": You examine the thing.",
				Formats.Command);
		}

		public static void Read(command cmd) { }
		public static void Read(invt::IReadable item) { item.Read(); }

		public static new void Drop() {
			((Person) Pathways.player).Drop(); }
		public static new void Drop(invt::Item item) {
			((Person) Pathways.player).Drop(item); }
		public static new void Take() {
			((Person) Pathways.player).Take(nearbyItems); }
		public static new void Take(invt::Item item) {
			((Person) Pathways.player).Take(item); }
		public static new void Wear() {
			((Person) Pathways.player).Wear(); }
		public static new void Wear(invt::IWearable item) {
			((Person) Pathways.player).Wear(item); }
		public static new void Stow() {
			((Person) Pathways.player).Stow(); }
		public static new void Stow(invt::IWearable item) {
			((Person) Pathways.player).Stow(item); }
		public static new void Goto(maps::Area tgt) {
			((Person) Pathways.player).Goto(tgt); }

		public class Holdall : invt::ItemSet {
			public new List<invt::Item> items;
			public uint lim { get { return 4; } }

			public new void Add(invt::Item item) {
				if (item.GetType().IsSubclassOf(typeof(invt::Backpack)))
					Player.holdall = (invt::IItemSet) item;
				else if (items.Count>=lim)
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
