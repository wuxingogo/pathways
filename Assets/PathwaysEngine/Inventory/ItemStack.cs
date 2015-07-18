/* Ben Scott * bescott@andrew.cmu.edu * 2014-07-10 * Item Stack */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PathwaysEngine.Inventory {
	public class ItemStack : Item, IStack {
		public uint count {
			get { return _count; }
			set { _count = (value==0)?(1):(value); }
		} uint _count;

		public ItemStack (Item item) {
			// get stuff to be the same from the item
			// maybe require stack
			this.count = 1;
		}

		public ItemStack (uint count) {
			this.count = count;
		}

		public void Stack() {
			// search my container and merge
			// maybe take another stack arg
		}

		public ItemStack Split(uint n) {
			count -= n;
			return default (ItemStack);
			// instantiate new Stack _c n
		}
	}
}
