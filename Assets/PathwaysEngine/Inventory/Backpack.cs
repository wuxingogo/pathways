/* Ben Scott * bescott@andrew.cmu.edu * 2015-07-07 * Backpack */

using UnityEngine;
using System.Collections;	using System.Collections.Generic;

namespace PathwaysEngine.Inventory {
	public class Backpack : MonoBehaviour {
		internal bool hasControl, isIgnoringInput;
		internal int radiusGet, layerItem, invItemValues;
		internal float reUpdateTimer; // reuptakeTimer;
		internal Item[] invItems, items;
		internal Weapon[] invWeapons;
		internal Flashlight cLight;
		internal List<Item> nearItems;
		internal GameObject rPlayer;

		public Backpack() {
			hasControl 		= false;	isIgnoringInput	= false;
			radiusGet 		= 16;		layerItem 		= 16;
			reUpdateTimer 	= 0f;		//reuptakeTimer = 3f;
		}

#if TODO
		public ~Backpack() { DropAll(); }
#endif

		internal void Awake() {
			invItems = GetComponentsInChildren<Item>();
			invWeapons = GetComponentsInChildren<Weapon>();
//			rPlayer = GameObject.FindGameObjectWithTag("Player");
			rPlayer = Pathways.player.gameObject;
			Pause.PausePlayer(false);
			items = Items.GetItemsOfType<Item>() as Item[];
		}

		internal bool CheckForDuplicates(Item item) {
			if (invItems!=null && invItems.Length>0)
				foreach (Item elem in invItems)
					if (elem==item) return true;
			return false;
		}

#if arcane
				{	if (item.description==elem.description
					&& item.title==elem.title
					&& item.icon==elem.icon
					&& item.GetType()==elem.GetType()
					&& item.sound==elem.sound) return true;
				}
			} return false;
#endif

		internal Item[] GetNearbyItems() {
			Collider[] temp = Physics.OverlapSphere(transform.position, radiusGet, 1<<layerItem);
			ArrayList list = new ArrayList();
			foreach (Collider entity in temp)
				if (entity.gameObject.GetComponent<Item>())
					list.Add(entity.gameObject.GetComponent<Item>());
			return list.ToArray(typeof(Item)) as Item[];
		}

		internal void AddRemoveItem() { // Brutish Method, Updates Slowly
			invItems = GetComponentsInChildren<Item>();
			invWeapons = GetComponentsInChildren<Weapon>();
		//	invClips = GetComponentsInChildren<Clip>();
			reUpdateTimer = 0;
		}

		internal void DropAll() { foreach (Item elem in invItems) elem.Drop(); }

		internal void AddRemoveItem(Item curItem) { invItems = ArrayF.Push(invItems, curItem); }

//	public T AddRemoveItem<T> ( T ItemType ) { ItemType = (Component)GetComponentInChildren(typeof (T)); } //Generic Overload
	} //*/
}







