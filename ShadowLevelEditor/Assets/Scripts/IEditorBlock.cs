using UnityEngine;
using System.Collections;

public interface IEditorBlock {
	Vector3 Position {
		set;
		get;
	}

	Quaternion Rotation {
		set;
		get;
	}

	GameObject GO {
		get;
	}

 	string TypeName {
		get;
	}
}