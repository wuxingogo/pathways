/* Ben Scott * bescott@andrew.cmu.edu * 2015-07-07 * Character Motor */

using UnityEngine;  		 using term=PathwaysEngine.UserInterface;
using System.Collections;    using System.Collections.Generic;

namespace PathwaysEngine.Movement {
	[RequireComponent(typeof (CharacterController))]
	public class CharacterMotor : MonoBehaviour {
		internal bool wasJumping, newPlatform;
		uint massPlayer;
		internal float maxSpeed, dampingGround, dampingAirborne,
			lastStartTime, lastEndTime, deltaPitch, deltaStep;
		public float modSprint, modCrouch, speedAnterior, speedLateral,
			speedPosterior, speedVertical, deltaHeight, weightPerp,
			weightSteep, extraHeight, slidingSpeed, lateralControl,
			speedControl, deltaCrouch, tgtCrouch;
		public enum transfer { None, initial, PermaTransfer, PermaLocked }
		public transfer dirTransfer;
		public AnimationCurve responseSlope;
		internal CollisionFlags hitFlags;
		internal Vector3 inputMove, jumpDir, platformVelocity, velocity,
			lastVelocity, groundNormal, lastGroundNormal, hitPoint,
			lastHitPoint, activeLocalPoint, activeGlobalPoint;
		internal Transform hitPlatform, activePlatform, mCamr, playerGraphics;
		internal Quaternion activeLocalRotation, activeGlobalRotation;
		internal Matrix4x4 lastMatrix;
		CharacterController cr;
		ControllerColliderHit lastColl;
		public term::key jump, dash, duck;
		public term::axis axisX, axisY;

		public bool jumping { get; set; }
		public bool isGrounded { get; set; }
		public bool wasGrounded { get; set; }
		public bool grounded {
			get { return (groundNormal.y>0.01); } set {} }
		public bool sliding {
			get { return (isGrounded && TooSteep()); } set {} }

		internal CharacterMotor() {
			jumping			= false;		wasJumping			= false;
			maxSpeed		= 57.2f;		massPlayer			= 80;
			dampingGround	= 30.0f;		dampingAirborne		= 20.0f;
			modSprint		= 1.6f;			modCrouch			= 0.8f;
			speedAnterior	= 16.0f;		speedPosterior		= 10.0f;
			speedLateral	= 12.0f;		speedVertical		= 1.0f;
			extraHeight		= 4.1f;			slidingSpeed		= 15.0f;
			weightPerp		= 0.0f;			weightSteep			= 0.5f;
			lastStartTime	= 0.0f;			lastEndTime			= -100.0f;
			lateralControl	= 1.0f;			speedControl		= 0.4f;
			deltaCrouch 	= 1.0f;			deltaHeight			= 2.0f;
			deltaPitch		= 0.2f;			deltaStep			= 0.3f;
			velocity		= Vector3.zero;	lastVelocity		= Vector3.zero;
			groundNormal	= Vector3.zero;	lastGroundNormal	= Vector3.zero;
			jumpDir			= Vector3.up;	inputMove			= Vector3.zero;
			hitPoint		= Vector3.zero;
			lastHitPoint 	= new Vector3(Mathf.Infinity,0,0);
			jump 			= new term::key((n)=>jump.input=n);
			dash			= new term::key((n)=>dash.input=n);
			duck 			= new term::key((n)=>duck.input=n);
			axisX 			= new term::axis((n)=>axisX.input=n);
			axisY 			= new term::axis((n)=>axisY.input=n);
		}

		/* internal ~CharacterMotor() { GameObject.Destroy(mapFollower); } */

		public void Start() {
			dirTransfer = transfer.PermaTransfer;
			cr = GetComponent<CharacterController>();
			mCamr = GameObject.FindGameObjectsWithTag("MainCamera")[0].transform;
			responseSlope = new AnimationCurve(new Keyframe(-90,1), new Keyframe(90,0));
		}

