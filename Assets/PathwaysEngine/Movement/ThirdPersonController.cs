/* Ben Scott * bescott@andrew.cmu.edu * 2015-07-07 * Third Person Controller */

//#define DEBUG

using UnityEngine;
using System.Collections;
using invt=PathwaysEngine.Inventory;
using util=PathwaysEngine.Utilities;

namespace PathwaysEngine.Movement {
	[RequireComponent(typeof(Animator))]
	public class ThirdPersonController : MonoBehaviour {
		public bool onGround, isLookCam, falling; //crouchChangeSpeed,
		public float jumpPower, airSpeed, airControl, terminalVelocity,
			stationaryTurnSpeed, movingTurnSpeed, headLookSpeed, moveStep,
			heightStep, crouchHeight, groundStick, groundCheckDist;
		float autoTurnThresholdAngle, autoTurnSpeed, originalHeight, lastAirTime,
			turnAmount, forwardAmount, jumpRepeatDelayTime, runCycleLegOffset;
		[Range(1,4)] public float gravityMult;
		[Range(0.1f,3f)] public float animSpeedMult;
		[Range(0.1f,3f)] public float moveSpeedMult;
		Vector3 currentLookPos, moveInput, velocity, lookPos, camForward, move;
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
		public util::key jump, dash, duck;
		public util::axis axisX, axisY;

		public bool dead {
			get { return _dead; }
			set { _dead = value;
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
					StartCoroutine(ProlongDeath(8f));
				}
				this.enabled = !value;
				cl.enabled = !value;
				animator.enabled = !value;
			}
		} bool _dead = false;

