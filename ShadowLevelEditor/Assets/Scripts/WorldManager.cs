using UnityEngine;
using System.Collections;

public class WorldManager : MonoBehaviour {

	public static WorldManager g;

	[SerializeField]
	Plane[] _planes;

	public Plane[] Planes {
		get { return _planes; }
	}

	void Awake () {
		if (g == null) {
			g = this;
		} else {
			Destroy(this);
		}
	}

}