		public void Update() {
			if (Mathf.Abs(Time.timeScale)<0.01f) return;
			if (modSprint==0||modCrouch==0||speedAnterior==0||speedLateral==0||speedPosterior==0) return;
			Vector3 dirVector = new Vector3(axisX.input, 0, axisY.input);
			if (dirVector != Vector3.zero) {
				float dirLength = dirVector.magnitude;
				dirVector /= dirLength;
				dirLength = Mathf.Min(1.0f,dirLength);
				dirLength = dirLength * dirLength;
				dirVector = dirVector * dirLength;
			} inputMove = transform.rotation * dirVector;
			UpdateFunction();
		}

		public void FixedUpdate() {
			if ((Mathf.Abs(Time.timeScale)>0.1f) && activePlatform != null) {
				if (!newPlatform)
					platformVelocity = (activePlatform.localToWorldMatrix.MultiplyPoint3x4(activeLocalPoint)
					- lastMatrix.MultiplyPoint3x4(activeLocalPoint))/((Mathf.Abs(Time.deltaTime)>0.01f)?Time.deltaTime:0.01f);
				lastMatrix = activePlatform.localToWorldMatrix;
				newPlatform = false;
			} else platformVelocity = Vector3.zero;
		}


		void UpdateFunction() {
			Vector3 tempVelocity = velocity, moveDistance = Vector3.zero;
			tempVelocity = applyDeltaVelocity(tempVelocity);
			wasGrounded = isGrounded;
			if (MoveWithPlatform()) {
				Vector3 newGlobalPoint = activePlatform.TransformPoint(activeLocalPoint);
				moveDistance = (newGlobalPoint - activeGlobalPoint);
				if (moveDistance != Vector3.zero) cr.Move(moveDistance);
				Quaternion newGlobalRotation = activePlatform.rotation * activeLocalRotation;
				Quaternion rotationDiff = newGlobalRotation * Quaternion.Inverse(activeGlobalRotation);
				float yRotation = rotationDiff.eulerAngles.y;
				if (yRotation != 0) transform.Rotate(0, yRotation, 0);
			}
			Vector3 lastPosition = transform.position;
			Vector3 currentMovementOffset = tempVelocity * ((Mathf.Abs(Time.deltaTime)>0.01f)?Time.deltaTime:0.01f);
			float pushDownOffset = Mathf.Max(cr.stepOffset, new Vector3(currentMovementOffset.x, 0, currentMovementOffset.z).magnitude);
			if (isGrounded) currentMovementOffset -= pushDownOffset * Vector3.up;
			hitPlatform = null;
			groundNormal = Vector3.zero;
			hitFlags = cr.Move(currentMovementOffset);		// This one moves the user and returns the direction of the hit
			lastHitPoint = hitPoint;
			lastGroundNormal = groundNormal;
			if (activePlatform != hitPlatform && hitPlatform != null) {
				activePlatform = hitPlatform;
				lastMatrix = hitPlatform.localToWorldMatrix;
				newPlatform = true;
			}
			Vector3 oldHVelocity = new Vector3(tempVelocity.x, 0, tempVelocity.z);
			velocity = (transform.position - lastPosition) / ((Mathf.Abs(Time.deltaTime)>0.01f)?Time.deltaTime:0.01f);
			Vector3 newHVelocity = new Vector3(velocity.x, 0, velocity.z);
			if (oldHVelocity != Vector3.zero) {
				float projectedNewVelocity = Vector3.Dot(newHVelocity, oldHVelocity) / oldHVelocity.sqrMagnitude;
				velocity = oldHVelocity * Mathf.Clamp01(projectedNewVelocity) + velocity.y * Vector3.up;
			} else velocity = new Vector3(0, velocity.y, 0);
			if (velocity.y < tempVelocity.y - 0.001) {
				if (velocity.y < 0) velocity.y = tempVelocity.y;
				else wasJumping = false;
			} if (isGrounded && !grounded) {
				isGrounded = false;
				if ((dirTransfer==transfer.initial || dirTransfer==transfer.PermaTransfer)) {
					lastVelocity = platformVelocity;
					velocity += platformVelocity;
				} transform.position += pushDownOffset * Vector3.up;
			} else if (!isGrounded && grounded) {
				isGrounded = true;
				jumping = false;
				SubtractNewPlatformVelocity();
			} if (MoveWithPlatform()) {
				activeGlobalPoint = transform.position
					+ Vector3.up * (cr.center.y - cr.height * 0.5f + cr.radius);
				activeLocalPoint = activePlatform.InverseTransformPoint(activeGlobalPoint);
				activeGlobalRotation = transform.rotation;
				activeLocalRotation = Quaternion.Inverse(activePlatform.rotation) * activeGlobalRotation;
			}
			slidingSpeed = (duck.input)?(4.0f):(15.0f);
			tgtCrouch = (duck.input)?1.62f:2.0f;
			if (Mathf.Abs(deltaHeight-tgtCrouch)<0.01f) deltaHeight = tgtCrouch;
			deltaHeight = Mathf.SmoothDamp(deltaHeight, tgtCrouch, ref deltaCrouch, 0.06f, 64, Time.smoothDeltaTime);
			cr.height = deltaHeight;
			if (mCamr) mCamr.localPosition = Vector3.up*(deltaHeight-0.2f);
			cr.center = Vector3.up*(deltaHeight/2.0f);
		}

