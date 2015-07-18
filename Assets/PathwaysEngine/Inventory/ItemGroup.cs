/* Ben Scott * bescott@andrew.cmu.edu * 2014-07-06 * Item Group */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PathwaysEngine.Inventory {
	public class ItemGroup : MonoBehaviour {//, IItem, IStack {
//		public int worth;
//		public uint weight, count;
		internal int radius, layerItem;
		internal Item[] items, allItems;
		internal List<Item> nearItems;
		internal Transform tr;

		public ItemGroup() { radius = 4; layerItem = 16; }

		internal void Awake() {
			tr = transform;
			allItems = Items.GetItemsOfType<Item>();
		}

		internal void Start() {

		}

		internal Item[] GetNearbyItems() {
			Collider[] temp = Physics.OverlapSphere(tr.position, radius, 1<<layerItem);
			ArrayList items = new ArrayList();
			foreach (Collider entity in temp)
				if (entity.gameObject.GetComponent<Item>())
					items.Add(entity.gameObject.GetComponent<Item>());
			return items.ToArray(typeof(Item)) as Item[];
		}

		internal void AddRemoveItem(Item curItem) { items = ArrayF.Push(items, curItem); }

		internal void DropAll() { foreach (Item elem in items) elem.Drop(); }
	} //*/
}
