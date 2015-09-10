/* Ben Scott * bescott@andrew.cmu.edu * 2015-08-11 * Equipment Base */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PathwaysEngine.Inventory {
	public abstract class Equipment : Item, IWearable {

		public uint uses { get; set; }

		public bool worn {
			get { return _worn; }
			set { _worn = value;
				if (_worn) Wear();
				else Stow(); }
		} bool _worn;

		public void Use() { worn = !worn; }
		public override void Take() { base.Take(); worn = true; }
		public override void Drop() { base.Drop(); worn = false; }
		public void Wear() { if (gameObject) gameObject.SetActive(true); }
		public void Stow() { gameObject.SetActive(false); }
	}
}
