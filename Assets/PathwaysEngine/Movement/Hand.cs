/* Ben Scott * bescott@andrew.cmu.edu * 2015-07-07 * Hand */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using invt=PathwaysEngine.Inventory;
using util=PathwaysEngine.Utilities;

namespace PathwaysEngine.Movement {
	public class Hand : MonoBehaviour {
		public bool ikActive = false;
		public Transform objHand = null;
        protected Animator animator;
        AvatarIKGoal handGoal;
		public Hands hand = Hands.Left;
		public invt::IEquippable heldItem;
		public util::key fire, lamp;

		public Hand() {
			fire = new util::key((n)=>fire.input=n);
			lamp = new util::key((n)=>lamp.input=n);
		}

		public void SwitchItem(invt::IEquippable item) {
		//	if (backpack && (heldItem!=null)) heldItem.Stow();
		//	else heldItem.Drop();
			heldItem = item;
		}

		void Awake() {
			if (hand==Hands.Right) Player.right = this;
			else if (hand==Hands.Left) Player.left = this;
		}

        void Start() {
            animator = Player.animator; //GetComponent<Animator>();
            handGoal = (hand==Hands.Left)?
                (AvatarIKGoal.LeftHand):(AvatarIKGoal.RightHand);
        }

		public void Update() {
			if (heldItem!=null && (hand==Hands.Left && lamp.input
			|| hand==Hands.Right && fire.input)) heldItem.Use();
		}

        void OnAnimatorIK() {
            if (animator) {
                if (ikActive) {
                    if (objHand!=null) { // Set the target position and rotation
                        animator.SetIKPositionWeight(handGoal,1);
                        animator.SetIKRotationWeight(handGoal,1);
                        animator.SetIKPosition(handGoal,objHand.position);
                        animator.SetIKRotation(handGoal,objHand.rotation);
                    }
                } else {
                    animator.SetIKPositionWeight(handGoal,0);
                    animator.SetIKRotationWeight(handGoal,0);
                    //animator.SetLookAtWeight(0);
                }
            }
        }
	}
}