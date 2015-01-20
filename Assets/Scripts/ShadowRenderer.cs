using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ShadowRenderer : MonoBehaviour {

	MeshFilter _meshFilter;

	void Awake () {
        // Create the mesh
        Mesh mesh = new Mesh();
        mesh.MarkDynamic();
        // Set up game object with mesh;
        gameObject.AddComponent<MeshRenderer>();
        _meshFilter = gameObject.AddComponent<MeshFilter>();
        _meshFilter.mesh = mesh;
	}

	public void SetVerts(List<Vector2> vertices2D, Plane plane) {
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
        Vector3[] vertices = new Vector3[vertices2D.Count];
        Vector3[] normals = new Vector3[vertices2D.Count];
        for (int i=0; i<toTriangulate.Count; i++) {
        	normals[i] = plane.Normal;
            vertices[i] = ProjectionMath.ThreeDimCoordsOnPlane(new Vector2(-toTriangulate[i].x, toTriangulate[i].y), plane) + plane.Normal*0.1f;
        }
        Mesh mesh = _meshFilter.mesh;
        mesh.vertices = vertices;
        mesh.triangles = indices;
        mesh.normals = normals;
        mesh.RecalculateBounds();
	}
}
