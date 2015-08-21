/* Ben Scott * bescott@andrew.cmu.edu * 2014-07-25 * Item Group */

using UnityEngine;
using System.Collections;    using System.Collections.Generic;

namespace PathwaysEngine.Inventory {
	public class ItemGroup <T> : Item, IItemGroup <T> {
		int radius = 4, layerItem = 16;

		public uint count {
			get { return _count; }
			set { _count = (value==0)?(1):(value); }
		} uint _count = 1;

		public ItemGroup(T t) { }
		public ItemGroup(T t, Item item) { }
		public ItemGroup(T t, Item item, uint count) {
			this.count = count; }

		public void Group() {
			// search my container and merge
			// maybe take another stack arg
		}

		public IItemGroup<T> Split(uint n) {
			count -= n;
			return default (ItemGroup<T>);
			// instantiate new Stack _c n
		}

		internal Item[] GetNearbyItems() {
			Collider[] temp = Physics.OverlapSphere(
				transform.position, radius, 1<<layerItem);
			ArrayList items = new ArrayList();
			foreach (Collider entity in temp)
				if (entity.gameObject.GetComponent<Item>())
					items.Add(entity.gameObject.GetComponent<Item>());
			return items.ToArray(typeof(Item)) as Item[];
		}

		public void Add(Item elem) {
			if (elem.GetType()==typeof(T)) {
				count++;
				Destroy(elem.gameObject);
			} else throw new System.Exception("fool");
		}

		public void Add(ItemGroup<T> elem) {
			if (elem.GetType()==typeof (T)) {
				count+=elem.count;
				Destroy(elem.gameObject);
			} else throw new System.Exception("silly");
		}
	}
}