		Vector3 applyDeltaVelocity(Vector3 tempVelocity) {
			Vector3 desiredVelocity;							// the horizontal to calculate direction from the jumping event
			if (isGrounded && TooSteep()) {						// and to support wall-jumping I need to change horizontal here
				desiredVelocity = new Vector3(groundNormal.x, 0, groundNormal.z).normalized;
				var projectedMoveDir = Vector3.Project(inputMove, desiredVelocity);
				desiredVelocity = desiredVelocity + projectedMoveDir * speedControl
					+ (inputMove - projectedMoveDir) * lateralControl;
				desiredVelocity *= slidingSpeed;
			} else desiredVelocity = GetDesiredHorizontalVelocity();
			if (dirTransfer==transfer.PermaTransfer) {
				desiredVelocity += lastVelocity;
				desiredVelocity.y = 0;
			} if (isGrounded) desiredVelocity = AdjustGroundVelocityToNormal(desiredVelocity, groundNormal);
			else tempVelocity.y = 0; 							// Enforce zero on Y because the axes are calculated separately
			float maxSpeedChange = GetMaxAcceleration(isGrounded) * Time.deltaTime;
			Vector3 velocityChangeVector = (desiredVelocity - tempVelocity);
			if (velocityChangeVector.sqrMagnitude > maxSpeedChange * maxSpeedChange)
				velocityChangeVector = velocityChangeVector.normalized * maxSpeedChange;
			if (isGrounded) tempVelocity += velocityChangeVector;
			if (isGrounded) tempVelocity.y = Mathf.Min(velocity.y, 0);
			if (!jump.input) {					// This second section aplies only the vertical axis motion but
				wasJumping = false;								// the reason I've conjoined these two is because I now have an
				lastEndTime = -100;								// interaction between the user's vertical & horizontal vectors
			} if (jump.input && lastEndTime<0) lastEndTime = Time.time;
			if (isGrounded) tempVelocity.y = Mathf.Min(0, tempVelocity.y) - -Physics.gravity.y * Time.deltaTime;
			else {
				tempVelocity.y = velocity.y - -Physics.gravity.y*2 * Time.deltaTime;
				if (jumping && wasJumping) {
				   if (Time.time < lastStartTime + extraHeight / CalculateJumpVerticalSpeed(speedVertical))
						tempVelocity += jumpDir * -Physics.gravity.y*2 * Time.deltaTime;
				} tempVelocity.y = Mathf.Max(tempVelocity.y, -maxSpeed);
			} if (isGrounded) {
				if (Time.time-lastEndTime<0.2) {
					isGrounded = false;
					jumping = true;
					lastStartTime = Time.time;
					lastEndTime = -100;
					wasJumping = true;
					if (TooSteep()) jumpDir = Vector3.Slerp(Vector3.up, groundNormal, weightSteep);
					else jumpDir = Vector3.Slerp(Vector3.up, groundNormal, weightPerp);
					tempVelocity.y = 0;
					tempVelocity += jumpDir * CalculateJumpVerticalSpeed(speedVertical);
					if (dirTransfer==transfer.initial || dirTransfer==transfer.PermaTransfer) {
						lastVelocity = platformVelocity;
						tempVelocity += platformVelocity;
					} // SendMessage("jump", SendMessageOptions.DontRequireReceiver);
				} else wasJumping = false;
			} else if (cr.collisionFlags==CollisionFlags.Sides)
				Vector3.Slerp(Vector3.up,lastColl.normal,lateralControl);
			return tempVelocity;
		}

