using UnityEngine;
using System.Collections;
using Shadow.Extensions.Interface;


public class GameEditor : MonoBehaviour {

	IEditorBlock _selectedEditorBlock;
	[SerializeField]
	float _precision = 0.5f;
	[SerializeField]
	Camera[] _cameras;
	[SerializeField]
	GameObject[] _spawnablePrefabs;

	private bool _isMoving = false;
	private bool _killObjectOnAbort = false;
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

			_sourcePosition = _selectedEditorBlock.Position;
		}
	}

	// Update is called once per frame
	void Update () {

		if (_isMoving) {
			Vector3 lastRelativePoint = _relativeCam.ScreenToViewportPoint(_lastMousePos);
			Vector3 relativePoint = _relativeCam.ScreenToViewportPoint(Input.mousePosition);
			_mouseDelta += (Vector2)(relativePoint - lastRelativePoint);

			Vector3 p = _relativeCam.ViewportToWorldPoint(_relativeCam.WorldToViewportPoint(_sourcePosition) + (Vector3)_mouseDelta);
			_selectedEditorBlock.Position = SnapToRounded(p, _precision);

			if (Input.GetKeyDown(KeyCode.Mouse0)) {
				_isMoving = false;
			}

			if (Input.GetKeyDown(KeyCode.Escape)) {
				if (_killObjectOnAbort) {
					Destroy(_selectedEditorBlock.GO);
					_selectedEditorBlock = null;
					_killObjectOnAbort = false;
				} else {
					_selectedEditorBlock.Position = _sourcePosition;
				}
				_isMoving = false;
			}


			for (int i = 0; i < _spawnablePrefabs.Length; i++) {
				if (Input.GetKeyDown(KeyCode.Alpha1 + i)) {
					var pos = _selectedEditorBlock.Position;
					var rot = _selectedEditorBlock.Rotation;
					Destroy(_selectedEditorBlock.GO);
					_selectedEditorBlock = (Instantiate(_spawnablePrefabs[i], pos, rot) as GameObject).GetInterface<IEditorBlock>();

					// TODO(Julian): Make Abort work Properly!

					StartGrabbing(_relativeCam);
					break;
				}
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

		if ((Input.GetKey(KeyCode.LeftShift) ||
			 Input.GetKey(KeyCode.RightShift)) && Input.GetKeyDown(KeyCode.D)) {
			_selectedEditorBlock = (Instantiate(_selectedEditorBlock.GO, _selectedEditorBlock.Position, _selectedEditorBlock.Rotation) as GameObject).GetInterface<IEditorBlock>();
			_killObjectOnAbort = true;
			StartGrabbing(foundCamera);
			return;
		}


		for (int i = 0; i < _spawnablePrefabs.Length; i++) {
			if (Input.GetKeyDown(KeyCode.Alpha1 + i)) {
				Vector3 p = foundCamera.ViewportToWorldPoint(foundCamera.ScreenToViewportPoint(Input.mousePosition));
				p += foundCamera.transform.forward*4f;
				_selectedEditorBlock = (Instantiate(_spawnablePrefabs[i], p, _spawnablePrefabs[i].transform.rotation) as GameObject).GetInterface<IEditorBlock>();
				_killObjectOnAbort = true;
				StartGrabbing(foundCamera);
				return;
			}
		}

		if(Input.GetKeyDown(KeyCode.Mouse0)) {
			Vector3 testPoint = foundCamera.ScreenToViewportPoint(Input.mousePosition);
			var testRay = foundCamera.ViewportPointToRay(testPoint);
			RaycastHit hit;
			if (Physics.Raycast(testRay, out hit)) {
				var t = hit.transform;
				do {
					IEditorBlock editorBlock = t.gameObject.GetInterface<IEditorBlock>();
					if(editorBlock != null) {
						_selectedEditorBlock = editorBlock;
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
