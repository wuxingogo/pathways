#pragma strict

var layerMask : LayerMask; //make sure we aren't in this layer
var cl : Collider;
var skinWidth : float = 0.1; //probably doesn't need to be changed
private var minimumExtent : float;
private var partialExtent : float;
private var sqrMinimumExtent : float;
private var previousPosition : Vector3;
private var myRigidbody : Rigidbody;
//initialize values

var delay = .0001;
var timeOut = 1.0;
var detachChildren = false;
var explosion : Transform;
var explodeAfterBounce : boolean = false;
var projectileDamage : float = 1000;
private var hasCollided : boolean = false;
private var explodeTime : float;
//private var hasExploded : boolean = false;

function Awake() {
   myRigidbody = GetComponent(Rigidbody);
   cl = GetComponent(Collider);
   previousPosition = myRigidbody.position;
   minimumExtent = Mathf.Min(Mathf.Min(cl.bounds.extents.x, cl.bounds.extents.y), cl.bounds.extents.z);
   partialExtent = minimumExtent*(1.0 - skinWidth);
   sqrMinimumExtent = minimumExtent*minimumExtent;
}

function Start (){
explodeTime = Time.time+timeOut;
}

function DestroyNow () {
		if (detachChildren) {
			transform.DetachChildren ();
		}
		DestroyObject (gameObject);
		Instantiate (explosion, transform.position, transform.rotation);
		SendMessage("ApplyDamage", projectileDamage, SendMessageOptions.DontRequireReceiver);
}

function OnCollisionEnter (collision : Collision){
	if(hasCollided || !explodeAfterBounce){
		DestroyNow();
	yield new WaitForSeconds(delay);
	hasCollided = true;
	}
}

function Update () {
   var projDirection = transform.TransformDirection(Vector3.forward);
   var hit : RaycastHit;
   if(Physics.Raycast (transform.position, projDirection, hit)){
   	DestroyNow();
   }
   if(Time.time > explodeTime)
   DestroyObject (gameObject);
}

function FixedUpdate() {
   //have we moved more than our minimum extent?
   var movementThisStep : Vector3 = myRigidbody.position - previousPosition;
   var movementSqrMagnitude : float = movementThisStep.sqrMagnitude;
   if (movementSqrMagnitude > sqrMinimumExtent) {
      var movementMagnitude : float = Mathf.Sqrt(movementSqrMagnitude);
      var hitInfo : RaycastHit;
      //check for obstructions we might have missed
      if (Physics.Raycast(previousPosition, movementThisStep, hitInfo, movementMagnitude, layerMask.value)){
         myRigidbody.position = hitInfo.point - (movementThisStep/movementMagnitude)*partialExtent;
         DestroyNow();
    previousPosition = myRigidbody.position;
    	}
	}
}
