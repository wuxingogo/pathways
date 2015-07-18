/* Ben Scott * bescott@andrew.cmu.edu * 2015-07-07 * Camera Collider */

//using System;
using UnityEngine;
using System.Collections;

namespace PathwaysEngine.Utilities {
	public class ProtectCameraFromWallClip : MonoBehaviour {
		public bool visualiseInEditor;
		float originalDist, moveVelocity, currentDist;
		public float clipMoveTime = 0.05f, returnTime = 0.4f;
	    public float sphereCastRadius = 0.1f, closestDistance = 0.5f;
		public string dontClipTag = "Player";

	    public bool protecting {get; private set;} // object blocking

	    Transform cam, pivot;
	    Ray ray; // casting between the camera and the target
	    RaycastHit[] hits;
	    RayHitComparer rayHitComparer;
	    LayerMask layerMask;

		void Start() {
			layerMask = ~(LayerMask.NameToLayer("Player")
				|LayerMask.NameToLayer("EquippedItems")
				|LayerMask.NameToLayer("Items"));
			cam = GetComponentInChildren<Camera>().transform;
			pivot = cam.parent;
			originalDist = cam.localPosition.magnitude;
			currentDist = originalDist;
	        rayHitComparer = new RayHitComparer();
		}

		void LateUpdate() {
			float targetDist = originalDist;
		    ray.origin = pivot.position+pivot.forward*sphereCastRadius;
		    ray.direction = -pivot.forward;
			Collider[] cols = Physics.OverlapSphere(ray.origin,sphereCastRadius,layerMask);
			bool initialIntersect = false;
			bool hitSomething = false;
		    for (int i=0;i<cols.Length;i++) { // loop collisions
				if ((!cols[i].isTrigger) && !(cols[i].attachedRigidbody!=null
				&& cols[i].attachedRigidbody.CompareTag(dontClipTag))) {
		            initialIntersect = true;
		            break;
		        }
		    } if (initialIntersect) {
				ray.origin += pivot.forward * sphereCastRadius;
	            // do a raycast and gather all the intersections
				hits = Physics.RaycastAll(ray, originalDist - sphereCastRadius, layerMask);
			} else hits = Physics.SphereCastAll(ray, sphereCastRadius, originalDist + sphereCastRadius, layerMask);
			System.Array.Sort(hits, rayHitComparer); // sort by distance
			float nearest = Mathf.Infinity; // set storing the closest far as possible
		    for (int i=0;i<hits.Length;i++) {
	            // only deal with the collision if it was closer than the previous one, not a trigger, and not attached to a rigidbody tagged with the dontClipTag
				if (hits[i].distance<nearest && (!hits[i].collider.isTrigger)
				&& !(hits[i].collider.attachedRigidbody!=null
				&& hits[i].collider.attachedRigidbody.CompareTag(dontClipTag))) {
					nearest = hits[i].distance;
	                targetDist = -pivot.InverseTransformPoint(hits[i].point).z;
		            hitSomething = true;
		        }
		    }

			if (hitSomething)
				Debug.DrawRay(ray.origin, -pivot.forward * (targetDist + sphereCastRadius),Color.red );
		    protecting = hitSomething; // hit something so move the camera to a better position
			currentDist = Mathf.SmoothDamp(currentDist, targetDist, ref moveVelocity, currentDist > targetDist ? clipMoveTime : returnTime );
			currentDist = Mathf.Clamp(currentDist,closestDistance,originalDist);
			cam.localPosition = -Vector3.forward * currentDist;

		}

		public class RayHitComparer: IComparer { // cmp for distances in raycast
			public int Compare(object x, object y) {
				return ((RaycastHit)x).distance.CompareTo(((RaycastHit)y).distance);
			}
		}
	}
}

