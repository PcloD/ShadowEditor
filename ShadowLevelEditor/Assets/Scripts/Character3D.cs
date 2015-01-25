using UnityEngine;
using System.Collections;

public class Character3D : MonoBehaviour {

	[SerializeField]
	GameObject _playerxy;
	[SerializeField]
	GameObject _playerzy;
	Renderer _renderer;
	[SerializeField]
	float _visibilityThreshold = 0.1f;
	[SerializeField]
	float _raycastEpsilon = 0.1f;
	[SerializeField]
	float _pivotGridPrecision = 0.5f;


	Transform _playerxyTransform;
	Transform _playerzyTransform;
	BoxCollider2D _playerxyCollider;
	BoxCollider2D _playerzyCollider;

	Transform _transform;

	void Awake () {
		_playerxyTransform = _playerxy.transform;
		_playerzyTransform = _playerzy.transform;
		_playerxyCollider = _playerxy.GetComponent<BoxCollider2D>();
		_playerzyCollider = _playerzy.GetComponent<BoxCollider2D>();
		_transform = transform;
		_renderer = GetComponent<Renderer>();
	}

	private float YPosition {
		get { return (_playerxyTransform.position.y + _playerzyTransform.position.y)/2f; }
	}

	public bool DoesExist {
		get { return Mathf.Abs(_playerxyTransform.position.y - _playerzyTransform.position.y) < _visibilityThreshold; }
	}

	public void Rotate(float direction) {
		RaycastHit[] hitInfo = new RaycastHit[4];
		bool[] didHit = new bool[4];
		float maxDistance = Mathf.Infinity;
		Collider hitCollider = null;

		Vector3 pivot = SnappingMath.SnapToRoundedOffset (_transform.position, _pivotGridPrecision);
		Debug.DrawLine(pivot - new Vector3(0,10,0), pivot + new Vector3(0,10,0), Color.red, 10f);

		didHit[0] = Physics.Raycast(pivot + Vector3.up * _raycastEpsilon + Vector3.right * _playerxyCollider.size.x/2f, -Vector3.up, out hitInfo[0]);
		didHit[1] = Physics.Raycast(pivot + Vector3.up * _raycastEpsilon + Vector3.left * _playerxyCollider.size.x/2f, -Vector3.up, out hitInfo[1]);
		didHit[2] = Physics.Raycast(pivot + Vector3.up * _raycastEpsilon + Vector3.forward * _playerzyCollider.size.x/2f, -Vector3.up, out hitInfo[2]);
		didHit[3] = Physics.Raycast(pivot + Vector3.up * _raycastEpsilon + Vector3.back * _playerzyCollider.size.x/2f, -Vector3.up, out hitInfo[3]);
		for (int i = 0; i<4; i++) {
			float currDistance = hitInfo[i].distance;
			if (didHit[i] && currDistance < maxDistance) {
				maxDistance = currDistance;
				hitCollider = hitInfo[i].collider;
			}
		}
		if (hitCollider == null) {
			Debug.LogError("HIT NOTHING!");
		} else {
			// TODO(Julian): Make this more robust
			hitCollider.gameObject.transform.parent.RotateAround(pivot, Vector3.up, direction * 90); // TODO(JULIAN): make rotation gradual
		}
	}



	void LateUpdate () {
		for (int i = -10; i < 10; i++) {
			for (int j = -10; j < 10; j++) {
				Vector3 gridline = new Vector3(i*_pivotGridPrecision,0,j*_pivotGridPrecision);
				Debug.DrawLine(gridline, gridline + new Vector3(0,10,0));
			}
		}

			_transform.position = new Vector3(_playerxyTransform.position.x,
											YPosition,
											-_playerzyTransform.position.x);
		if (DoesExist) {
			_renderer.enabled = true;
		} else {
			_renderer.enabled = false;
		}
	}

}
