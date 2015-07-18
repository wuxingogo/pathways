/* Ben Scott*bescott@andrew.cmu.edu*2015-07-07*Experimental Motor */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using invt=PathwaysEngine.Inventory;
using term=PathwaysEngine.UserInterface;
using util=PathwaysEngine.Utilities;

namespace PathwaysEngine.Movement {
	public class PrototypeMotor : MonoBehaviour {
		public enum transfer { None, initial, PermaTransfer, PermaLocked }
		public transfer dirTransfer;
		bool wasJumping, newPlatform;
		public uint massPlayer;
		float maxSpeed, dampingGround, dampingAirborne, lastStartTime, lastEndTime;
		public float modSprint, modCrouch, speedAnterior, speedLateral,
			speedPosterior, speedVertical, deltaHeight, weightPerp,
			weightSteep, extraHeight, slidingSpeed, lateralControl,
			speedControl, deltaCrouch, tgtCrouch;
		public AnimationCurve responseSlope;
		public Vector3 inputMove, jumpDir, platformVelocity, velocity,
			lastVelocity, groundNormal, lastGroundNormal, hitPoint,
			lastHitPoint, activeLocalPoint, activeGlobalPoint;
		Transform hitPlatform, activePlatform, mCamr, playerGraphics;
		Quaternion activeLocalRotation, activeGlobalRotation;
		Matrix4x4 lastMatrix;
		ControllerColliderHit lastColl;
		Animator animator;
		LayerMask layerMask;
	 	Rigidbody rb;
	 	Rigidbody[] rbs;
		CapsuleCollider cl;
		Collider[] cls;
		IComparer rayHitComparer;
		public PhysicMaterial frictionZero, frictionFull;
		Transform cam;
		public Transform lookTarget { get; set; }
		public GameObject root;
		public term::Controls.InputKey OnJump, OnDash, OnDuck;
		public term::Controls.InputAxis OnAxisX, OnAxisY;

		public bool jump { get; set; }
		public bool dash { get; set; }
		public bool duck { get; set; }
		public float axisX { get; set; }
		public float axisY { get; set; }
		public bool jumping { get; set; }
		public bool isGrounded { get; set; }
		public bool wasGrounded { get; set; }
		public bool grounded {
			get { return (groundNormal.y>0.01); } set {} }
		public bool sliding {
			get { return (isGrounded && TooSteep()); } set {} }

		public bool dead {
			get { return _dead; }
			set {
				_dead = value;
				rb.detectCollisions = !value;
				foreach (var elem in cls) elem.enabled = value;
				foreach (var elem in rbs) {
					elem.detectCollisions = value;
					elem.isKinematic = !value;
					elem.velocity = (value)?(rb.velocity):(elem.velocity);
					elem.drag = 1f;
				} if (_dead) {
					Destroy(rb); Destroy(cl);
					util::CameraFade.StartAlphaFade(
						RenderSettings.fogColor,false,6f,6f);
				}
				this.enabled = !value;
				cl.enabled = !value;
				animator.enabled = !value;
			}
		} bool _dead = false;



		// tpc fields
		public bool onGround, isLookCam, falling;
		public float jumpPower, airSpeed, airControl, terminalVelocity,
			stationaryTurnSpeed, movingTurnSpeed, headLookSpeed, moveStep,
			heightStep, crouchHeight, groundStick, groundCheckDist;
		float autoTurnThresholdAngle, autoTurnSpeed, originalHeight, lastAirTime,
			turnAmount, forwardAmount, jumpRepeatDelayTime, runCycleLegOffset;
		[Range(1,4)] public float gravityMult;
		[Range(0.1f,3f)] public float animSpeedMult;
		[Range(0.1f,3f)] public float moveSpeedMult;
		Vector3 currentLookPos, moveInput, _velocity, lookPos, camForward, move;

