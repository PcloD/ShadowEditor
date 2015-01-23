using UnityEngine;
using System.Collections;

public class CharacterEditorBlock : MonoBehaviour, IEditorBlock {
	[SerializeField]
	bool xEnabled = true;
	[SerializeField]
	int mapX = 0;
	[SerializeField]
	bool yEnabled = true;
	[SerializeField]
	int mapY = 1;
	[SerializeField]
	bool zEnabled = true;
	[SerializeField]
	int mapZ = 2;

	[SerializeField]
	string _typeName;

	public string TypeName {
		get { return _typeName; }
	}

	public Vector3 Position {
		set {
			Vector3 newPos = new Vector3(
									xEnabled? value[(int)Mathf.Abs(mapX)] * Mathf.Sign(mapX) : transform.position.x,
									yEnabled? value[(int)Mathf.Abs(mapY)] * Mathf.Sign(mapY) : transform.position.y,
									zEnabled? value[(int)Mathf.Abs(mapZ)] * Mathf.Sign(mapZ) : transform.position.z);
			transform.position = newPos;
		}
		get {
			return transform.position;
		}
	}

	public Quaternion Rotation {
		set { Debug.LogError("Rotating Characters isn't possible"); }
		get { return transform.rotation; }
	}

	public GameObject GO {
		get { return gameObject; }
	}
}