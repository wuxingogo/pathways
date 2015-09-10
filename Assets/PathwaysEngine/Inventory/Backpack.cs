/* Ben Scott * bescott@andrew.cmu.edu * 2015-07-07 * Backpack */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PathwaysEngine.Inventory {
	public class Backpack : Bag, IWearable {
		public bool worn {
			get { return _worn; }
			set { _worn = value;
				if (_worn) Wear();
				else Stow(); }
		} bool _worn;

		~Backpack() { DropAll(); }

		public override void Take() { base.Take(); Player.Wear(this); }
		public override void Drop() { base.Drop(); Player.Stow(this); }

		public void Wear() {
			gameObject.SetActive(true);
			Terminal.Log("You put on the backpack.\n",Formats.Command);
		}

		public void Stow() {
			Terminal.Log("You take off the backpack.\n",Formats.Command);
			gameObject.SetActive(false);
		}
	}
}
