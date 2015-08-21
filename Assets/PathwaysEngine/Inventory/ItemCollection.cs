/* Ben Scott * bescott@andrew.cmu.edu * 2015-08-04 * Item Collection */

using UnityEngine;			using type=System.Type;
using System.Collections;   using System.Collections.Generic;

namespace PathwaysEngine.Inventory {
	public abstract class ItemCollection : Item, IItemCollection {
		public bool IsSynchronized { get { return false; } }
		public object SyncRoot { get { return default (object); } }
		public int Count { get { return items.Count; } }
		public uint maxCount { get; set; }

		public ItemSet items { get { return _items; }} ItemSet _items;

		public ItemCollection() { this._items = new ItemSet(); }
		public ItemCollection(List<Item> items) {
			this._items = new ItemSet(items); }

		IEnumerator IEnumerable.GetEnumerator() {
			return (IEnumerator) GetEnumerator(); }

		public IEnumerator<Item> GetEnumerator() {
			return (IEnumerator<Item>) new ItemCollectionEnumerator(items); }

		public bool Contains(Item item) { return items.Contains(item); }

	 	public void CopyTo(System.Array itemArr, int n) { }//items.CopyTo(itemArr,n); }

	 	public Item GetItemOfType<T>() where T : Item {
	 		return items.GetItemOfType<T>(); }
	 	public List<Item> GetItemsOfType<T>() where T : Item{
	 		return items.GetItemsOfType<T>(); }

		public class ItemCollectionEnumerator : IEnumerator {
			List<Item> _items;
			int position = -1;

			public object Current {
				get {
					try { return _items[position]; }
					catch (System.IndexOutOfRangeException) {
						throw new System.Exception();
					}
				}
			}

			public ItemCollectionEnumerator(Item[] list) {
				this._items = new List<Item>(list); }

			public ItemCollectionEnumerator(List<Item> list) {
				this._items = list; }

			public ItemCollectionEnumerator(ItemSet list) {
				this._items = list[typeof (Item)]; }

			public bool MoveNext() {
				position++;
				return (position<_items.Count);
			}

			public void Reset() { position = -1; }
		}
	}
}
