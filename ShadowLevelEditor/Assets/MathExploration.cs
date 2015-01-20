using UnityEngine;
using System.Collections;

public class MathExploration : MonoBehaviour {

	public float __sa;
	public Transform a;
	public Transform b;
	public Transform n;

	private float SignedAngle (Vector3 a, Vector3 b, Vector3 normal) {
	    return Mathf.Atan2(Vector3.Dot(normal, Vector3.Cross(a, b)), Vector3.Dot(a, b));
	}

	void Update () {
		__sa = SignedAngle(a.position - transform.position, b.position - transform.position, n.position - transform.position);
	}
}
