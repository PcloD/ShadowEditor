using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using ClipperLib;

namespace Projection {
  using Path = List<IntPoint>;
  using Paths = List<List<IntPoint>>;

public class Project2 : MonoBehaviour {

	public Vector2 __pos;

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
		// Debug.Log(__res);

		// string __res = "{\n";
		for (int i = 0; i < _adjacencyDictionary.Length; i++) {
			__res += i + ": [";
			for (int j = 0; j < _adjacencyDictionary[i].Length; j++) {
				__res += ((j < _adjacencyDictionary[i].Length-1)?_adjacencyDictionary[i][j] + ", ":_adjacencyDictionary[i][j] + "");
			}
			__res += ((i < _adjacencyDictionary.Length-1)?"],\n":"]\n");
		}
		__res += "}";
		// Debug.Log(__res);

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

	private Vector3 ThreeDimCoordsOnPlane (Vector2 point, Plane plane) {
		return (point.x * plane.Right) + (point.y * plane.Up) + plane.Origin;
	}

	private float AngleBetween(Vector2 a, Vector2 b) {
		float dot = a.x*b.x + a.y*b.y;      // dot product
		float det = a.x*b.y - a.y*b.x;      // determinant
		return Mathf.Atan2(det, dot);  // atan2(y, x) or atan2(sin, cos)
	}

	void Update () {

		// for (int j = 0; j < _planes.Length; j++) {
		Plane plane = _planes[0];
		__pos = TwoDimCoordsOnPlane(_transform.position, plane);
		// }
	}


	Path PathFromVerts(Vector3 v1, Vector3 v2, Vector3 v3, Plane plane, float precision) {
		Path p = new Path(3);
		v1 = TwoDimCoordsOnPlane(v1, plane);
		v2 = TwoDimCoordsOnPlane(v2, plane);
		v3 = TwoDimCoordsOnPlane(v3, plane);
		p.Add(IntPointFromVector(v1, precision));
		p.Add(IntPointFromVector(v2, precision));
		p.Add(IntPointFromVector(v3, precision));
		// p.Add(IntPointFromVector(v1, precision));

		// string __res = "";
		// __res += "["+IntPointFromVector(v1, precision).X +","+IntPointFromVector(v1, precision).Y + ", ";
		// __res += " "+IntPointFromVector(v2, precision).X +","+IntPointFromVector(v2, precision).Y + ", ";
		// __res += " "+IntPointFromVector(v3, precision).X +","+IntPointFromVector(v3, precision).Y + "], ";
		// Debug.Log(__res);

		return p;
	}

	IntPoint IntPointFromVector(Vector2 vector, float precision) {
		Vector2 v = vector * precision;
		return new IntPoint((int)v.x, (int)v.y);
	}

	Vector3 Vector3FromIntPoint(IntPoint ip, Plane plane, float precision) {
		Vector2 v = new Vector2(ip.X,ip.Y)/precision;
		return ThreeDimCoordsOnPlane(v, plane);
	}

	Vector3 Vector2FromIntPoint(IntPoint ip, Plane plane, float precision) {
		return new Vector2(ip.X,ip.Y)/precision;
	}

