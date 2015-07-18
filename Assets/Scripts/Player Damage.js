#pragma strict
private var Controller : CharacterController;
private var au : AudioSource;
//private var Player : GameObject;
var maximumHitPoints = 100.0;
var hitPoints = 100.0;
var deadReplacement : Transform;
var WalkSounds : AudioClip[];
var SprintSounds : AudioClip[];
var PitchVariance : long = .2;
var painLittle : AudioClip;
var painBig : AudioClip;
var painNegative : AudioClip;
var audioStepLength = 0.3;
private var gotHitTimer = -1.0;
enum PlayerStatesEnum { Walk, Crouch, Sprint, Stop }
var PlayerStateChoice : PlayerStatesEnum = PlayerStatesEnum.Walk;

function Start () {
	Controller = GetComponent(CharacterController);
	au = GetComponent(AudioSource);
	//Player = GameObject.FindGameObjectWithTag("Player");
	PlayStepSounds();
}

function ApplyDamage (damage : float) {
	if (hitPoints < 0.0)	return;
	if (damage < 0.0) 	au.PlayOneShot(painNegative, 1.0/au.volume);
	hitPoints -= damage;
	if (Time.time > gotHitTimer && painBig && painLittle) {
		if (hitPoints < maximumHitPoints * 0.2 || damage > 20) {
			au.PlayOneShot(painBig, 1.0 / au.volume);
			gotHitTimer = Time.time + Random.Range(painBig.length * 2, painBig.length * 3);
		} else {
			au.PlayOneShot(painLittle, 1.0 / au.volume);
			gotHitTimer = Time.time + Random.Range(painLittle.length * 2, painLittle.length * 3);
		}
	}
	if (hitPoints < 0.0)	Detonate();
}
/*
function OnGUI() {
	GUI.color = Color.red;
	GUI.HorizontalScrollbar(Rect (Screen.width*0.2, 32, Screen.width*0.8-128,96), 0, hitPoints, 0, maximumHitPoints);
}
*/
function Detonate () {
	Destroy(gameObject);
	if (deadReplacement) Instantiate(deadReplacement, transform.position, transform.rotation);
		//var dead : Transform = Instantiate(deadReplacement, transform.position, transform.rotation);
		//CopyTransformsRecurse(transform, dead);
	//}
}
/*
static function CopyTransformsRecurse (src : Transform,  dst : Transform) {
	    dst.position = src.position;
	    dst.rotation = src.rotation;
	    for (var child : Transform in dst) {
	        var curSrc = src.Find(child.name);
	        if (curSrc)
	            CopyTransformsRecurse(curSrc, child);
	    }
}//*/

function PlayStepSounds () { //StepNoise : boolean
	while (true) {
		if (Controller.isGrounded && Controller.velocity.magnitude > 0.3) {
			switch (PlayerStateChoice) {
				case (PlayerStatesEnum.Walk) :
					au.clip = (WalkSounds[Random.Range(0, WalkSounds.length)]);
					au.pitch = Random.Range(1-PitchVariance, 1+PitchVariance);
					au.Play();
					yield WaitForSeconds(audioStepLength);
				break;
				case (PlayerStatesEnum.Sprint) :
						au.clip = SprintSounds[Random.Range(0, SprintSounds.length)];
						au.pitch = Random.Range(1-PitchVariance, 1+PitchVariance);
						au.Play();
						yield WaitForSeconds(audioStepLength);
				break;
			}
		} else yield;
	}
}


/*
function PlayStepSounds (PlayerState : PlayerStatesEnum) {
	switch (PlayerStateChoice) {
		case (PlayerState.Walk) :
				if (Controller.isGrounded && Controller.velocity.magnitude > 0.3) {
					audio.PlayOneShot(WalkSounds[Random.Range(0, WalkSounds.length)]);
					yield WaitForSeconds(audioStepLength);
				} break;
		case (PlayerState.Sprint) :
			if (Controller.isGrounded && Controller.velocity.magnitude > 0.3) {
				//audio.clip = SprintSounds[Random.Range(0, SprintSounds.length)];
				audio.PlayOneShot(SprintSounds[Random.Range(0, SprintSounds.length)]);
				yield WaitForSeconds(audioStepLength);
			}	break;
	}
}//*/
