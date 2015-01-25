using UnityEngine;
using System.IO;
using System.Collections;
using System.Xml;
using System.Xml.Serialization;
using Shadow.Extensions.Interface;

public class GameEditor : MonoBehaviour {

	IEditorBlock _selectedEditorBlock;
	[SerializeField]
	float _precision = 0.5f;
	[SerializeField]
	Camera[] _cameras;
	[SerializeField]
	GameObject[] _spawnablePrefabs;
	[SerializeField]
	Transform _characterXYTransform;
	[SerializeField]
	Transform _characterZYTransform;

	private float _width;
	private float _height;

	private bool _isMoving = false;
	private bool _killObjectOnAbort = false;
	private Vector2 _mouseDelta;


	private Vector2 _lastMousePos;

	private Vector3 _sourcePosition;

	private Camera _relativeCam;

	private MeshFilter _meshFilter;

	void Awake () {
		_meshFilter = gameObject.AddComponent<MeshFilter>();
		_meshFilter.mesh = new Mesh();
	}

	private Vector3 SnapToRoundedOffest (Vector3 point, float precision) {
		Vector3 result = point;
		result.x = Mathf.Round(point.x / precision) * precision - precision/2f;
		result.y = Mathf.Round(point.y / precision) * precision - precision/2f;
		result.z = Mathf.Round(point.z / precision) * precision - precision/2f;
		return result;
	}

	private Vector3 SnapToRounded (Vector3 point, float precision) {
		Vector3 result = point;
		result.x = Mathf.Round(point.x / precision) * precision;
		result.y = Mathf.Round(point.y / precision) * precision;
		result.z = Mathf.Round(point.z / precision) * precision;
		return result;
	}

	private void StartGrabbing (Camera camera) {
		if (camera != null) {
			_relativeCam = camera;
			_isMoving = true;

			_mouseDelta = Vector2.zero;
			_lastMousePos = Input.mousePosition;

			_sourcePosition = _selectedEditorBlock.Position;
		}
	}

	private void DoRotate90(Camera camera, float direction, float precision) {
		if (camera != null) {
			_relativeCam = camera;

			// Vector3 pivot = SnappingMath.SnapToRoundedOffset (_selectedEditorBlock.Position, precision);
			Vector3 pivot = _selectedEditorBlock.Position;
			// Vector3 originalPos = _selectedEditorBlock.Position;
			_selectedEditorBlock.GO.transform.RotateAround(pivot, -camera.transform.forward, direction * 90); // TODO(JULIAN): make rotation gradual
			// _selectedEditorBlock.Position = SnappingMath.SnapToRounded(originalPos, precision);
			// _selectedEditorBlock.Rotation;
		}
	}

	public struct SavableEditorBlock {
		public string name;
		public string type;
		public Vector3 position;
		public Vector3 rotation;
		public Vector3 scale;
		public Vector3 editorPivot;
	}

	public struct SavableLevel {
		public Vector2 worldDims;
		public Vector2 characterXYPos;
		public Vector2 characterZYPos;
		public SavableEditorBlock[] levelBlocks;
	}

	public void WidthChanged (string newWidth) {
		_width = float.Parse(newWidth);
	}

	public void HeightChanged (string newWidth) {
		_height = float.Parse(newWidth);
	}


	private string _levelName = "firstLevel.xml";
	private void Save () {
		CharacterEditorBlock[] characterBlocks = FindObjectsOfType(typeof(CharacterEditorBlock)) as CharacterEditorBlock[];
		EditorBlock[] blocks = FindObjectsOfType(typeof(EditorBlock)) as EditorBlock[];

		SavableEditorBlock[] savableBlocks = new SavableEditorBlock[blocks.Length];

		SavableLevel toSave = new SavableLevel();
		toSave.worldDims = new Vector2(_width, _height);
		for (int i = 0; i < blocks.Length; i++) {
			savableBlocks[i].type = blocks[i].TypeName;
			savableBlocks[i].name = blocks[i].GO.name;
			savableBlocks[i].position = blocks[i].Position;
			savableBlocks[i].rotation = blocks[i].Rotation.eulerAngles;
			savableBlocks[i].scale = new Vector3(1,1,1); // TODO(Julian): Save the scale!
			savableBlocks[i].editorPivot = Vector3.zero; // TODO(Julian): Save the editor pivot!
		}
		toSave.levelBlocks = savableBlocks;
		toSave.characterXYPos = new Vector2(_characterXYTransform.position.x, _characterXYTransform.position.y);
		toSave.characterZYPos = new Vector2(_characterZYTransform.position.x, _characterZYTransform.position.y);

		XmlSerializer levelSerializer = new XmlSerializer(typeof(SavableLevel));
		StreamWriter levelWriter = new StreamWriter(_levelName); // TODO(Julian): Varying filenames
		levelSerializer.Serialize(levelWriter, toSave);
		levelWriter.Close();
	}

