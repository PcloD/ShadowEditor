using UnityEngine;
using System.Collections;

public class EditorBlock : MonoBehaviour, IEditorBlock {

	public Vector3 Position {
		set { transform.position = value; }
		get { return transform.position; }
	}

	public Quaternion Rotation {
		set { transform.rotation = value; }
		get { return transform.rotation; }
	}

	public GameObject GO {
		get { return gameObject; }
	}

}