		public ThirdPersonController() {
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
			jump					= new util::key((n)=>jump.input=n);
			dash 					= new util::key((n)=>dash.input=n);
			duck 					= new util::key((n)=>duck.input=n);
			axisX 					= new util::axis((n)=>axisX.input=n);
			axisY 					= new util::axis((n)=>axisY.input=n);
		}

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
			SetUpAnimator();
			dead = false;
		}

		void FixedUpdate() {
			if (cam) {
				camForward = Vector3.Scale(cam.forward,new Vector3(1,0,1)).normalized;
				move = axisY.input*camForward+axisX.input*cam.right;
			} else move = axisY.input*Vector3.forward+axisX.input*Vector3.right;
			if (move.magnitude>1) move.Normalize();
			move *= (dash.input)?1f:0.5f;
		    lookPos = (isLookCam && cam!=null)
		    	? transform.position+cam.forward*100
		    	: transform.position+transform.forward*100;
			Move(move,lookPos);
		}

		public void Move(Vector3 move, Vector3 lookPos) {
			if (move.magnitude>1) move.Normalize();
			this.moveInput = move;
			this.currentLookPos = lookPos;
			this.velocity = rb.velocity; // grab current velocity, we will be changing it
			// convert global move vector to local, set turn and forward motion amount
			Vector3 localMove = transform.InverseTransformDirection(moveInput);
			turnAmount = Mathf.Atan2(localMove.x, localMove.z);
			forwardAmount = localMove.z;
			if (Mathf.Abs(forwardAmount)<0.01f) { // set the character and camera abreast
				Vector3 lookDelta = transform.InverseTransformDirection(currentLookPos-transform.position);
				float lookAngle = Mathf.Atan2(lookDelta.x, lookDelta.z)*Mathf.Rad2Deg;
				if (Mathf.Abs(lookAngle)>autoTurnThresholdAngle)
					turnAmount += lookAngle * autoTurnSpeed * 0.001f;
			} if (!duck.input) { // prevent standing up in duck to avoid ceiling problems
				Ray duckRay = new Ray(rb.position+Vector3.up*cl.radius*0.5f,Vector3.up);
				float duckRayLength = originalHeight-cl.radius*0.5f;
				if (Physics.SphereCast(duckRay,cl.radius*0.5f,duckRayLength, layerMask)) duck.input = true;
			} if (onGround && duck.input && (cl.height!=originalHeight*crouchHeight)) {
				cl.height = Mathf.MoveTowards(cl.height, originalHeight*crouchHeight,Time.smoothDeltaTime*4);
				cl.center = Vector3.MoveTowards(cl.center,Vector3.up*originalHeight*crouchHeight*0.5f,Time.smoothDeltaTime*2);
			} else if (cl.height!=originalHeight && cl.center!=Vector3.up*originalHeight*0.5f) {
				cl.height = Mathf.MoveTowards(cl.height, originalHeight, Time.smoothDeltaTime * 4);
				cl.center = Vector3.MoveTowards(cl.center, Vector3.up*originalHeight*0.5f,Time.smoothDeltaTime*2);
			} // in addition to root rotation in the animations
			float turnSpeed = Mathf.Lerp(stationaryTurnSpeed, movingTurnSpeed, forwardAmount);
			transform.Rotate(0, turnAmount*turnSpeed*Time.smoothDeltaTime, 0);
			GroundCheck(); // detect and stick to ground
			SetFriction(); // use low or high friction values, depending
			if (onGround) HandleGroundedVelocities();
			else HandleAirborneVelocities();
#if DEBUG
			Debug.DrawRay(transform.position+Vector3.up*(crouchHeight), transform.forward);
			Debug.DrawRay(transform.position+Vector3.up*heightStep, transform.forward);
#endif
			RaycastHit[] hits = Physics.CapsuleCastAll(
				transform.position+Vector3.up*crouchHeight,
				transform.position+Vector3.up*heightStep,
				cl.radius-0.01f, transform.forward, moveStep, layerMask);
			System.Array.Sort(hits, rayHitComparer);
			if (hits.Length>0 && hits[0].distance<cl.radius+0.01f && hits[0].distance>0) {
				velocity.x = 0; velocity.z = 0; }
			UpdateAnimator(); // send input and other state parameters to the animator
			rb.velocity = velocity; // reassign velocity, it was probably modified
			falling = (rb.velocity.y<-terminalVelocity); // falling death
		}

		void GroundCheck() {
			Ray ray = new Ray(transform.position+Vector3.up*0.1f, Vector3.down);
			RaycastHit[] hits = Physics.RaycastAll(ray,0.5f);
//			RaycastHit[] hits = Physics.SphereCastAll(
//				transform.position+Vector3.up*0.1f,0.05f,Vector3.down,0.5f);//,layerMask);
			System.Array.Sort(hits, rayHitComparer);
			if (velocity.y<jumpPower*0.5f) {
				onGround = false;
				rb.useGravity = true;
				foreach (var hit in hits) { // check whether we hit a non-trigger collider
#if DEBUG
					DrawPoint(hit.point, Color.blue);
					DrawPoint(rb.position, Color.red);
#endif
					if (!hit.collider.isTrigger) { // this counts as being on ground
						if (velocity.y<-terminalVelocity || falling) {
							Player.dead = true;
							dead = true;
						} else if (velocity.y<=0)
							rb.position = Vector3.MoveTowards(
								rb.position, hit.point, Time.deltaTime*groundStick);
						onGround = true;
						rb.useGravity = false;
						break;
					}
				} // remember when we were last in air, for jump delay
			} if (!onGround) lastAirTime = Time.time;
		}

		void SetFriction() { cl.material = (moveInput.magnitude==0 && onGround)?frictionFull:frictionZero; }

		void HandleGroundedVelocities() {
			velocity.y = 0;
			if (moveInput.magnitude == 0) {	velocity.x = 0; velocity.z = 0; }
			bool animationGrounded = animator.GetCurrentAnimatorStateInfo(0).IsName("Grounded");
			bool okToRepeatJump = Time.time>lastAirTime + jumpRepeatDelayTime;
			if (jump.input && !duck.input && okToRepeatJump && animationGrounded) {
				onGround = false;
				velocity = moveInput * airSpeed;
				velocity.y = jumpPower;
			}
		}

		void HandleAirborneVelocities() {
			// we allow some movement in air, but it's very different to when on ground
			// (typically allowing a small change in trajectory)
			Vector3 airMove = new Vector3(moveInput.x * airSpeed, velocity.y, moveInput.z * airSpeed);
			velocity = Vector3.Lerp(velocity, airMove, Time.deltaTime * airControl);
			rb.useGravity = true;
			Vector3 extraGravityForce = (Physics.gravity * gravityMult) - Physics.gravity;
			rb.AddForce(extraGravityForce); // apply extra gravity from multiplier:
		}

		void UpdateAnimator() {
			animator.applyRootMotion = onGround;
			animator.SetFloat("Forward", forwardAmount, 0.1f, Time.deltaTime);
			animator.SetFloat("Turn", turnAmount, 0.1f, Time.deltaTime);
			animator.SetBool("Crouch", duck.input);
			animator.SetBool("OnGround", onGround);
			if (!onGround) animator.SetFloat("Jump", velocity.y);
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

		void SetUpAnimator() {
			animator = GetComponent<Animator>(); // this is a ref to animator@root
			foreach (var childAnimator in GetComponentsInChildren<Animator>()) {
				if (childAnimator != animator) { // for hot-swapping avatars, child it
					animator.avatar = childAnimator.avatar;
					Destroy (childAnimator);
					break;
				}
			}
		}

		public void OnAnimatorMove() { // override the default root motion to modify positional speed
			rb.rotation = animator.rootRotation; // before it is modified
			if (onGround && Time.deltaTime > 0) {
				Vector3 v = (animator.deltaPosition * moveSpeedMult) / Time.deltaTime;
				v.y = rb.velocity.y; // preserve the existing y component of the current velocity
				rb.velocity = v;
			}
		}

		IEnumerator ProlongDeath(float t) {
			yield return new WaitForSeconds(t); // Application.LoadLevel(1);
		}

#if DEBUG
		public void DrawPoint(Vector3 p, Color c) {
			for (var i=8;i>0;--i)
				Debug.DrawRay(p,Random.onUnitSphere*0.05f, c);
		}
#endif

		class RayHitComparer : IComparer { // used for comparing distances
			public int Compare(object x, object y) {
				return ((RaycastHit)x).distance.CompareTo(((RaycastHit)y).distance);
			}
		}
	}
}
