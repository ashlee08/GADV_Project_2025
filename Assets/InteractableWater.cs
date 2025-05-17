using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(EdgeCollider2D))]
[RequireComponent(typeof(WaterTriggerHandler))]
public class InteractableWater : MonoBehaviour
{
    [Header("Mesh Generation")]
    [Range(2, 500)] public int NumOfXVertices = 70;
    public float Width = 10f;
    public float Height = 4f;
    public Material WaterMaterial;
    private const int NUM_OF_Y_VERTICES = 2;

    [Header("Gizmo")]
    public Color GizmoColor = Color.white;

    private Mesh _mesh;
    private MeshRenderer _renderer;
    private MeshFilter _filter;
    private Vector3[] _vertices;
    private int[] _topVerticesIndex;

    private EdgeCollider2D _edgeCollider;

    // Start is called before the first frame update
    void Start()
    {
        GenerateMesh();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void Reset()
    {
        _edgeCollider = GetComponent<EdgeCollider2D>();
        _edgeCollider.isTrigger = true;
    }

    // For editor use
    public void ResetEdgeCollider()
    {
        _edgeCollider = GetComponent<EdgeCollider2D>();
        Vector2[] newPoints = new Vector2[2];
        Vector2 firstPoint = new Vector2(_vertices[_topVerticesIndex[0]].x, _vertices[_topVerticesIndex[0]].y);
        newPoints[0] = firstPoint;
        Vector2 secondPoint = new Vector2(_vertices[_topVerticesIndex[_topVerticesIndex.Length - 1]].x, _vertices[_topVerticesIndex[_topVerticesIndex.Length - 1]].y);
        newPoints[1] = secondPoint;

        _edgeCollider.offset = Vector2.zero;
        _edgeCollider.points = newPoints;
    }

    public void GenerateMesh()
    {
        _mesh = new Mesh();
        // X-> Columns
        // Y -> Rows
        _vertices = new Vector3[NumOfXVertices * NUM_OF_Y_VERTICES]; // number of water quads
        _topVerticesIndex = new int[NumOfXVertices]; // water line on top
        for(int y = 0; y < NUM_OF_Y_VERTICES; ++y)
        {
            for(int x = 0; x < NumOfXVertices; ++x)
            {
                // xPos and yPos is the middle of the water quad
                float xPos = (x / (float)(NumOfXVertices - 1)) * Width - Width / 2;
                float yPos = (y / (float)(NUM_OF_Y_VERTICES - 1)) * Height - Height / 2;

                // vertices is 1D array,(y* NumOfXVertices) + x
                // row (y* NumOfXVertices)
                // +x is your column offset.
                _vertices[y * NumOfXVertices + x] = new Vector3(xPos, yPos, 0f);
                if(y == NUM_OF_Y_VERTICES - 1)
                {
                    _topVerticesIndex[x] = y * NumOfXVertices + x;
                }
            }
        }

        // construct triangles
        int[] triangles = new int[(NumOfXVertices - 1) * (NUM_OF_Y_VERTICES - 1) * 6];
        int index = 0;
        for(int y =0; y < NUM_OF_Y_VERTICES - 1; ++y)
        {
            for (int x = 0; x < NumOfXVertices -1; ++x)
            {
                int bottomLeft = y * NumOfXVertices + x;
                int bottomRight = bottomLeft + 1;
                int topLeft = bottomLeft + NumOfXVertices;
                int topRight = topLeft + 1;

                // first triangle
                triangles[index++] = bottomLeft;
                triangles[index++] = topLeft;
                triangles[index++] = bottomRight;

                // second trianlg
                triangles[index++] = bottomRight;
                triangles[index++] = topLeft;
                triangles[index++] = topRight;
                   
            }
        }

        //UVs - water blue color, water effect visually textures
        Vector2[] uvs = new Vector2[_vertices.Length];
        for(int i = 0; i < _vertices.Length; ++i)
        {
            uvs[i] = new Vector2((_vertices[i].x + Width / 2) / Width, (_vertices[i].y + Height / 2) / Height);
        }

        if (_renderer == null)
        {
            _renderer = GetComponent<MeshRenderer>();
        }

        if (_filter == null)
        {
            _filter = GetComponent<MeshFilter>();
        }

        _renderer.material = WaterMaterial;
        _mesh.vertices = _vertices;
        _mesh.triangles = triangles;
        _mesh.uv = uvs;

        _mesh.RecalculateNormals();
        _mesh.RecalculateBounds();

        _filter.mesh = _mesh;
    }

    [CustomEditor(typeof(InteractableWater))]
    public class InteractableWaterEditor : Editor
    {
        private InteractableWater _water;

        private void OnEnable()
        {
            _water =  (InteractableWater)target;
        }

        public override VisualElement CreateInspectorGUI()
        {
            VisualElement root = new VisualElement();
            InspectorElement.FillDefaultInspector(root, serializedObject, this);
            root.Add(new VisualElement { style = { height = 10 } });

            Button generateMeshButton = new Button(() => _water.GenerateMesh())
            {
                text = "Generate Mesh"
            };
            root.Add(generateMeshButton);


            Button placeEdgeColliderButton = new Button(() => _water.ResetEdgeCollider())
            {
                text = "Place Edge Collider"
            };
            root.Add(placeEdgeColliderButton);

            return root;
        }
        private void ChangeDimensions(ref float width, ref float height, float calculatedWidthMax, float calculatedHeightMax)
        {
            width = Mathf.Max(0.1f, calculatedWidthMax);
            height = Mathf.Max(0.1f, calculatedHeightMax);
        }

        private void OnSceneGUI()
        {
            // Draw the wireframe box
            Handles.color = _water.GizmoColor;
            Vector3 center = _water.transform.position;
            Vector3 size = new Vector3(_water.Width, _water.Height, 0.1f);
            Handles.DrawWireCube(center, size);

            float handleSize = HandleUtility.GetHandleSize(center) * 0.1f;
            Vector3 snap = Vector3.one * 0.1f;

            Vector3[] corners = new Vector3[4];
            corners[0] = center + new Vector3(-_water.Width / 2, -_water.Height / 2, 0);
            corners[1] = center + new Vector3(_water.Width / 2, -_water.Height / 2, 0);
            corners[2] = center + new Vector3(-_water.Width / 2, _water.Height / 2, 0);
            corners[3] = center + new Vector3(_water.Width / 2, _water.Height / 2, 0);

            EditorGUI.BeginChangeCheck();
            Vector3 newBottomLeft = Handles.FreeMoveHandle(corners[0], handleSize, snap, Handles.CubeHandleCap);
            if (EditorGUI.EndChangeCheck())
            {
                ChangeDimensions(ref _water.Width, ref _water.Height, corners[1].x - newBottomLeft.x, corners[3].y - newBottomLeft.y);
                _water.transform.position += new Vector3((newBottomLeft.x - corners[0].x) / 2, (newBottomLeft.y - corners[0].y) / 2, 0);
            }

            EditorGUI.BeginChangeCheck();
            Vector3 newBottomRight = Handles.FreeMoveHandle(corners[1], handleSize, snap, Handles.CubeHandleCap);
            if (EditorGUI.EndChangeCheck())
            {
                ChangeDimensions(ref _water.Width, ref _water.Height, newBottomRight.x - corners[0].x, corners[3].y - newBottomRight.y);
                _water.transform.position += new Vector3((newBottomRight.x - corners[1].x) / 2, (newBottomLeft.y - corners[1].y) / 2, 0);
            }

            EditorGUI.BeginChangeCheck();
            Vector3 newTopLeft = Handles.FreeMoveHandle(corners[2], handleSize, snap, Handles.CubeHandleCap);
            if (EditorGUI.EndChangeCheck())
            {
                ChangeDimensions(ref _water.Width, ref _water.Height, corners[3].x - newTopLeft.x, newTopLeft.y - corners[0].y);
                _water.transform.position += new Vector3((newTopLeft.x - corners[2].x) / 2, (newTopLeft.y - corners[2].y) / 2, 0);
            }

            EditorGUI.BeginChangeCheck();
            Vector3 newTopRight = Handles.FreeMoveHandle(corners[3], handleSize, snap, Handles.CubeHandleCap);
            if (EditorGUI.EndChangeCheck())
            {
                ChangeDimensions(ref _water.Width, ref _water.Height, newTopRight.x - corners[2].x, newTopRight.y - corners[1].y);
                _water.transform.position += new Vector3((newTopRight.x - corners[3].x) / 2, (newTopRight.y - corners[3].y) / 2, 0);
            }

            if (GUI.changed)
            {
                _water.GenerateMesh();
            }
        }

    }

   
}
