using UnityEngine;
using System.Collections;

public class ProjectSpriteToPlane : MonoBehaviour {

	[SerializeField]
	Plane _plane;

	private Transform _transform;
	private Transform _parentTransform;

	void Awake () {
		_transform = transform;
		_parentTransform = transform.parent;
	}

	void LateUpdate () {
		Vector2 pos2d = (Vector2)_parentTransform.position;
		pos2d.x *= -1f;
		Vector3 pos = ProjectionMath.ThreeDimCoordsOnPlane (pos2d, _plane) + _plane.Normal*0.15f;
		Debug.DrawLine(pos, pos+Vector3.up);
		_transform.position = pos;
	}
}