	private void Load () {
		XmlSerializer levelDeserializer = new XmlSerializer(typeof(SavableLevel));
		FileStream levelReader = new FileStream(_levelName, FileMode.Open); // TODO(Julian): Varying filenames
		XmlReader xmlReader = XmlReader.Create(levelReader);
		SavableLevel toLoad = (SavableLevel)levelDeserializer.Deserialize(xmlReader);
		levelReader.Close();

		CharacterEditorBlock[] characterBlocks = FindObjectsOfType(typeof(CharacterEditorBlock)) as CharacterEditorBlock[];
		EditorBlock[] blocks = FindObjectsOfType(typeof(EditorBlock)) as EditorBlock[];
		for (int i = 0; i < blocks.Length; i++) {
			Destroy(blocks[i].GO);
		}

		// TODO(Julian): Destroy the characters!
		_characterXYTransform.position = new Vector3(toLoad.characterXYPos[0],toLoad.characterXYPos[1],0);
		_characterZYTransform.position = new Vector3(toLoad.characterZYPos[0],toLoad.characterZYPos[1],0);

		SavableEditorBlock[] loadableBlocks = toLoad.levelBlocks;
		toLoad.worldDims = new Vector2(_width, _height);
		for (int i = 0; i < blocks.Length; i++) {
			SavableEditorBlock currBlock = loadableBlocks[i];
			for (int j = 0; j < _spawnablePrefabs.Length; j++) {
				IEditorBlock blockRef = _spawnablePrefabs[j].GetInterface<IEditorBlock>();
				if (blockRef.TypeName == currBlock.type) {
					GameObject createdObject = (Instantiate(_spawnablePrefabs[j], currBlock.position, Quaternion.Euler(currBlock.rotation)) as GameObject);
					createdObject.name = currBlock.name;
					// TODO(Julian): Use the scale!
					// TODO(Julian): Use the editor pivot!
					break;
				}
			}
		}

		_selectedEditorBlock = null;
		_isMoving = false;


	}

	void DrawSelectedEditorBlock() {
		Mesh mesh = _meshFilter.sharedMesh;
		if (_selectedEditorBlock != null) {
			Vector3[] verts = _selectedEditorBlock.SelectionVerts;
			Vector3[] normals = _selectedEditorBlock.SelectionVertNormals;

			Vector3[] drawVerts = new Vector3[verts.Length * 12];
			Vector3[] drawNormals = new Vector3[normals.Length * 12];
			int[] triangles = new int[verts.Length * 12];
			float r = 0.2f;
			// Unrotated and not moved tetrahedron verts
			Vector3 pt0 = new Vector3(0,0,0);
			Vector3 pt1 = new Vector3(r,0,0);
			Vector3 pt2 = new Vector3(0.5f*r,0,Mathf.Sqrt(0.75f)*r);
			Vector3 pt3 = new Vector3(0.5f*r,Mathf.Sqrt(0.75f)*r,Mathf.Sqrt(0.75f)/3*r);

			mesh.Clear();

			for (int i = 0; i < verts.Length; i++) {
				Quaternion rot = Quaternion.FromToRotation(new Vector3(0,0,1), normals[i]);
				// Quaternion rot = Quaternion.identity;
				Vector3 p0 = (rot * pt0) + verts[i];
				Vector3 p1 = (rot * pt1) + verts[i];
				Vector3 p2 = (rot * pt2) + verts[i];
				Vector3 p3 = (rot * pt3) + verts[i];
				drawVerts[i * 12 + 0] = p0; drawVerts[i * 12 + 1] = p1; drawVerts[i * 12 + 2] = p2;
				drawVerts[i * 12 + 3] = p0; drawVerts[i * 12 + 4] = p2; drawVerts[i * 12 + 5] = p3;
				drawVerts[i * 12 + 6] = p2; drawVerts[i * 12 + 7] = p1; drawVerts[i * 12 + 8] = p3;
				drawVerts[i * 12 + 9] = p0; drawVerts[i * 12 + 10] = p3; drawVerts[i * 12 + 11] = p1;

				triangles[i * 12 + 0] = i*12+0; triangles[i * 12 + 1] = i*12+1; triangles[i * 12 + 2] = i*12+2;
				triangles[i * 12 + 3] = i*12+3; triangles[i * 12 + 4] = i*12+4; triangles[i * 12 + 5] = i*12+5;
				triangles[i * 12 + 6] = i*12+6; triangles[i * 12 + 7] = i*12+7; triangles[i * 12 + 8] = i*12+8;
				triangles[i * 12 + 9] = i*12+9; triangles[i * 12 + 10] = i*12+10; triangles[i * 12 + 11] = i*12+11;



				Debug.DrawLine(verts[i],verts[i]+normals[i], Color.blue);
			}
			mesh.vertices = drawVerts;
			mesh.triangles = triangles;

			mesh.RecalculateNormals();
			mesh.RecalculateBounds();
			mesh.Optimize();
			// mesh.vertices = verts;
		}

	}

