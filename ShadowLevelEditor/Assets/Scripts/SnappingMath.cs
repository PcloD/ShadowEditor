using UnityEngine;
using System.Collections;

public class SnappingMath : MonoBehaviour {

	public static Vector3 SnapToRoundedOffset (Vector3 point, float precision) {
		Vector3 result = point;
		result.x = Mathf.Round(point.x / precision) * precision - precision/2f;
		result.y = Mathf.Round(point.y / precision) * precision - precision/2f;
		result.z = Mathf.Round(point.z / precision) * precision - precision/2f;
		return result;
	}

	public static Vector3 SnapToRounded (Vector3 point, float precision) {
		Vector3 result = point;
		result.x = Mathf.Round(point.x / precision) * precision;
		result.y = Mathf.Round(point.y / precision) * precision;
		result.z = Mathf.Round(point.z / precision) * precision;
		return result;
	}

}