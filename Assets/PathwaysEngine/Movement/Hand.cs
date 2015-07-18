/* Ben Scott * bescott@andrew.cmu.edu * 2015-07-07 * Hand */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using invt=PathwaysEngine.Inventory;
using HandError=System.NullReferenceException;

namespace PathwaysEngine.Movement {
	public class Hand : MonoBehaviour {
		public enum Handedness : byte { Left, Right };
		public Handedness hand = Handedness.Left;
		public invt::IEquippable heldItem;
		public invt::Backpack backpack;

		public void Update() {
			if (heldItem!=null && (hand==Handedness.Left && Input.GetButton("HandLeft")
			|| hand==Handedness.Right && Input.GetButton("HandRight"))) heldItem.Use();
		}

		public void SwitchItem(invt::IEquippable item) {
			if (backpack && (heldItem!=null)) heldItem.Stow();
			else heldItem.Drop();
			heldItem = item;
		}
	}
}
