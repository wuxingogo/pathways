/* Ben Scott * bescott@andrew.cmu.edu * 2014-07-06 * Damage */

using UnityEngine;
using System.Collections;

public class Focus : MonoBehaviour {
	bool isFar = true;
	int mask;
	float distance = 0f;
	float max = 64f;
	DepthOfField34 DepthOfField;
	RaycastHit hit;

	public void Awake() {
		DepthOfField = GetComponent<DepthOfField34>();
		mask = ~(LayerMask.NameToLayer("Player")&LayerMask.NameToLayer("Equipped Items"));
	}

	public void Update() {
		DepthOfField.focalPoint = (isFar)?(max):(distance);
		DepthOfField.enabled = !isFar;
		DepthOfField.focalPoint = distance;
	}

	public void FixedUpdate() {
		isFar = !Physics.SphereCast(transform.position, 0.25f, transform.forward, out hit, max, mask);
		Debug.DrawRay(transform.position, transform.forward*hit.distance, Color.red);
		distance = hit.distance;
	}
}
