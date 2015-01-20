using UnityEngine;
using System.Collections;

public class Character3D : MonoBehaviour {

	[SerializeField]
	Transform _playerxy;
	[SerializeField]
	Transform _playerzy;
	Renderer _renderer;
	[SerializeField]
	float _visibilityThreshold = 0.1f;

	Transform _transform;

	void Awake () {
		_transform = transform;
		_renderer = GetComponent<Renderer>();
	}

	void LateUpdate () {
		if (Mathf.Abs(_playerxy.position.y - _playerzy.position.y) < _visibilityThreshold) {
			_transform.position = new Vector3(_playerxy.position.x,
											(_playerxy.position.y + _playerzy.position.y)/2f,
											_playerzy.position.z);
			_renderer.enabled = true;
		} else {
			_renderer.enabled = false;
		}
	}

}
