/* Ben Scott * bescott@andrew.cmu.edu * 2015-07-08 * Flashlight */

using UnityEngine;
using System.Collections;

namespace PathwaysEngine.Inventory {
	public class Flashlight : Lamp {
		public new void Equip() {
			print("asdf");
			Terminal.Log("You turn on your flashlight.");
			base.Equip();
		}

		public new void Stow() {
			Terminal.Log("You stow your flashlight.");
			base.Stow();
		}
	}
}

