/* Ben Scott * bescott@andrew.cmu.edu * 2015-07-07 * Bag */

using UnityEngine;
using System.Collections;   using System.Collections.Generic;

namespace PathwaysEngine.Inventory {
	public class Bag : MonoBehaviour {
		internal bool hasScreenControl, isIgnoringInput;
		internal int radiusGet, layerItem, invItemValues;
		internal float reUpdateTimer, reuptakeTimer;
		internal Flashlight cLight;
		internal Item[] invItems, allItems;
		internal Weapon[] invWeapons;
		internal List<Item> nearItems;
		internal GameObject rPlayer;
		internal Transform localTR;
		internal Camera mCAMR;

		public Bag() {
			hasScreenControl	= false;	isIgnoringInput	= false;
			radiusGet 			= 16;		layerItem 		= 16;
			reUpdateTimer 		= 0f;		reuptakeTimer 	= 3f;
		}

		internal void Awake() {
			localTR = transform;
			invItems = GetComponentsInChildren<Item>();
			invWeapons = GetComponentsInChildren<Weapon>();
			rPlayer = GameObject.FindGameObjectWithTag("Player");
			Pause.PausePlayer(false);
			allItems = Object.FindObjectsOfType(typeof (Item)) as Item[];
		}

		internal void Update() {
			reuptakeTimer += Time.deltaTime;
			if (!isIgnoringInput) {
				if (!hasScreenControl && Input.GetButtonDown("InventoryToggle") || Input.GetButtonDown("MenuToggle")) {
					Pause.PausePlayer(true);
					hasScreenControl = true;
				} else if (hasScreenControl && Input.GetButtonDown("MenuToggle") || Input.GetButtonDown("InventoryToggle")) {
					Pause.PausePlayer(false);
					hasScreenControl = false;
				} // StartCoroutine(InventorySlow());
			} // if (Mathf.Abs(Input.GetAxis("Mouse ScrollWheel"))>0.1) CycleWeapons(true);
			if (mCAMR) mCAMR.pixelRect = new Rect(Screen.width-320, 64, 256, 256);
			else mCAMR = GameObject.FindGameObjectWithTag("Map Camera").GetComponent<Camera>();
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
			if (item.playerPack.invItems!=null && item.playerPack.invItems.Length>0) {
				foreach (Item elem in item.playerPack.invItems) {
					if (item.description == elem.description
					&& item.title==elem.title&&item.icon==elem.icon
					&& item.GetType()==elem.GetType()&&item.sound==elem.sound) return true;
				}
			} return false;
		}
#endif

		internal Item[] GetNearbyItems() {
			Collider[] temp = Physics.OverlapSphere(localTR.position, radiusGet, 1<<layerItem);
			ArrayList items = new ArrayList();
			foreach (Collider entity in temp)
				if (entity.gameObject.GetComponent<Item>())
					items.Add(entity.gameObject.GetComponent<Item>());
			return items.ToArray(typeof(Item)) as Item[];
		}

		internal void AddRemoveItem() { // Brutish Method, Updates Slowly
			invItems = GetComponentsInChildren<Item>();
		//	invClips = GetComponentsInChildren<Clip>();
			invWeapons = GetComponentsInChildren<Weapon>();
			reUpdateTimer = 0;
		}

		internal void AddRemoveItem(Item curItem) { invItems = ArrayF.Push(invItems, curItem); }

//	public T AddRemoveItem<T> ( T ItemType ) { ItemType = (Component)GetComponentInChildren(typeof (T)); } //Generic Overload
	} //*/
}
