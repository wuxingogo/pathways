using UnityEngine;
using System.Collections;

public class SuperCubeLightRotate : MonoBehaviour {
	[SerializeField]
	Vector3 mAxisDist;
	[SerializeField]
	Vector3 mAxisSpeed = Vector3.one;
	[SerializeField]
	Vector3 mTimeOffset;
	
	Vector3 mStartPos;
	Transform mTrans;
	
	void Start() {
		mTrans = transform;
		mStartPos = transform.position;
	}
	
	void Update () {
		mTrans.position = mStartPos +  new Vector3(
			Mathf.Cos((Time.time + mTimeOffset.x) * mAxisSpeed.x) * mAxisDist.x,
			Mathf.Cos((Time.time + mTimeOffset.y) * mAxisSpeed.y) * mAxisDist.y,
			Mathf.Sin((Time.time + mTimeOffset.z) * mAxisSpeed.z) * mAxisDist.z);
	}
}
