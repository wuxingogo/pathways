#pragma strict

var delay = 1.0;
var timeOut = 1.0;
var detachChildren = false;
var explosion : Transform;
var explodeAfterBounce : boolean = false;
private var hasCollided : boolean = false;
private var explodeTime : float;
//private var hasExploded : boolean = false;

function Start () {
explodeTime = Time.time+timeOut;
}

function OnCollisionEnter (collision : Collision){
	if(hasCollided || !explodeAfterBounce) DestroyNow();
	yield new WaitForSeconds(delay);
	hasCollided = true;
}

function DestroyNow () {
		if (detachChildren) transform.DetachChildren ();
	DestroyObject (gameObject);
	Instantiate (explosion, transform.position, transform.rotation);
}

function Update (){
   transform.TransformDirection(Vector3.forward);
   //var hit : RaycastHit;
   if (Time.time > explodeTime) DestroyNow();
}