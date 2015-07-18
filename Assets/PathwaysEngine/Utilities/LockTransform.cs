/* Ben Scott * bescott@andrew.cmu.edu * 2014-07-07 * Lock Transform */

#define NONFUNCTIONAL

using UnityEngine;
using System.Collections;

namespace PathwaysEngine.Utilities {
	public class LockTransform : MonoBehaviour {
		public bool isLocked = true, useInit = true;
		Vector3 initT = Vector3.zero;
		Transform src, initParent;
		public Transform tgt;

		void Start() {
			src = transform;
			initParent = src.parent;
			src.parent = tgt;
			if (useInit) initT = src.localPosition;
			src.parent = initParent;
		}

		void FixedUpdate() {
			if (isLocked && src && tgt) {
				src.parent = tgt;
				src.localPosition = initT;
				//src.position = tgt.position+initT;
				src.localRotation = Quaternion.identity;
				src.parent = initParent;
				//tgt.rotation; // multiply quaterions?
			}
		}
	}
}