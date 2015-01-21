using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using ClipperLib;

namespace Projection {
  using Path = List<IntPoint>;
  using Paths = List<List<IntPoint>>;

public class Project : MonoBehaviour {

	public Vector2 __pos;

	private Plane[] _planes;
	private EdgeCollider2D[] _colliders;
	private ShadowRenderer[] _shadowRenderers;
	private Transform[] _colliderTransforms;
	private Vector2[][] _colliderPoints;
	private int _pointsPerCollider = 100;

	private Mesh _mesh;
	private Transform _transform;
	[SerializeField]
	private Transform _parentTransform;
	[SerializeField]
	private Material _shadowMaterial;
	private Clipper _clipper;


	void Awake () {
		_clipper = new Clipper();
		_mesh = GetComponent<MeshFilter>().mesh;
		_transform = GetComponent<Transform>();
	}

	void Start () {
		_planes = WorldManager.g.Planes;
		_colliders = new EdgeCollider2D[_planes.Length];
		_shadowRenderers = new ShadowRenderer[_planes.Length];
		_colliderTransforms = new Transform[_planes.Length];
		_colliderPoints = new Vector2[_planes.Length][];
		for (int i = 0; i < _planes.Length; i++) {
			GameObject colliderObject = new GameObject(_planes[i].gameObject.name + " Collider");
			colliderObject.layer = _planes[i].Layer;
			colliderObject.transform.parent = _parentTransform;
			_colliderTransforms[i] = colliderObject.transform;
			_colliders[i] = colliderObject.AddComponent<EdgeCollider2D>();
			_shadowRenderers[i] = colliderObject.AddComponent<ShadowRenderer>();
			_shadowRenderers[i].SetMaterial(_shadowMaterial);
			_colliderPoints[i] = new Vector2[_pointsPerCollider];
			_colliders[i].points = _colliderPoints[i];
		}
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
		return ProjectionMath.ProjectPointToPlane(VertexPosToWorldPos(vert), plane);
	}

	Path PathFromVerts(Vector3 v1, Vector3 v2, Vector3 v3, Plane plane, float precision) {
		Path p = new Path(3);
		v1 = ProjectionMath.TwoDimCoordsOnPlane(v1, plane);
		v2 = ProjectionMath.TwoDimCoordsOnPlane(v2, plane);
		v3 = ProjectionMath.TwoDimCoordsOnPlane(v3, plane);
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
		return ProjectionMath.ThreeDimCoordsOnPlane(v, plane);
	}

	Vector3 Vector2FromIntPoint(IntPoint ip, Plane plane, float precision) {
		return new Vector2(ip.X,ip.Y)/precision;
	}


