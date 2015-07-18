using UnityEngine;
using System.Collections;

public class SuperCubeCameraRotate : MonoBehaviour {
	public Vector3 mLookatPoint      = Vector3.zero;
	public Vector3 mRotationDistance = new Vector3(10, 10, 10);
	public float   mSpeed = 2;
	
	
	void Start () {
		Update();
	}
	
	void Update () {
		transform.position = mLookatPoint + Vector3.Scale(mRotationDistance, new Vector3(Mathf.Cos(Time.time * mSpeed), 1, Mathf.Sin(Time.time * mSpeed)));
		transform.LookAt(mLookatPoint);
	}
}