	// Update is called once per frame
	void Update () {

		DrawSelectedEditorBlock();

		if(Input.GetKeyDown(KeyCode.S)) {
			Save();
		}

		if(Input.GetKeyDown(KeyCode.L)) {
			Load();
		}

		if (_isMoving) {
			Vector3 lastRelativePoint = _relativeCam.ScreenToViewportPoint(_lastMousePos);
			Vector3 relativePoint = _relativeCam.ScreenToViewportPoint(Input.mousePosition);
			_mouseDelta += (Vector2)(relativePoint - lastRelativePoint);

			Vector3 p = _relativeCam.ViewportToWorldPoint(_relativeCam.WorldToViewportPoint(_sourcePosition) + (Vector3)_mouseDelta);
			_selectedEditorBlock.Position = SnappingMath.SnapToRounded(p, _precision);

			if (Input.GetKeyDown(KeyCode.Mouse0)) {
				_isMoving = false;
			}

			if (Input.GetKeyDown(KeyCode.Escape)) {
				if (_killObjectOnAbort) {
					Destroy(_selectedEditorBlock.GO);
					_selectedEditorBlock = null;
					_killObjectOnAbort = false;
				} else {
					_selectedEditorBlock.Position = _sourcePosition;
				}
				_isMoving = false;
			}


			for (int i = 0; i < _spawnablePrefabs.Length; i++) {
				if (Input.GetKeyDown(KeyCode.Alpha1 + i)) {
					var pos = _selectedEditorBlock.Position;
					var rot = _selectedEditorBlock.Rotation;
					Destroy(_selectedEditorBlock.GO);
					_selectedEditorBlock = (Instantiate(_spawnablePrefabs[i], pos, rot) as GameObject).GetInterface<IEditorBlock>();

					// TODO(Julian): Make Abort work Properly!

					StartGrabbing(_relativeCam);
					break;
				}
			}

			_lastMousePos = Input.mousePosition;
			return;
		}

		Camera foundCamera = null;
		for (int i = 0; i < _cameras.Length; i++) {
			Vector3 testPoint = _cameras[i].ScreenToViewportPoint(Input.mousePosition);
			if (testPoint.x >= 0f && testPoint.x <= 1f &&
				testPoint.y >= 0f && testPoint.y <= 1f) {
				// Within bounds
				foundCamera = _cameras[i];
				break;
			}
		}

		if (foundCamera == null) {
			return;
		}


		if ((Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)) && Input.GetKeyDown(KeyCode.R)) {
			DoRotate90(foundCamera, -1f, _precision);
			return;
		}

		if (Input.GetKeyDown(KeyCode.R)) {
			DoRotate90(foundCamera, 1f, _precision);
			return;
		}

		if (Input.GetKeyDown(KeyCode.G)) {
			StartGrabbing(foundCamera);
			return;
		}

		if ((Input.GetKey(KeyCode.LeftShift) ||
			 Input.GetKey(KeyCode.RightShift)) && Input.GetKeyDown(KeyCode.D)) {
			_selectedEditorBlock = (Instantiate(_selectedEditorBlock.GO, _selectedEditorBlock.Position, _selectedEditorBlock.Rotation) as GameObject).GetInterface<IEditorBlock>();
			_killObjectOnAbort = true;
			StartGrabbing(foundCamera);
			return;
		}


		for (int i = 0; i < _spawnablePrefabs.Length; i++) {
			if (Input.GetKeyDown(KeyCode.Alpha1 + i)) {
				Vector3 p = foundCamera.ViewportToWorldPoint(foundCamera.ScreenToViewportPoint(Input.mousePosition));
				p += foundCamera.transform.forward*4f;
				_selectedEditorBlock = (Instantiate(_spawnablePrefabs[i], p, _spawnablePrefabs[i].transform.rotation) as GameObject).GetInterface<IEditorBlock>();
				_killObjectOnAbort = true;
				StartGrabbing(foundCamera);
				return;
			}
		}

		if(Input.GetKeyDown(KeyCode.Mouse0)) {
			Vector3 testPoint = foundCamera.ScreenToViewportPoint(Input.mousePosition);
			var testRay = foundCamera.ViewportPointToRay(testPoint);
			RaycastHit hit;
			if (Physics.Raycast(testRay, out hit)) {
				var t = hit.transform;
				do {
					IEditorBlock editorBlock = t.gameObject.GetInterface<IEditorBlock>();
					if(editorBlock != null) {
						_selectedEditorBlock = editorBlock;
						break;
					}
					t = t.parent;
				} while (t != null);

	            Debug.DrawLine(testRay.origin, hit.point);
			}
			return;
		}

	}
}
