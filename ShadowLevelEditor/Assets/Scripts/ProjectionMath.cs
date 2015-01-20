using UnityEngine;
using System.Collections;

public class ProjectionMath : MonoBehaviour {

	public static Vector3 ThreeDimCoordsOnPlane (Vector2 point, Plane plane) {
		return (point.x * plane.Right) + (point.y * plane.Up) + plane.Origin;
	}

	public static Vector3 ProjectPointToPlane (Vector3 point, Plane plane) {
		Vector3 originToPoint = point - plane.Origin;
		float dist = Vector3.Dot(originToPoint, plane.Normal);
		return point - dist * plane.Normal;
	}

	public static Vector2 TwoDimCoordsOnPlane (Vector3 point, Plane plane) {
		return new Vector2(Vector3.Dot(point - plane.Origin, plane.Right), Vector3.Dot(point - plane.Origin, plane.Up));
	}

}