	void OnDrawGizmos () {
		if (_mesh == null) return;
		float precision = 10000f;
		int[] triangles = _mesh.triangles;
		int triangleCount = triangles.Length;
		Vector3[] vertices = _mesh.vertices;
		int vertexCount = vertices.Length;



		for (int planeIndex = 0; planeIndex < _planes.Length; planeIndex++) {
			Paths subj = new Paths(triangleCount/3); // One subject path per tri
			// Path poly = new Path(vertexCount);
			// Paths subj = new Paths(1); // One subject path per tri

			Vector3[] projectedVertices3d = new Vector3[vertexCount];

			Plane plane = _planes[planeIndex];
			for (int unprojectedVertIndex = 0; unprojectedVertIndex < vertexCount; unprojectedVertIndex++) {
				Vector3 projected3d = ProjectVertIndexToPlane(unprojectedVertIndex, plane);
				projectedVertices3d[unprojectedVertIndex] = projected3d;
			}

			string __res = "";

			// for (int i = 0; i < 1;i++){//triangleCount/3; i++) {
			for (int i = 0; i < triangleCount/3; i++) {
				Vector2 v1 = TwoDimCoordsOnPlane(projectedVertices3d[triangles[i*3]], plane);
				Vector2 v2 = TwoDimCoordsOnPlane(projectedVertices3d[triangles[i*3+1]], plane);
				Vector2 v3 = TwoDimCoordsOnPlane(projectedVertices3d[triangles[i*3+2]], plane);

				if (Vector3.Dot(Vector3.Cross((Vector3)(v1 - v2), (Vector3)(v1 - v3)), plane.Normal) > 0f) {
					Vector2 temp = v2;
					v2 = v3;
					v3 = v2;
				}

				// poly.Add(IntPointFromVector(v1, precision));
				// poly.Add(IntPointFromVector(v2, precision));
				// poly.Add(IntPointFromVector(v3, precision));

				__res += "["+IntPointFromVector(v1, precision).X +","+IntPointFromVector(v1, precision).Y + ", ";
				__res += IntPointFromVector(v2, precision).X +","+IntPointFromVector(v2, precision).Y +", ";
				__res += IntPointFromVector(v3, precision).X +","+IntPointFromVector(v3, precision).Y +", ";
				__res += IntPointFromVector(v1, precision).X +","+IntPointFromVector(v1, precision).Y +"], ";

				// Debug.DrawLine(Vector3FromIntPoint(IntPointFromVector(v1, precision), plane, precision),Vector3FromIntPoint(IntPointFromVector(v2, precision), plane, precision),Color.green);
				// Debug.DrawLine(Vector3FromIntPoint(IntPointFromVector(v2, precision), plane, precision),Vector3FromIntPoint(IntPointFromVector(v3, precision), plane, precision),Color.green);
				// Debug.DrawLine(Vector3FromIntPoint(IntPointFromVector(v3, precision), plane, precision),Vector3FromIntPoint(IntPointFromVector(v1, precision), plane, precision),Color.green);
				Path p = new Path(4);
				// subj.Add (PathFromVerts(projectedVertices3d[triangles[i*3]],
				// 	                    projectedVertices3d[triangles[i*3+1]],
				// 	                    projectedVertices3d[triangles[i*3+2]],
				// 	                    plane,
				// 	                    precision));
				p.Add(IntPointFromVector(v1, precision));
				p.Add(IntPointFromVector(v2, precision));
				p.Add(IntPointFromVector(v3, precision));
				p.Add(IntPointFromVector(v1, precision));
				subj.Add(p);
				// Debug.DrawLine(Vector3FromIntPoint(subj[i][0], plane, precision),Vector3FromIntPoint(subj[i][1], plane, precision),Color.green);
				// Debug.DrawLine(Vector3FromIntPoint(subj[i][1], plane, precision),Vector3FromIntPoint(subj[i][2], plane, precision),Color.green);
				// Debug.DrawLine(Vector3FromIntPoint(subj[i][2], plane, precision),Vector3FromIntPoint(subj[i][0], plane, precision),Color.green);
			}
			Debug.Log(__res);

			float width = 10f;
			float height = 10f;
			Paths clip = new Paths(1);
			clip.Add(new Path(4));
			Vector2 c1 = TwoDimCoordsOnPlane(plane.Origin + plane.Up * height/2f - plane.Right * width/2f, plane);
			Vector2 c2 = TwoDimCoordsOnPlane(plane.Origin + plane.Up * height/2f + plane.Right * width/2f, plane);
			Vector2 c3 = TwoDimCoordsOnPlane(plane.Origin - plane.Up * height/2f + plane.Right * width/2f, plane);
			Vector2 c4 = TwoDimCoordsOnPlane(plane.Origin - plane.Up * height/2f - plane.Right * width/2f, plane);

			IntPoint cip1 = IntPointFromVector(c1, precision);
			IntPoint cip2 = IntPointFromVector(c2, precision);
			IntPoint cip3 = IntPointFromVector(c3, precision);
			IntPoint cip4 = IntPointFromVector(c4, precision);

			clip[0].Add(cip1);
			clip[0].Add(cip2);
			clip[0].Add(cip3);
			clip[0].Add(cip4);

	Debug.DrawLine(Vector3FromIntPoint(cip1, plane, precision),Vector3FromIntPoint(cip2, plane, precision),Color.blue);
	Debug.DrawLine(Vector3FromIntPoint(cip2, plane, precision),Vector3FromIntPoint(cip3, plane, precision),Color.blue);
	Debug.DrawLine(Vector3FromIntPoint(cip3, plane, precision),Vector3FromIntPoint(cip4, plane, precision),Color.blue);
	Debug.DrawLine(Vector3FromIntPoint(cip4, plane, precision),Vector3FromIntPoint(cip1, plane, precision),Color.blue);

			Paths solution = new Paths();

			// Paths solution = Clipper.SimplifyPolygons(subj, PolyFillType.pftNonZero);

			Clipper c = new Clipper();
			c.AddPaths(subj, PolyType.ptSubject, true);
			c.AddPaths(clip, PolyType.ptClip, true);
			c.Execute(ClipType.ctIntersection, solution,
	  		          subjFillType: PolyFillType.pftNonZero,
	  		          clipFillType: PolyFillType.pftNonZero);

			//pftEvenOdd, pftNonZero, pftPositive, pftNegative

			List<Vector3> concaveHullVerts = new List<Vector3>();
			int cnt = 0;
			for (int i = 0; i < solution.Count; i++) {
				for (int j = 0; j < solution[i].Count; j++) {
					cnt++;
					concaveHullVerts.Add(Vector3FromIntPoint(solution[i][j], plane, precision));
				}
			}

			// concaveHullVerts.Add(Vector3FromIntPoint(solution[0][0], plane, precision));

			// if (cnt > 0) {
			// 	Debug.Log(cnt);
			// }

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
			for (int i = 1; i < concaveHullVerts.Count; i++) {
				Gizmos.DrawLine(concaveHullVerts[i-1], concaveHullVerts[i]);
			}

		}
		// Debug.Log(__res);
		// Debug.Log("------------------------");
	}
}
}