		internal PrototypeMotor() {
			jump			= false;		dash				= false;
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
			velocity		= Vector3.zero;	lastVelocity		= Vector3.zero;
			groundNormal	= Vector3.zero;	lastGroundNormal	= Vector3.zero;
			jumpDir			= Vector3.up;	inputMove			= Vector3.zero;
			hitPoint		= Vector3.zero;
			lastHitPoint 	= new Vector3(Mathf.Infinity,0,0);
			OnJump 			= new term::Controls.InputKey((n)=>jump=n);
			OnDash			= new term::Controls.InputKey((n)=>dash=n);
			OnDuck 			= new term::Controls.InputKey((n)=>duck=n);
			OnAxisX 		= new term::Controls.InputAxis((n)=>axisX=n);
			OnAxisY 		= new term::Controls.InputAxis((n)=>axisY=n);

			// tpc
			isLookCam 				= true;
			airSpeed 				= 6.0f;		airControl 			= 2.0f;
			gravityMult 			= 2.0f;		terminalVelocity 	= 24.0f;
			moveSpeedMult			= 1.0f; 	animSpeedMult		= 1.0f;
			stationaryTurnSpeed		= 180.0f;	movingTurnSpeed 	= 360.0f;
			headLookSpeed 			= 1.0f;		groundStick 		= 5.0f;
			moveStep 				= 0.2f;		heightStep 			= 0.3f;
			crouchHeight 			= 0.6f;		//crouchChangeSpeed = 2.0f;
			autoTurnThresholdAngle	= 100.0f;	autoTurnSpeed		= 2.0f;
			jumpRepeatDelayTime 	= 0.25f;	runCycleLegOffset 	= 0.2f;
			jumpPower 				= 12.0f;	groundCheckDist 	= 0.1f;
		}

		/* internal ~PrototypeMotor() { GameObject.Destroy(mapFollower); } */
		void Awake() {
			if (Camera.main) cam = Camera.main.transform;
			layerMask = ~(LayerMask.NameToLayer("Player")
				|LayerMask.NameToLayer("EquippedItems")
				|LayerMask.NameToLayer("Items"));
			animator = GetComponentInChildren<Animator>();
			rb = (gameObject.GetComponent<Rigidbody>())
				?? gameObject.AddComponent<Rigidbody>();
			rbs = root.GetComponentsInChildren<Rigidbody>();
			cl = (gameObject.GetComponent<Collider>()) as CapsuleCollider;
			cls = root.GetComponentsInChildren<Collider>();
			if (!cl) { // operator can return null
				Destroy(gameObject.GetComponent<Collider>());
				gameObject.AddComponent<CapsuleCollider>();
				cl = gameObject.GetComponent<Collider>() as CapsuleCollider;
			}
			originalHeight = cl.height;
			cl.center = Vector3.up*originalHeight*0.5f;
			rayHitComparer = new RayHitComparer();
			dead = false;
			animator = GetComponent<Animator>();
			foreach (var childAnimator in GetComponentsInChildren<Animator>()) {
				if (childAnimator != animator) { // for hot-swapping avatars, child it
					animator.avatar = childAnimator.avatar;
					Destroy(childAnimator);
					break;
				}
			}
		}

		public void Start() {
			dirTransfer = transfer.PermaTransfer;
			mCamr = GameObject.FindGameObjectsWithTag("MainCamera")[0].transform;
			responseSlope = new AnimationCurve(new Keyframe(-90,1), new Keyframe(90,0));
		}

		public void Update() {
			if (Mathf.Abs(Time.timeScale)<0.01f) return;
			if (modSprint==0||modCrouch==0||speedAnterior==0||speedLateral==0||speedPosterior==0) return;
			Vector3 dirVector = new Vector3(axisX, 0, axisY);
			if (dirVector != Vector3.zero) {
				float dirLength = dirVector.magnitude;
				dirVector /= dirLength;
				dirLength = Mathf.Min(1f,dirLength);
				dirLength = dirLength*dirLength;
				dirVector = dirVector*dirLength;
			} inputMove = transform.rotation*dirVector;
			//UpdateFunction();
		}

		public void FixedUpdate() {
			if ((Mathf.Abs(Time.timeScale)>0.1f) && activePlatform != null) {
				if (!newPlatform)
					platformVelocity = (activePlatform.localToWorldMatrix.MultiplyPoint3x4(activeLocalPoint)
					- lastMatrix.MultiplyPoint3x4(activeLocalPoint))/((Mathf.Abs(Time.deltaTime)>0.01f)?Time.deltaTime:0.01f);
				lastMatrix = activePlatform.localToWorldMatrix;
				newPlatform = false;
			} else platformVelocity = Vector3.zero;
			// tpc
			if (cam) {
				camForward = Vector3.Scale(cam.forward,new Vector3(1,0,1)).normalized;
				move = axisY*camForward+axisX*cam.right;
			} else move = axisY*Vector3.forward+axisX*Vector3.right;
			if (move.magnitude>1) move.Normalize();
			move *= (dash)?1f:0.5f;
		    lookPos = (isLookCam && cam!=null)
		    	? transform.position+cam.forward*100
		    	: transform.position+transform.forward*100;
			Move(move,lookPos);
		}

