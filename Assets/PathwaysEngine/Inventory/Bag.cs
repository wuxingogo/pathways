/* Ben Scott * bescott@andrew.cmu.edu * 2015-07-28 * Bag */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using type=System.Type;
using util=PathwaysEngine.Utilities;

namespace PathwaysEngine.Inventory {
	public class Bag : ItemCollection {
		internal bool hasScreenControl, isIgnoringInput;
		internal int radiusGet, layerItem, invItemValues;
		internal float reUpdateTimer, reuptakeTimer;
		LayerMask layerMask;
		Flashlight cLight;
		internal List<Item> nearItems;
		internal Camera mCAMR;
		util::key invt, menu;

		public Bag() {
			hasScreenControl = false; isIgnoringInput = false;
			radiusGet 		 = 16;	  layerItem 	  = 16;
			reUpdateTimer 	 = 0f;	  reuptakeTimer   = 3f;
			invt = new util::key((n)=>invt.input=n);
			menu = new util::key((n)=>menu.input=n);
		}

		public override void Awake() {
			base.Awake(); //Pause.PausePlayer(false);
			layerMask = LayerMask.NameToLayer("Items");
		}

		void Update() {
			reuptakeTimer += Time.deltaTime;
			if (!isIgnoringInput) {
				if (!hasScreenControl && invt.input || menu.input) {
					hasScreenControl = true; Pause.PausePlayer(true); }
				else if (hasScreenControl && invt.input || menu.input) {
					hasScreenControl = false; Pause.PausePlayer(false); }
			} // StartCoroutine(InventorySlow());
			//if (mCAMR) mCAMR.pixelRect = new Rect(Screen.width-320, 64, 256, 256);
			//else mCAMR = GameObject.FindGameObjectWithTag("Map Camera").GetComponent<Camera>();
		}

		internal Item[] GetNearbyItems() {
			Collider[] temp = Physics.OverlapSphere(
				transform.position,8f,layerMask);
			List<Item> items = new List<Item>();
			foreach (Collider entity in temp)
				if (entity.gameObject.GetComponent<Item>())
					items.Add(entity.gameObject.GetComponent<Item>());
			return items.ToArray();//typeof(Item) as Item[];
		}

#if IMPL
		internal void ItemButton(IUsable item, int m) {
			item.Drop();
			if (m==0) item.Take();
			else if (m==1) item.Use();
			else item.Drop();
		}

		internal void ItemButton(Item item, int m) {
			item.Drop();
			if (m==0) item.Take();
//			else if (m==1) item.Use();
			else item.Drop();
		}
		internal void ItemButton(Weapon item, int m) {
			CycleWeapons(true);
			if (m==0) CycleWeapons(true);
			else if (m==1) item.Use();
			else item.Drop();
		}

		internal void ItemButton(Flashlight item, int m) {
			item.Use();
			if (m==0) item.Use();
			if (m==1) item.Drop();
		}

		internal bool CheckForDuplicates(Item item) {
			if (item.playerPack.!=null && item.playerPack..Length>0) {
				foreach (Item elem in item.playerPack.) {
					if (item.description == elem.description
					&& item.title==elem.title&&item.icon==elem.icon
					&& item.GetType()==elem.GetType()&&item.sound==elem.sound) return true;
				}
			} return false;
		}

		public void Add(Item item) { items[item.GetType()].Add(item); }
		public void Clear() { items = new Dictionary<type,List<Item>>(); }
		public bool Contains(Item item) {
			foreach (var list in items.Values)
				foreach (var elem in list)
					if (elem.uuid==item.uuid && elem==item) return true;
			return false;
		}

		public void Remove(Item item) { items[item.GetType()].Remove(item); }

		internal Item[] GetNearbyItems() {
			Collider[] temp = Physics.OverlapSphere(
				transform.position,radiusGet,1<<layerItem);
			ArrayList items = new ArrayList();
			foreach (Collider entity in temp)
				if (entity.gameObject.GetComponent<Item>())
					items.Add(entity.gameObject.GetComponent<Item>());
			return items.ToArray(typeof(Item)) as Item[];
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return (IEnumerator) GetEnumerator(); }

		public BagEnum GetEnumerator() {
			return new BagEnum(_items[typeof (Item)]); }
		public class BagEnum : IEnumerator {
			List<Item> _items;
			int position = -1;

			public Item Current {
				get {
					try { return items[position]; }
					catch (IndexOutOfRangeException) {
						throw new InvalidOperatonException();
					}
				}
			}

			public BagEnum(Item[] _items) { this._items = new List<Item>(_items); }
			public BagEnum(List<Item> _items) { this._items = _items; }

			public bool MoveNext() {
				position++;
				return (position<items.Length);
			}
		}
#endif
	}
}
