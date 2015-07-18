#pragma strict

var ExplosionRadius = 5.0;
var ExplosionPower = 10.0;
var ExplosionDamage = 100.0;
var ExplosionTimeout = 2.0;
var vFactor : float = 3;

function Start () {
	var ExplosionPosition = transform.position;
	var Colliders : Collider[] = Physics.OverlapSphere (ExplosionPosition, ExplosionRadius);
	for (var Hit in Colliders) {
		// Calculate distance from the explosion position to the closest point on the collider
		var ClosestPoint = Hit.ClosestPointOnBounds(ExplosionPosition);
		var Distance = Vector3.Distance(ClosestPoint, ExplosionPosition);
		// The hit points we apply fall decrease with distance from the explosion point
		var HitPoints = 1.0 - Mathf.Clamp01(Distance / ExplosionRadius);
		HitPoints *= ExplosionDamage;
		// Tell the rigidbody or any other script attached to the hit object how much damage is to be applied
		Hit.SendMessage("ApplyDamage", HitPoints, SendMessageOptions.DontRequireReceiver);
	}
	// Enemies are first turned into ragdolls with ApplyDamage then we apply forces to all the spawned body parts
	Colliders = Physics.OverlapSphere (ExplosionPosition, ExplosionRadius);
	for (var Hit in Colliders) {
		if (Hit.GetComponent(Rigidbody)) Hit.GetComponent(Rigidbody).AddExplosionForce(ExplosionPower, ExplosionPosition, ExplosionRadius, vFactor);
	}
	if (GetComponent(ParticleEmitter)) {
        GetComponent(ParticleEmitter).emit = true;
		yield WaitForSeconds(0.5);
		GetComponent(ParticleEmitter).emit = false;
    } Destroy (gameObject, ExplosionTimeout);
}