		void UpdateFunction() {
			Vector3 tempVelocity = velocity, moveDistance = Vector3.zero;
			tempVelocity = applyDeltaVelocity(tempVelocity);
			wasGrounded = isGrounded;
			if (MoveWithPlatform()) {
				Vector3 newGlobalPoint = activePlatform.TransformPoint(activeLocalPoint);
				moveDistance = (newGlobalPoint - activeGlobalPoint);
				if (moveDistance != Vector3.zero) Move(moveDistance,lookPos);
				Quaternion newGlobalRotation = activePlatform.rotation*activeLocalRotation;
				Quaternion rotationDiff = newGlobalRotation*Quaternion.Inverse(activeGlobalRotation);
				float yRotation = rotationDiff.eulerAngles.y;
				if (yRotation != 0) transform.Rotate(0, yRotation, 0);
			}
			Vector3 lastPosition = transform.position;
			Vector3 currentMovementOffset = tempVelocity*((Mathf.Abs(Time.deltaTime)>0.01f)?Time.deltaTime:0.01f);
			float pushDownOffset = Mathf.Max(0.1f, new Vector3(currentMovementOffset.x, 0, currentMovementOffset.z).magnitude); // step offset 0.1f
			if (isGrounded) currentMovementOffset -= pushDownOffset*Vector3.up;
			hitPlatform = null;
			groundNormal = Vector3.zero;
			Move(currentMovementOffset, lookPos);
			lastHitPoint = hitPoint;
			lastGroundNormal = groundNormal;
			if (activePlatform != hitPlatform && hitPlatform != null) {
				activePlatform = hitPlatform;
				lastMatrix = hitPlatform.localToWorldMatrix;
				newPlatform = true;
			}
			Vector3 oldHVelocity = new Vector3(tempVelocity.x, 0, tempVelocity.z);
			velocity = (transform.position - lastPosition)/((Mathf.Abs(Time.deltaTime)>0.01f)?Time.deltaTime:0.01f);
			Vector3 newHVelocity = new Vector3(velocity.x, 0, velocity.z);
			if (oldHVelocity != Vector3.zero) {
				float projectedNewVelocity = Vector3.Dot(newHVelocity, oldHVelocity)/oldHVelocity.sqrMagnitude;
				velocity = oldHVelocity*Mathf.Clamp01(projectedNewVelocity)+velocity.y*Vector3.up;
			} else velocity = new Vector3(0, velocity.y, 0);
			if (velocity.y<tempVelocity.y - 0.001) {
				if (velocity.y<0) velocity.y = tempVelocity.y;
				else wasJumping = false;
			} if (isGrounded && !grounded) {
				isGrounded = false;
				if ((dirTransfer==transfer.initial || dirTransfer==transfer.PermaTransfer)) {
					lastVelocity = platformVelocity;
					velocity += platformVelocity;
				} transform.position += pushDownOffset*Vector3.up;
			} else if (!isGrounded && grounded) {
				isGrounded = true;
				jumping = false;
				SubtractNewPlatformVelocity();
			} if (MoveWithPlatform()) {
				activeGlobalPoint = transform.position+Vector3.up*(cl.center.y-cl.height*0.5f+cl.radius);
				activeLocalPoint = activePlatform.InverseTransformPoint(activeGlobalPoint);
				activeGlobalRotation = transform.rotation;
				activeLocalRotation = Quaternion.Inverse(activePlatform.rotation)*activeGlobalRotation;
			}
			slidingSpeed = (duck)?(4.0f):(15.0f);
			tgtCrouch = (duck)?1.62f:2.0f;
			if (Mathf.Abs(deltaHeight-tgtCrouch)<0.01f) deltaHeight = tgtCrouch;
			deltaHeight = Mathf.SmoothDamp(deltaHeight, tgtCrouch, ref deltaCrouch, 0.06f, 64, Time.smoothDeltaTime);
			cl.height = deltaHeight;
			if (mCamr) mCamr.localPosition = Vector3.up*(deltaHeight-0.2f);
			cl.center = Vector3.up*(deltaHeight/2.0f);
		}