	void Update() {
		float precision = 10000f;
		int[] triangles = _mesh.triangles;
		int triangleCount = triangles.Length;
		Vector3[] vertices = _mesh.vertices;
		int vertexCount = vertices.Length;

		for (int planeIndex = 0; planeIndex < _planes.Length; planeIndex++) {
			Paths subj = new Paths(triangleCount/3); // One subject path per tri

			Vector3[] projectedVertices3d = new Vector3[vertexCount];

			Plane plane = _planes[planeIndex];
			for (int unprojectedVertIndex = 0; unprojectedVertIndex < vertexCount; unprojectedVertIndex++) {
				Vector3 projected3d = ProjectVertIndexToPlane(unprojectedVertIndex, plane);
				projectedVertices3d[unprojectedVertIndex] = projected3d;
			}


			for (int i = 0; i < triangleCount/3; i++) {
				Vector2 v1 = ProjectionMath.TwoDimCoordsOnPlane(projectedVertices3d[triangles[i*3]], plane);
				Vector2 v2 = ProjectionMath.TwoDimCoordsOnPlane(projectedVertices3d[triangles[i*3+1]], plane);
				Vector2 v3 = ProjectionMath.TwoDimCoordsOnPlane(projectedVertices3d[triangles[i*3+2]], plane);

				if (Vector3.Dot(Vector3.Cross((Vector3)(v1 - v2), (Vector3)(v1 - v3)), plane.Normal) > 0f) {
					Vector2 temp = v2;
					v2 = v3;
					v3 = temp;
				}

				Path p = new Path(4);

				p.Add(IntPointFromVector(v1, precision));
				p.Add(IntPointFromVector(v2, precision));
				p.Add(IntPointFromVector(v3, precision));
				p.Add(IntPointFromVector(v1, precision));
				subj.Add(p);
			}

			float width = 10f;
			float height = 10f;
			Paths clip = new Paths(1);
			clip.Add(new Path(4));
			Vector2 c1 = ProjectionMath.TwoDimCoordsOnPlane(plane.Origin + plane.Up * height/2f - plane.Right * width/2f, plane);
			Vector2 c2 = ProjectionMath.TwoDimCoordsOnPlane(plane.Origin + plane.Up * height/2f + plane.Right * width/2f, plane);
			Vector2 c3 = ProjectionMath.TwoDimCoordsOnPlane(plane.Origin - plane.Up * height/2f + plane.Right * width/2f, plane);
			Vector2 c4 = ProjectionMath.TwoDimCoordsOnPlane(plane.Origin - plane.Up * height/2f - plane.Right * width/2f, plane);

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

			_clipper.Clear();
			_clipper.AddPaths(subj, PolyType.ptSubject, true);
			_clipper.AddPaths(clip, PolyType.ptClip, true);
			_clipper.Execute(ClipType.ctIntersection, solution,
				subjFillType: PolyFillType.pftNonZero,
				clipFillType: PolyFillType.pftNonZero);

			solution = Clipper.SimplifyPolygons(solution, PolyFillType.pftNonZero);

			//pftEvenOdd, pftNonZero, pftPositive, pftNegative

			List<Vector3> concaveHullVerts = new List<Vector3>();
			for (int i = 0; i < solution.Count; i++) {
				for (int j = 0; j < solution[i].Count; j++) {
					concaveHullVerts.Add(Vector3FromIntPoint(solution[i][j], plane, precision));
				}
				concaveHullVerts.Add(concaveHullVerts[concaveHullVerts.Count - solution[i].Count]);
			}
			// concaveHullVerts.Add(Vector3FromIntPoint(solution[0][0], plane, precision));

			List<Vector2> triangulatorVerts = new List<Vector2>();

			int concaveHullVertsIndex;
			_colliderTransforms[planeIndex].rotation = Quaternion.identity;
			_colliderTransforms[planeIndex].position = Vector3.zero;
			for (concaveHullVertsIndex = 0; concaveHullVertsIndex < concaveHullVerts.Count; concaveHullVertsIndex++) {
				Vector2 pt = ProjectionMath.TwoDimCoordsOnPlane(concaveHullVerts[concaveHullVertsIndex], plane);
				// pt -= TwoDimCoordsOnPlane(_colliderTransforms[planeIndex].position, plane);
				pt.x *= -1f;
				_colliderPoints[planeIndex][concaveHullVertsIndex] = pt;

				triangulatorVerts.Add(pt);
			}
			for (concaveHullVertsIndex = concaveHullVerts.Count; concaveHullVertsIndex < _pointsPerCollider; concaveHullVertsIndex++) {
				_colliderPoints[planeIndex][concaveHullVertsIndex] = _colliderPoints[planeIndex][0];
				triangulatorVerts.Add(_colliderPoints[planeIndex][0]);
			}
			_colliders[planeIndex].points = _colliderPoints[planeIndex];

			_shadowRenderers[planeIndex].SetVerts(triangulatorVerts, plane);



			for (int i = 0; i < triangleCount; i+=3) {
				Vector3 vert1 = projectedVertices3d[triangles[i]];
				Vector3 vert2 = projectedVertices3d[triangles[i+1]];
				Vector3 vert3 = projectedVertices3d[triangles[i+2]];
				Debug.DrawLine(vert1, vert2);
				Debug.DrawLine(vert2, vert3);
				Debug.DrawLine(vert3, vert1);
			}

			for (int i = 1; i < concaveHullVerts.Count; i++) {
				Debug.DrawLine(concaveHullVerts[i-1], concaveHullVerts[i], Color.red);
			}
		}
	}
}
}