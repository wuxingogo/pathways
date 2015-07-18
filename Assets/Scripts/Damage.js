#pragma strict
var Vitality = 100.0;
var Explosion : Transform;
var DamageParticles : Transform;
var deadReplacement : Transform;
var deadReplacementRoot : Rigidbody;
var DeathNoise : AudioClip;
var CriticalDamage : int = 20;
var PhysicsApplicable : boolean;

function ApplyDamage (Damage : int) {
	Vitality -= Damage;
	if (Vitality <= 0 && Vitality >= -CriticalDamage) FatalReplace (false);
	if (Vitality <= CriticalDamage)  FatalReplace (true);
}

function FatalReplace (Critical : boolean) {
	if (DeathNoise) GetComponent(AudioSource).PlayOneShot(DeathNoise);
	//yield WaitForEndOfFrame();
	if (Explosion) Instantiate (Explosion, transform.position, transform.rotation);
	if (deadReplacement) {
		Instantiate(deadReplacement, transform.position, transform.rotation);
		yield WaitForSeconds (.1);
		if (PhysicsApplicable) {
			deadReplacementRoot.velocity = GetComponent(Rigidbody).velocity;
			deadReplacementRoot.angularVelocity = GetComponent(Rigidbody).angularVelocity;
		}
	}
	Destroy(gameObject);
}
