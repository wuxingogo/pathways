var destination : Transform;
var teleportationSound : AudioClip;

function OnTriggerEnter(other : Collider) {
	au = GetComponent(AudioSource);
    other.transform.position = destination.position;
    au.clip = teleportationSound;
    au.Play();
}
