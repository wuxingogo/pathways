/* Ben Scott * bescott@andrew.cmu.edu * 2015-07-12 * Player */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using type=System.Type;
using invt=PathwaysEngine.Inventory;
using mvmt=PathwaysEngine.Movement;
using stat=PathwaysEngine.Statistics;
using term=PathwaysEngine.UserInterface;
using util=PathwaysEngine.Utilities;

namespace PathwaysEngine {
	public class Player : MonoBehaviour {
		static public Dictionary<type,invt::Item[]> items;
		static public invt::Backpack backpack;
		static public mvmt::ThirdPersonController tpc;
		static public mvmt::Hand right, left;

		static public bool dead {
			get { return tpc.dead; }
			set { _dead = value;
				if (_dead) {
					foreach (var elem in items[typeof(invt::Item)])
						elem.Drop();
					term::Terminal.Log("*** YOU HAVE DIED. ***");
					term::Terminal.Log("Would you like to Restore, Restart, or Quit?");
					term::Terminal.focus = true;
				}
			}
		} static bool _dead = false;

		void Awake() { // petrichor is a cool word//Pathways.player = this;
			items = new Dictionary<type,invt::Item[]>();
			items[typeof(invt::Item)] = GetComponentsInChildren<invt::Item>();
			backpack = GetComponentInChildren<invt::Backpack>();
			tpc = GetComponentInChildren<mvmt::ThirdPersonController>();
			var temp = GetComponentsInChildren<mvmt::Hand>();
			foreach (var elem in temp)
				if (elem.hand==mvmt::Hand.Handedness.Right) right = elem;
				else left = elem;
		}

		public static invt::Item[] GetPlayerItems<T>() where T : invt::Item {
			if (items.ContainsKey(typeof (T))) return items[typeof (T)];
			else return new invt::Item[0];
		}

		public void ResetPlayerLocalPosition() {
			tpc.transform.localPosition = Vector3.zero;
		}
	}
}