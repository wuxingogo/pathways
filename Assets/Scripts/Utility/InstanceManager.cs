using UnityEngine;
using System;
using System.Collections;

public class InstanceManager : MonoBehaviour {

	[SerializeField] internal bool offCameraDelete = false;
	[SerializeField] internal bool childDetach = false;
	[SerializeField] internal float uInterval = 5;
	[SerializeField] internal AudioClip[] rSounds;
	[NonSerialized] private Transform[] rTransforms;
	
	
	void Start (  ) {
		if (GetComponent<ParticleSystem>()) uInterval = GetComponent<ParticleSystem>().time;
		if (GetComponent<AudioSource>()) GetComponent<AudioSource>().PlayOneShot (rSounds[UnityEngine.Random.Range(0, rSounds.Length)]);
		if (!offCameraDelete && childDetach) {
			if (!transform.parent) transform.DetachChildren();
			else {
				rTransforms = GetComponentsInChildren<Transform>();
				foreach (Transform rChild in rTransforms) rChild.parent = transform.parent;
			} Destroy(gameObject);
		} else if (!offCameraDelete && !childDetach) StartCoroutine(WaitAndDestroy());
	}
	
	void OnBecameInvisible (  ) { if (offCameraDelete) StartCoroutine(WaitAndDestroy()); }
	
	IEnumerator WaitAndDestroy (  ) {
		yield return new WaitForSeconds (uInterval);
		if (!GetComponent<Renderer>().isVisible) DestroyObject (gameObject);
	}
}