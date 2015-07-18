/* Ben Scott * bescott@andrew.cmu.edu * 2014-07-07 * Flashlight@wield */

using UnityEngine;
using System.Collections;
using invt=PathwaysEngine.Inventory;

namespace PathwaysEngine.StateMachine {
	public class FlashlightWield : StateMachineBehaviour {
		invt::Flashlight flashlight;

		override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
			flashlight = (invt::Flashlight)(invt::Items.GetItemsOfType<invt::Flashlight>() as invt::Item[])[0];
			flashlight.worn = true;
		}

//		override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
//			flashlight.isOn = true;
//		}
	}
}
