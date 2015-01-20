using UnityEngine;
using System.Collections;

public class ProjectionMath : MonoBehaviour {

	public static Vector3 ThreeDimCoordsOnPlane (Vector2 point, Plane plane) {
		return (point.x * plane.Right) + (point.y * plane.Up) + plane.Origin;
	}
}
