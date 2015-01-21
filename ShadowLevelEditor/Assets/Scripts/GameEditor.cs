using UnityEngine;
using System.Collections;

public class GameEditor : MonoBehaviour {

	[SerializeField]
	GameObject _selectedObject;
	[SerializeField]
	float _precision = 0.5f;
	[SerializeField]
	Camera[] _cameras;

	private bool _isMoving = false;
	private Vector2 _mouseDelta;


	private Vector2 _lastMousePos;

	private Vector3 _sourcePosition;

	private Camera _relativeCam;

	private Vector3 SnapToRounded (Vector3 point, float precision) {
		Vector3 result = point;
		result.x = Mathf.Round(point.x / precision) * precision;
		result.y = Mathf.Round(point.y / precision) * precision;
		result.z = Mathf.Round(point.z / precision) * precision;
		return result;
	}

	private void StartGrabbing (Camera camera) {
		if (camera != null) {
			_relativeCam = camera;
			_isMoving = true;

			_mouseDelta = Vector2.zero;
			_lastMousePos = Input.mousePosition;

			_sourcePosition = _selectedObject.transform.position;
		}
	}

	// Update is called once per frame
	void Update () {

		if (_isMoving) {
			Vector3 lastRelativePoint = _relativeCam.ScreenToViewportPoint(_lastMousePos);
			Vector3 relativePoint = _relativeCam.ScreenToViewportPoint(Input.mousePosition);
			_mouseDelta += (Vector2)(relativePoint - lastRelativePoint);

			// TODO(Julian): Fix this
			Vector3 p = _relativeCam.ViewportToWorldPoint(_relativeCam.WorldToViewportPoint(_sourcePosition) + (Vector3)_mouseDelta);
			_selectedObject.transform.position = SnapToRounded(p, _precision);

			if (Input.GetKeyDown(KeyCode.Mouse0)) {
				_isMoving = false;
			}

			if (Input.GetKeyDown(KeyCode.Escape)) {
				_isMoving = false;
				_selectedObject.transform.position = _sourcePosition;
			}

			_lastMousePos = Input.mousePosition;
			return;
		}

		Camera foundCamera = null;
		for (int i = 0; i < _cameras.Length; i++) {
			Vector3 testPoint = _cameras[i].ScreenToViewportPoint(Input.mousePosition);
			if (testPoint.x >= 0f && testPoint.x <= 1f &&
				testPoint.y >= 0f && testPoint.y <= 1f) {
				// Within bounds
				foundCamera = _cameras[i];
				break;
			}
		}

		if (foundCamera == null) {
			return;
		}

		if (Input.GetKeyDown(KeyCode.G)) {
			StartGrabbing(foundCamera);
			return;
		}

		if (!_isMoving && (Input.GetKey(KeyCode.LeftShift) ||
			 Input.GetKey(KeyCode.RightShift)) && Input.GetKeyDown(KeyCode.D)) {
			_selectedObject = Instantiate(_selectedObject, _selectedObject.transform.position, _selectedObject.transform.rotation) as GameObject;
			StartGrabbing(foundCamera);
			return;
		}

		if(!_isMoving && Input.GetKeyDown(KeyCode.Mouse0)) {
			Vector3 testPoint = foundCamera.ScreenToViewportPoint(Input.mousePosition);
			var testRay = foundCamera.ViewportPointToRay(testPoint);
			RaycastHit hit;
			if (Physics.Raycast(testRay, out hit)) {
				var t = hit.transform;
				do {
					if(t.gameObject.GetComponent<EditorBlock>() != null) {
						_selectedObject = t.gameObject;
						break;
					}
					t = t.parent;
				} while (t != null);

	            Debug.DrawLine(testRay.origin, hit.point);
			}
			return;
		}

	}
}
