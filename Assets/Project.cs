using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Project : MonoBehaviour {
	[SerializeField]
	private Plane[] _planes;

	private Mesh _mesh;
	private Transform _transform;
	private int[][] _adjacencyDictionary;

	void Awake () {
		_mesh = GetComponent<MeshFilter>().mesh;
		_transform = GetComponent<Transform>();
		CacheAdjacencies();

		string __res = "";
		int vertexCount = _mesh.vertices.Length;
		for (int i = 0; i < vertexCount; i++) {

			Vector3 worldPos = _mesh.vertices[i];
			worldPos = new Vector3(_transform.localScale.x * worldPos.x, _transform.localScale.y * worldPos.y, _transform.localScale.z * worldPos.z);
			worldPos = _transform.rotation * worldPos;
			worldPos += _transform.position;

			__res+=(i + ":" + worldPos.x
				+ "," + worldPos.y
				+ "," + worldPos.z + "\n");
		}
		Debug.Log(__res);

		// string __res = "{\n";
		for (int i = 0; i < _adjacencyDictionary.Length; i++) {
			__res += i + ": [";
			for (int j = 0; j < _adjacencyDictionary[i].Length; j++) {
				__res += ((j < _adjacencyDictionary[i].Length-1)?_adjacencyDictionary[i][j] + ", ":_adjacencyDictionary[i][j] + "");
			}
			__res += ((i < _adjacencyDictionary.Length-1)?"],\n":"]\n");
		}
		__res += "}";
		Debug.Log(__res);

		// __res = "";
		// for (int i = 0; i < _adjacencyDictionary.Length; i++) {
		// 	__res += i + "\t";
		// 	for (int j = 0; j < _adjacencyDictionary[i].Length; j++) {
		// 		__res += ((j < _adjacencyDictionary[i].Length-1)?_adjacencyDictionary[i][j] + ";":_adjacencyDictionary[i][j] + "");
		// 	}
		// 	__res += "\n";
		// }
		// Debug.Log(__res);
	}

	// Note that the adjacencies may not appear correct
	// for meshes with sharp edges. In such cases,
	// the mesh will have unconnected, overlapping vertices.
	// To fix, we must weld overlapping verts together
	private void CacheAdjacencies () {
		int[] triangles = _mesh.triangles;
		int triangleCount = triangles.Length;
		int vertexCount = _mesh.vertices.Length;
		Debug.Log("VERTS: "+vertexCount);
		HashSet<int>[] dynAdjacencyDictionary = new HashSet<int>[vertexCount];
		_adjacencyDictionary = new int[vertexCount][];
		for (int i = 0; i < vertexCount; i++) {
			dynAdjacencyDictionary[i] = new HashSet<int>();
		}
		for (int i = 0; i < triangleCount; i+=3) {
			int vertIndex0 = triangles[i];
			int vertIndex1 = triangles[i+1];
			int vertIndex2 = triangles[i+2];
			dynAdjacencyDictionary[vertIndex0].Add(vertIndex1);
			dynAdjacencyDictionary[vertIndex0].Add(vertIndex2);

			dynAdjacencyDictionary[vertIndex1].Add(vertIndex0);
			dynAdjacencyDictionary[vertIndex1].Add(vertIndex2);

			dynAdjacencyDictionary[vertIndex2].Add(vertIndex0);
			dynAdjacencyDictionary[vertIndex2].Add(vertIndex1);
		}
		for (int i = 0; i < vertexCount; i++) {
			int[] verts = new int[dynAdjacencyDictionary[i].Count];
			dynAdjacencyDictionary[i].CopyTo(verts);
			_adjacencyDictionary[i] = verts;
		}
	}

	private Vector3 ProjectPointToPlane (Vector3 point, Plane plane) {
		Vector3 originToPoint = point - plane.Origin;
		float dist = Vector3.Dot(originToPoint, plane.Normal);
		return point - dist * plane.Normal;
	}

	private Vector3 VertexPosToWorldPos (Vector3 vertexPos) {
		Vector3 worldPos = vertexPos;
		worldPos = new Vector3(_transform.localScale.x * worldPos.x,
							   _transform.localScale.y * worldPos.y,
							   _transform.localScale.z * worldPos.z);
		worldPos = _transform.rotation * worldPos;
		worldPos += _transform.position;
		return worldPos;
	}

	private Vector3 ProjectVertIndexToPlane (int index, Plane plane) {
		Vector3 vert = _mesh.vertices[index];
		return ProjectPointToPlane(VertexPosToWorldPos(vert), plane);
	}

	private Vector2 TwoDimCoordsOnPlane (Vector3 point, Plane plane) {
		return new Vector2(Vector3.Dot(point - plane.Origin, plane.Right), Vector3.Dot(point - plane.Origin, plane.Up));
	}

	// private float SignedAngle (Vector3 a, Vector3 b, Vector3 normal) {
	//     return Mathf.Atan2(Vector3.Dot(normal, Vector3.Cross(a, b)), Vector3.Dot(a, b));
	// }

	private float AngleBetween(Vector2 a, Vector2 b) {
		float dot = a.x*b.x + a.y*b.y;      // dot product
		float det = a.x*b.y - a.y*b.x;      // determinant
		return Mathf.Atan2(det, dot);  // atan2(y, x) or atan2(sin, cos)
	}

	void OnDrawGizmos () {
		if (_mesh == null) return;
		int[] triangles = _mesh.triangles;
		int triangleCount = triangles.Length;
		Vector3[] vertices = _mesh.vertices;
		int vertexCount = vertices.Length;

		Vector3[] projectedVertices3d = new Vector3[vertexCount];
		int highestVertIndex = -1;

		float bestDotProd = Mathf.Infinity;
		for (int planeIndex = 0; planeIndex < _planes.Length; planeIndex++) {
			Plane plane = _planes[planeIndex];
			for (int unprojectedVertIndex = 0; unprojectedVertIndex < vertexCount; unprojectedVertIndex++) {
				Vector3 projected3d = ProjectVertIndexToPlane(unprojectedVertIndex, plane);
				projectedVertices3d[unprojectedVertIndex] = projected3d;

				float currDotProd = Vector3.Dot(plane.Up, projected3d);
				if (currDotProd < bestDotProd) {
					highestVertIndex = unprojectedVertIndex;
					bestDotProd = currDotProd;
				}
			}


			List<int> concaveHullIndices = new List<int>();
			concaveHullIndices.Add(highestVertIndex); // We know the highest vertex must be part of the convex hull
			Vector3 angleTestVertex = projectedVertices3d[highestVertIndex] - plane.Right;
			int __breakOut__ = 0;
			do {
				float largestAngle = 0.0f;
				int currentWrapIndex = concaveHullIndices[concaveHullIndices.Count - 1];
				int[] possibleVertices = _adjacencyDictionary[currentWrapIndex];
				concaveHullIndices.Add(-1); // Add a placeholder to be replaced with next index
				Vector2 currentWrapVert = TwoDimCoordsOnPlane(projectedVertices3d[currentWrapIndex], plane);
				Vector2 v2 = TwoDimCoordsOnPlane(angleTestVertex, plane) - currentWrapVert;
				for (int connectionIndex = 0; connectionIndex < possibleVertices.Length; connectionIndex++) {
					int testIndex = possibleVertices[connectionIndex];
					Vector2 v1 = TwoDimCoordsOnPlane(projectedVertices3d[testIndex], plane) - currentWrapVert;
					float currentAngle = AngleBetween(v1, v2);
					if (largestAngle < currentAngle) {
						largestAngle = currentAngle;
						concaveHullIndices[concaveHullIndices.Count - 1] = testIndex; // replace placeholder
					}
				}
				angleTestVertex = projectedVertices3d[concaveHullIndices[concaveHullIndices.Count - 2]];
				__breakOut__++; // Prevent infinite loops
			} while (__breakOut__ < 1000 && highestVertIndex != concaveHullIndices[concaveHullIndices.Count - 1]);
			// Debug.Log(__breakOut__);

			Gizmos.color = Color.white;
			for (int i = 0; i < triangleCount; i+=3) {
				Vector3 vert1 = projectedVertices3d[triangles[i]];
				Vector3 vert2 = projectedVertices3d[triangles[i+1]];
				Vector3 vert3 = projectedVertices3d[triangles[i+2]];
				Gizmos.DrawLine(vert1, vert2);
				Gizmos.DrawLine(vert2, vert3);
				Gizmos.DrawLine(vert3, vert1);
			}

			Gizmos.color = Color.red;
			for (int i = 1; i < concaveHullIndices.Count; i++) {
				Gizmos.DrawLine(projectedVertices3d[concaveHullIndices[i-1]], projectedVertices3d[concaveHullIndices[i]]);
			}

		}
		// Debug.Log(__res);
		// Debug.Log("------------------------");
	}
}
