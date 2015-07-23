/* Ben Scott * bescott@andrew.cmu.edu * 2015-07-08 * Flashlight */

using UnityEngine;
using System.Collections;
using term=PathwaysEngine.UserInterface;

namespace PathwaysEngine.Inventory {
	public class Flashlight : Lamp {
		public new void Equip() {
			base.Equip();
			term::Terminal.Log("You turn on your flashlight.");
		}

		public new void Stow() {
			base.Stow();
			term::Terminal.Log("You stow your flashlight.");
		}
	}
}