		Vector3 applyDeltaVelocity(Vector3 tempVelocity) {
			Vector3 desiredVelocity;							// the horizontal to calculate direction from the jumping event
			if (isGrounded && TooSteep()) {						// and to support wall-jumping I need to change horizontal here
				desiredVelocity = new Vector3(groundNormal.x, 0, groundNormal.z).normalized;
				var projectedMoveDir = Vector3.Project(inputMove, desiredVelocity);
				desiredVelocity = desiredVelocity+projectedMoveDir*speedControl
					+ (inputMove - projectedMoveDir)*lateralControl;
				desiredVelocity *= slidingSpeed;
			} else desiredVelocity = GetDesiredHorizontalVelocity();
			if (dirTransfer==transfer.PermaTransfer) {
				desiredVelocity += lastVelocity;
				desiredVelocity.y = 0;
			} if (isGrounded) desiredVelocity = AdjustGroundVelocityToNormal(desiredVelocity, groundNormal);
			else tempVelocity.y = 0; 							// Enforce zero on Y because the axes are calculated separately
			float maxSpeedChange = GetMaxAcceleration(isGrounded)*Time.deltaTime;
			Vector3 velocityChangeVector = (desiredVelocity - tempVelocity);
			if (velocityChangeVector.sqrMagnitude > maxSpeedChange*maxSpeedChange)
				velocityChangeVector = velocityChangeVector.normalized*maxSpeedChange;
			if (isGrounded) tempVelocity += velocityChangeVector;
			if (isGrounded) tempVelocity.y = Mathf.Min(velocity.y, 0);
			if (!jump) {					// This second section aplies only the vertical axis motion but
				wasJumping = false;								// the reason I've conjoined these two is because I now have an
				lastEndTime = -100;								// interaction between the user's vertical & horizontal vectors
			} if (jump && lastEndTime<0) lastEndTime = Time.time;
			if (isGrounded) tempVelocity.y = Mathf.Min(0, tempVelocity.y) - -Physics.gravity.y*Time.deltaTime;
			else {
				tempVelocity.y = velocity.y - -Physics.gravity.y*2*Time.deltaTime;
				if (jumping && wasJumping) {
				   if (Time.time<lastStartTime+extraHeight/CalculateJumpVerticalSpeed(speedVertical))
						tempVelocity += jumpDir*-Physics.gravity.y*2*Time.deltaTime;
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
					tempVelocity += jumpDir*CalculateJumpVerticalSpeed(speedVertical);
					if (dirTransfer==transfer.initial || dirTransfer==transfer.PermaTransfer) {
						lastVelocity = platformVelocity;
						tempVelocity += platformVelocity;
					} // SendMessage("OnJump", SendMessageOptions.DontRequireReceiver);
				} else wasJumping = false;
			} //else if (cr.collisionFlags==CollisionFlags.Sides)
				//Vector3.Slerp(Vector3.up,lastColl.normal,lateralControl); yikes
			return tempVelocity;
		}
#if adsf
		void OnControllerColliderHit() { // ControllerColliderHit hit
			Rigidbody other = hit.collider.attachedRigidbody; // lastColl = hit;
			if (other && hit.moveDirection.y>-0.05)
				other.velocity = new Vector3(hit.moveDirection.x,0,hit.moveDirection.z)*(massPlayer+other.mass)/(2*-Physics.gravity.y);
			if (hit.normal.y>0 && hit.normal.y>groundNormal.y && hit.moveDirection.y<0) {
				if ((hit.point - lastHitPoint).sqrMagnitude>0.001 || lastGroundNormal==Vector3.zero) groundNormal = hit.normal;
				else groundNormal = lastGroundNormal;
				hitPlatform = hit.collider.transform;
				lastVelocity = Vector3.zero;
				hitPoint = hit.point;
			}
		}
#endif
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
				float zAxisEllipseMultiplier = (dirDesired.z > 0 ? speedAnterior : speedPosterior)/speedLateral;
				if (dash && isGrounded) zAxisEllipseMultiplier *= modSprint;
				else if (duck && isGrounded) zAxisEllipseMultiplier *= modCrouch;
				Vector3 temp = new Vector3(dirDesired.x, 0, dirDesired.z/zAxisEllipseMultiplier).normalized;
				maxSpeed = new Vector3(temp.x, 0, temp.z*zAxisEllipseMultiplier).magnitude*speedLateral;
			} if (isGrounded) {
				var movementSlopeAngle = Mathf.Asin(velocity.normalized.y)*Mathf.Rad2Deg;
				maxSpeed *= responseSlope.Evaluate(movementSlopeAngle);
			} return transform.TransformDirection(dirDesired*maxSpeed);
		}

		internal void SetVelocity(Vector3 tempVelocity) {
			velocity = tempVelocity;
			lastVelocity = Vector3.zero;
			isGrounded = false;
		}

		bool MoveWithPlatform() {
			return (isGrounded || dirTransfer==transfer.PermaLocked)&&(activePlatform); }
		Vector3 AdjustGroundVelocityToNormal( Vector3 hVelocity, Vector3 groundNormal ) {
			return Vector3.Cross(Vector3.Cross(Vector3.up, hVelocity), groundNormal).normalized*hVelocity.magnitude; }
		float GetMaxAcceleration(bool isGrounded) { return isGrounded ? dampingGround : dampingAirborne; }
		float CalculateJumpVerticalSpeed(float tgtHeight) { return Mathf.Sqrt(2*tgtHeight*-Physics.gravity.y*2); }
		bool TooSteep() { return (groundNormal.y<=Mathf.Cos(15f*Mathf.Deg2Rad)); } // 15f slope limit

		// tpc
		public void Move(Vector3 move, Vector3 lookPos) {
			if (move.magnitude>1) move.Normalize();
			this.moveInput = move;
			this.currentLookPos = lookPos;
			this._velocity = rb.velocity; // grab current velocity, we will be changing it
			// convert global move vector to local, set turn and forward motion amount
			Vector3 localMove = transform.InverseTransformDirection(moveInput);
			turnAmount = Mathf.Atan2(localMove.x, localMove.z);
			forwardAmount = localMove.z;
			if (Mathf.Abs(forwardAmount)<0.01f) { // set the character and camera abreast
				Vector3 lookDelta = transform.InverseTransformDirection(currentLookPos-transform.position);
				float lookAngle = Mathf.Atan2(lookDelta.x, lookDelta.z)*Mathf.Rad2Deg;
				if (Mathf.Abs(lookAngle)>autoTurnThresholdAngle)
					turnAmount += lookAngle*autoTurnSpeed*0.001f;
			} if (!duck) { // prevent standing up in duck to avoid ceiling problems
				Ray crouchRay = new Ray(rb.position+Vector3.up*cl.radius*0.5f,Vector3.up);
				float crouchRayLength = originalHeight-cl.radius*0.5f;
				if (Physics.SphereCast(crouchRay,cl.radius*0.5f,crouchRayLength, layerMask)) duck = true;
			} if (onGround && duck && (cl.height!=originalHeight*crouchHeight)) {
				cl.height = Mathf.MoveTowards(cl.height, originalHeight*crouchHeight,Time.smoothDeltaTime*4);
				cl.center = Vector3.MoveTowards(cl.center,Vector3.up*originalHeight*crouchHeight*0.5f,Time.smoothDeltaTime*2);
			} else if (cl.height!=originalHeight && cl.center!=Vector3.up*originalHeight*0.5f) {
				cl.height = Mathf.MoveTowards(cl.height, originalHeight, Time.smoothDeltaTime*4);
				cl.center = Vector3.MoveTowards(cl.center, Vector3.up*originalHeight*0.5f,Time.smoothDeltaTime*2);
			} // in addition to root rotation in the animations
			float turnSpeed = Mathf.Lerp(stationaryTurnSpeed, movingTurnSpeed, forwardAmount);
			transform.Rotate(0, turnAmount*turnSpeed*Time.smoothDeltaTime, 0);
			GroundCheck(); // detect and stick to ground
			cl.material = (moveInput.magnitude==0 && onGround)?frictionFull:frictionZero;
			if (onGround) {
				_velocity.y = 0;
				if (moveInput.magnitude == 0) {	_velocity.x = 0; _velocity.z = 0; }
				bool animationGrounded = animator.GetCurrentAnimatorStateInfo(0).IsName("Grounded");
				bool okToRepeatJump = Time.time>lastAirTime+jumpRepeatDelayTime;
				if (jump && !duck && okToRepeatJump && animationGrounded) {
					onGround = false;
					_velocity = moveInput*airSpeed;
					_velocity.y = jumpPower;
				}
			} else {
				// we allow some movement in air, but it's very different to when on ground
				// (typically allowing a small change in trajectory)
				Vector3 airMove = new Vector3(moveInput.x*airSpeed, _velocity.y, moveInput.z*airSpeed);
				_velocity = Vector3.Lerp(_velocity, airMove, Time.deltaTime*airControl);
				rb.useGravity = true;
				Vector3 extraGravityForce = (Physics.gravity*gravityMult) - Physics.gravity;
				rb.AddForce(extraGravityForce); // apply extra gravity from multiplier:
			}
			RaycastHit[] hits = Physics.CapsuleCastAll(
				transform.position+Vector3.up*crouchHeight,
				transform.position+Vector3.up*heightStep,
				cl.radius-0.01f, transform.forward, moveStep, layerMask);
			System.Array.Sort(hits, rayHitComparer);
			if (hits.Length>0 && hits[0].distance<cl.radius+0.01f && hits[0].distance>0) {
				_velocity.x = 0; _velocity.z = 0; }
			UpdateAnimator(); // send input and other state parameters to the animator
			rb.velocity = _velocity; // reassign velocity, it was probably modified
		}

		void GroundCheck() {
			Ray ray = new Ray(transform.position+Vector3.up*0.1f, Vector3.down);
			RaycastHit[] hits = Physics.RaycastAll(ray,0.5f);
//			RaycastHit[] hits = Physics.SphereCastAll(
//				transform.position+Vector3.up*0.1f,0.05f,Vector3.down,0.5f);//,layerMask);
			System.Array.Sort(hits, rayHitComparer);
			if (_velocity.y<jumpPower*0.5f) {
				onGround = false;
				rb.useGravity = true;
				foreach (var hit in hits) { // check whether we hit a non-trigger collider
					if (!hit.collider.isTrigger) { // this counts as being on ground
						if (_velocity.y<-terminalVelocity) {
							Player.dead = true;
							dead = true;
						} if (_velocity.y<=0)
							rb.position = Vector3.MoveTowards(
								rb.position, hit.point, Time.deltaTime*groundStick);
						onGround = true;
						rb.useGravity = false;
						break;
					}
				} // remember when we were last in air, for jump delay
			} if (!onGround) lastAirTime = Time.time;
		}

		void UpdateAnimator() {
			animator.applyRootMotion = onGround;
			animator.SetFloat("Forward", forwardAmount, 0.1f, Time.deltaTime);
			animator.SetFloat("Turn", turnAmount, 0.1f, Time.deltaTime);
			animator.SetBool("Crouch", duck);
			animator.SetBool("OnGround", onGround);
			if (!onGround) animator.SetFloat("Jump", _velocity.y);
			// calculate which leg is behind, so as to leave that leg trailing in the jump animation
			// (This code is reliant on the specific run cycle offset in our animations,
			// and assumes one leg passes the other at the normalized clip times of 0.0 and 0.5)
			float runCycle = Mathf.Repeat(animator.GetCurrentAnimatorStateInfo(0).normalizedTime+runCycleLegOffset,1);
			float jumpLeg = ((runCycle<0.5f)?1:-1)*forwardAmount;
			if (onGround) animator.SetFloat("JumpLeg", jumpLeg);
			// the anim speed multiplier allows the overall speed of walking/running to be tweaked in the inspector,
			// which affects the movement speed because of the root motion.
			animator.speed = (onGround && moveInput.magnitude>0) ? animSpeedMult : 1;
		}

		void OnAnimatorIK(int layerIndex) { // set the weight so the look-turn is done with the head
			// if a transform is assigned as a target, it takes precedence
			animator.SetLookAtWeight(1, 0.2f, 2.5f);
			if (lookTarget!=null) currentLookPos = lookTarget.position;
			animator.SetLookAtPosition(currentLookPos);	// used for the head look feature
		}

		public void OnAnimatorMove() { // override the default root motion to modify positional speed
			rb.rotation = animator.rootRotation; // before it is modified
			if (onGround && Time.deltaTime > 0) {
				Vector3 v = (animator.deltaPosition*moveSpeedMult)/Time.deltaTime;
				v.y = rb.velocity.y; // preserve the existing y component of the current velocity
				rb.velocity = v;
			}
		}

		class RayHitComparer : IComparer { // used for comparing distances
			public int Compare(object x, object y) {
				return ((RaycastHit)x).distance.CompareTo(((RaycastHit)y).distance);
			}
		}
	}
}


