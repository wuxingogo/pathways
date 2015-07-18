/* Ben Scott * bescott@andrew.cmu.edu * 2014-07-07 * Flashlight@wield */

using UnityEngine;
using System.Collections;
using invt=PathwaysEngine.Inventory;

namespace PathwaysEngine.StateMachine {
	public class FlashlightStowed : StateMachineBehaviour {
		override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
			((invt::Flashlight)(invt::Items.GetItemsOfType<invt::Flashlight>() as invt::Item[])[0]).on = false;
		}
	}
}
