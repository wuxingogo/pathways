/* Ben Scott * bescott@andrew.cmu.edu * 2014-07-07 * Flashlight@wield */

using UnityEngine;
using System.Collections;
using invt=PathwaysEngine.Inventory;

namespace PathwaysEngine.Movement.StateMachine {
	public class FlashlightWield : StateMachineBehaviour {
		invt::Flashlight flashlight;

		override public void OnStateEnter(Animator a,AnimatorStateInfo asi,int i) {
			flashlight = (invt::Flashlight) Pathways.player.holdall.GetItemOfType<invt::Flashlight>();
			flashlight.worn = true;
		}

//		override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
//			flashlight.isOn = true;
//		}
	}
}
