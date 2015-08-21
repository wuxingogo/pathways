/* Ben Scott * bescott@andrew.cmu.edu * 2015-07-07 * Backpack */

using UnityEngine;			using type=System.Type;
using System.Collections;	using System.Collections.Generic;

namespace PathwaysEngine.Inventory {
	public class Backpack : Bag, IEquippable {

		~Backpack() { DropAll(); }

		public bool worn {
			get { return _worn; }
			set { _worn = value;
				if (_worn) Equip();
				else Stow(); }
		} bool _worn;

		public uint uses { get; set; }

		public Backpack() { }

		public void Use() { worn = !worn; }
		public override void Take() { base.Take(); worn = true; }
		//public override void Drop() { base.Drop(); worn = false; }
		public void Equip() {
			if (gameObject) gameObject.SetActive(true);
			Player.Equip(this);
		}

		public void Stow() { gameObject.SetActive(false); }
		public void DropAll() { foreach (var elem in this) elem.Drop(); }
	}
} // print("Stow!");
