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
    [Header("Springs")]
    [SerializeField] private float _spriteConstant = 1.4f;
    [SerializeField] private float _damping = 1.1f;
    [SerializeField] private float _spread = 6.5f;
    [SerializeField, Range(1,10)] private int _wavePropogationIteration = 8;
    [SerializeField, Range(0, 20f)] private float _speedMult = 5.5f;

    [Header("Force")]
    public float ForceMultiplier = 0.2f;
    [SerializeField, Range(0, 50f)] public float MaxForce = 5f;


    [Header("Collision")]
    [SerializeField, Range(1f, 10f)] private float _playerCollisionRadiusMult = 4.15f;

    private class WaterPoint
    {
        public float velocity, acceleration, pos, targetHeight;
    }

    private List<WaterPoint> _waterPoints = new List<WaterPoint>();


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
        _edgeCollider = GetComponent<EdgeCollider2D>();
        GenerateMesh();
        CreateWaterPoints();
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(Vector2.up * Time.deltaTime * 0.03f);
    }

    private void FixedUpdate()
    {
        //update all spring positions
        for(int i =1; i  < _waterPoints.Count - 1; ++i)
        {
            WaterPoint point = _waterPoints[i];
            float x = point.pos - point.targetHeight;
            float acceleration = -_spriteConstant * x - _damping * point.velocity;
            point.pos += point.velocity * _speedMult * Time.fixedDeltaTime;
            _vertices[_topVerticesIndex[i]].y = point.pos;
            point.velocity += acceleration * _speedMult * Time.fixedDeltaTime;
            //WaterPoint left = _waterPoints[i - 1];
            //WaterPoint right = _waterPoints[i + 1];
            //// spring force
            //float leftForce = (left.pos - point.pos) * _spriteConstant;
            //float rightForce = (right.pos - point.pos) * _spriteConstant;
            //// damping force
            //float leftDamping = (left.velocity - point.velocity) / _damping;
            //float rightDamping = (right.velocity - point.velocity) / _damping;
            //// spread force
            //float spreadForce = (left.pos + right.pos - 2 * point.pos) / _spread;
            //// update acceleration
            //point.acceleration = leftForce + rightForce + leftDamping + rightDamping + spreadForce;
        }

        // Wave propogation
        for (int j = 0; j < _wavePropogationIteration; ++j)
        {
            for (int i = 1; i < _waterPoints.Count - 1; ++i)
            {
                float leftDelta = _spread * (_waterPoints[i].pos - _waterPoints[i - 1].pos) * _speedMult * Time.fixedDeltaTime;
                _waterPoints[i - 1].velocity += leftDelta;
                float rightDelta = _spread * (_waterPoints[i].pos - _waterPoints[i + 1].pos) * _speedMult * Time.fixedDeltaTime;
                _waterPoints[i + 1].velocity += rightDelta;
            }
        }

        // update the mesh vertices
        _mesh.vertices = _vertices;
    }

    public void Splash(Collider2D collision, float force)
    {
        float radius = collision.bounds.extents.x * _playerCollisionRadiusMult;
        Vector2 center = collision.transform.position;

        for (int i = 0; i < _waterPoints.Count; ++i)
        {
            Vector2 vertexWorldPos = transform.TransformPoint(_vertices[_topVerticesIndex[i]]);
            if(IsPointInsideCircle(vertexWorldPos, center, radius))
            {
                _waterPoints[i].velocity = force;
            }
        }

    }

    private bool IsPointInsideCircle(Vector2 point, Vector2 center, float radius)
    {
        // dont need to square root, more efficient for circle collision check
        float distanceSquared = (point - center).sqrMagnitude; 
        return distanceSquared <= radius*radius;
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

    
    private void CreateWaterPoints()
    {
        _waterPoints.Clear();
        for (int i = 0; i < _topVerticesIndex.Length; ++i)
        {
            WaterPoint point = new WaterPoint
            {
                pos = _vertices[_topVerticesIndex[i]].y,
                targetHeight = _vertices[_topVerticesIndex[i]].y,
            };
            _waterPoints.Add(point);
        }
    }


}

[CustomEditor(typeof(InteractableWater))]
public class InteractableWaterEditor : Editor
{
    private InteractableWater _water;

    private void OnEnable()
    {
        _water = (InteractableWater)target;
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
