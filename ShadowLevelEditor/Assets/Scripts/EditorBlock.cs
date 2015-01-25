using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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

	public Vector3[] SelectionVerts {
		get {
			MeshFilter[] allFilters = GetComponentsInChildren<MeshFilter>();
			List<Vector3> verts = new List<Vector3>();
			for (int i = 0; i < allFilters.Length; i++) {
				Mesh mesh = allFilters[i].mesh;
				if (allFilters[i].gameObject.GetComponent<ShadowRenderer>() != null) {
					for (int j = 0; j < mesh.vertices.Length; j++) {
						verts.Add(mesh.vertices[j]);
					}
				} else {
					for (int j = 0; j < mesh.vertices.Length; j++) {
						Vector3 worldPos = mesh.vertices[j];
						worldPos = new Vector3(transform.localScale.x * worldPos.x,
											   transform.localScale.y * worldPos.y,
											   transform.localScale.z * worldPos.z);
						worldPos = transform.rotation * worldPos;
						worldPos += transform.position;

						verts.Add(worldPos);
					}
				}
			}
			return verts.ToArray();
		}
	}

	public Vector3[] SelectionVertNormals {
		get {
			MeshFilter[] allFilters = GetComponentsInChildren<MeshFilter>();
			List<Vector3> verts = new List<Vector3>();
			for (int i = 0; i < allFilters.Length; i++) {
				Mesh mesh = allFilters[i].mesh;
				if (allFilters[i].gameObject.GetComponent<ShadowRenderer>() != null) {
					for (int j = 0; j < mesh.normals.Length; j++) {
						verts.Add(mesh.normals[j]);
					}
				} else {
					for (int j = 0; j < mesh.normals.Length; j++) {
						Vector3 worldPos = mesh.normals[j];
						worldPos = transform.rotation * worldPos;

						verts.Add(worldPos);
					}
				}
			}
			return verts.ToArray();
		}
	}

}
