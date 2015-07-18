#pragma strict
var CollisionSounds : AudioClip[];
var au : AudioSource;
var SingleUse : boolean;
private var UseAgain : boolean = false;

function OnAwake () { au = GetComponent(AudioSource); }

function OnCollisionEnter (Entrant : Collision) {
	if (!au.isPlaying && UseAgain) {
		if (Entrant.relativeVelocity.magnitude > 0.5) {
			au.clip = CollisionSounds[Random.Range(0, CollisionSounds.length)];
			au.volume = (Entrant.relativeVelocity.magnitude / 8);
			au.pitch = (Random.value * 0.5 + 0.5);
			au.Play();
			if (SingleUse) UseAgain = false;
		}
	}
}
