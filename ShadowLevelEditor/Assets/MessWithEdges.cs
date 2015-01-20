using UnityEngine;
using System.Collections;

public class MessWithEdges : MonoBehaviour {

	[SerializeField]
	private float scale = 5f;
	[SerializeField]
	private bool isMoving = true;

	EdgeCollider2D _edgeCollider;
	Vector2[] points;

	float timer = 0f;

	// Use this for initialization
	void Awake () {
		_edgeCollider = GetComponent<EdgeCollider2D>();
		points = _edgeCollider.points;
		points[0] = new Vector2(-10,0);
	}

	// Update is called once per frame
	void FixedUpdate () {
		if (!isMoving) return;
		timer += Time.fixedDeltaTime;
		points[1] = new Vector2(points[1].x, Mathf.Sin(timer) * scale);
		_edgeCollider.points = points;
	}
}
