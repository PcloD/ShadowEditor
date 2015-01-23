using UnityEngine;
using System.Collections;

public class EditorBlock : MonoBehaviour, IEditorBlock {

	[SerializeField]
	string _typeName;

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

	public string TypeName {
		get { return _typeName; }
	}

}
