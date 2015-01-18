using UnityEngine;
using System.Collections;

public class Plane : MonoBehaviour {
	public Vector3 Origin {
		get { return transform.position; }
	}
	public Vector3 Up {
		get { return -transform.forward; }
	}
	public Vector3 Right {
		get { return -transform.right; }
	}
	public Vector3 Normal {
		get { return -transform.up; }
	}
}
