using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ShadowRenderer : MonoBehaviour {

    MeshFilter _meshFilter;
	MeshRenderer _meshRenderer;

	void Awake () {
        // Create the mesh
        Mesh mesh = new Mesh();
        mesh.MarkDynamic();
        // Set up game object with mesh;
        _meshRenderer = gameObject.GetComponent<MeshRenderer>();
        if (_meshRenderer == null) {
            _meshRenderer = gameObject.AddComponent<MeshRenderer>();
        }
        _meshFilter = gameObject.GetComponent<MeshFilter>();
        if (_meshFilter == null) {
            _meshFilter = gameObject.AddComponent<MeshFilter>();
        }
        _meshFilter.mesh = mesh;
	}

    public void SetMaterial(Material material) {
        _meshRenderer.material = material;
    }


    Vector3[] _vertices;
    Vector3[] _normals;
    Vector2[] _uvs;
	public void SetVerts(List<Vector2> vertices2D, Plane plane) {
        transform.rotation = Quaternion.identity;
		List<Vector2> toTriangulate = new List<Vector2>();
        HashSet<Vector2> duplicates = new HashSet<Vector2>();
        for (int i=vertices2D.Count-1; i>0; i--) {
        	Vector3 toInsert = vertices2D[i];
        	if (duplicates.Contains(toInsert)) {
        		continue; // Don't allow dups
        	}
			duplicates.Add(toInsert);
            toTriangulate.Add(toInsert);
        }

        // Use the triangulator to get indices for creating triangles
        Triangulator tr = new Triangulator(toTriangulate.ToArray());
        int[] indices = tr.Triangulate();

        // Create the Vector3 vertices
        if (_vertices == null || _vertices.Length != vertices2D.Count) {
            _vertices = new Vector3[vertices2D.Count];
            _normals = new Vector3[vertices2D.Count];
            _uvs = new Vector2[vertices2D.Count];
        }
        for (int i=0; i<toTriangulate.Count; i++) {
        	_normals[i] = plane.Normal;
            _vertices[i] = ProjectionMath.ThreeDimCoordsOnPlane(new Vector2(-toTriangulate[i].x, toTriangulate[i].y), plane) + plane.Normal*0.1f;
            // TODO(Julian): set the uvs if we need shadow textures
        }
        Mesh mesh = _meshFilter.mesh;
        mesh.vertices = _vertices;
        mesh.triangles = indices;
        mesh.normals = _normals;
        mesh.uv = _uvs;
        mesh.RecalculateBounds();
	}
}