		void OnControllerColliderHit(ControllerColliderHit hit) {
			Rigidbody other = hit.collider.attachedRigidbody;
			lastColl = hit;
			if (other && hit.moveDirection.y>-0.05) other.velocity = new Vector3(hit.moveDirection.x,0,hit.moveDirection.z)
																	*(massPlayer+other.mass)/(2*-Physics.gravity.y);
			if (hit.normal.y > 0 && hit.normal.y > groundNormal.y && hit.moveDirection.y < 0) {
				if ((hit.point - lastHitPoint).sqrMagnitude>0.001 || lastGroundNormal==Vector3.zero) groundNormal = hit.normal;
				else groundNormal = lastGroundNormal;
				hitPlatform = hit.collider.transform;
				lastVelocity = Vector3.zero;
				hitPoint = hit.point;
			}
		}

		IEnumerator SubtractNewPlatformVelocity() {
			if (dirTransfer==transfer.initial || dirTransfer==transfer.PermaTransfer) {
				if (newPlatform) {
					Transform platform = activePlatform;
					yield return new WaitForFixedUpdate();		// Both yields are present as a kind of corruption of von Braun
					yield return new WaitForFixedUpdate();		// style redundancy as it might be near or have missed the call
					if (isGrounded && platform==activePlatform) yield break;
				} velocity -= platformVelocity;
			}
		}

		Vector3 GetDesiredHorizontalVelocity() {
			Vector3 dirDesired = transform.InverseTransformDirection(inputMove);
		   	float maxSpeed = 0.0f;
			if (dirDesired != Vector3.zero) {
				float zAxisEllipseMultiplier = (dirDesired.z > 0 ? speedAnterior : speedPosterior) / speedLateral;
				if (dash.input && isGrounded) zAxisEllipseMultiplier *= modSprint;
				else if (duck.input && isGrounded) zAxisEllipseMultiplier *= modCrouch;
				Vector3 temp = new Vector3(dirDesired.x, 0, dirDesired.z / zAxisEllipseMultiplier).normalized;
				maxSpeed = new Vector3(temp.x, 0, temp.z * zAxisEllipseMultiplier).magnitude * speedLateral;
			} if (isGrounded) {
				var movementSlopeAngle = Mathf.Asin(velocity.normalized.y) * Mathf.Rad2Deg;
				maxSpeed *= responseSlope.Evaluate(movementSlopeAngle);
			} return transform.TransformDirection(dirDesired * maxSpeed);
		}

		internal void SetVelocity(Vector3 tempVelocity) {
			velocity = tempVelocity;
			lastVelocity = Vector3.zero;
			isGrounded = false;
		}

		bool MoveWithPlatform() {
			return (isGrounded || dirTransfer==transfer.PermaLocked)&&(activePlatform); }
		Vector3 AdjustGroundVelocityToNormal( Vector3 hVelocity, Vector3 groundNormal ) {
			return Vector3.Cross(Vector3.Cross(Vector3.up, hVelocity), groundNormal).normalized * hVelocity.magnitude; }
		float GetMaxAcceleration(bool isGrounded) { return isGrounded ? dampingGround : dampingAirborne; }
		float CalculateJumpVerticalSpeed(float tgtHeight) { return Mathf.Sqrt(2*tgtHeight*-Physics.gravity.y*2); }
		bool IsTouchingCeiling() { return (hitFlags&CollisionFlags.CollidedAbove)!=0; }
		bool TooSteep() { return (groundNormal.y <= Mathf.Cos(cr.slopeLimit*Mathf.Deg2Rad)); }
	}
}

