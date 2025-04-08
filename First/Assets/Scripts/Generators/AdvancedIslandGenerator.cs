using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class RandomIslandGenerator : MonoBehaviour
{
    [Header("Base Settings")]
    [Range(3, 10)] public int complexity = 5; // Количество углов у формы
    [Range(1, 20)] public int size = 5;
    public float segmentSize = 30f;
    public float maxHeight = 30f;
    public Material islandMaterial;

    [Header("Edges")]
    [Range(0, 1)] public float edgeIrregularity = 0.5f;
    public bool sharpEdges = true;
    [Range(1, 5)] public int edgeSteps = 1;

    [Header("Noise")]
    public bool useNoise = true;
    [Range(0.01f, 0.2f)] public float noiseScale = 0.1f;
    [Range(0, 10f)] public float noiseHeight = 3f;

    [Header("Save Options")]
    public bool saveAsAsset = false;
    public string savePath = "Assets/GeneratedMeshes";

    private Mesh mesh;
    private List<Vector2> shapePoints = new List<Vector2>();

    void Start() => GenerateIsland();

    public void GenerateIsland()
    {
        GenerateRandomShape();
        CreateMesh();
        if (saveAsAsset) SaveMeshAsset();
    }

    void GenerateRandomShape()
    {
        shapePoints.Clear();
        float radius = size;
        float angleStep = 360f / complexity;

        // Генерация случайных точек формы
        for (int i = 0; i < complexity; i++)
        {
            float angle = angleStep * i;
            float randomRadius = radius * (1 - Random.Range(0, edgeIrregularity));
            Vector2 point = new Vector2(
                Mathf.Cos(angle * Mathf.Deg2Rad),
                Mathf.Sin(angle * Mathf.Deg2Rad)
            ) * randomRadius;
            shapePoints.Add(point);
        }
    }

    bool IsInShape(Vector2 point)
    {
        // Алгоритм проверки точки внутри полигона
        bool inside = false;
        for (int i = 0, j = shapePoints.Count - 1; i < shapePoints.Count; j = i++)
        {
            if (((shapePoints[i].y > point.y) != (shapePoints[j].y > point.y)) &&
                (point.x < (shapePoints[j].x - shapePoints[i].x) * (point.y - shapePoints[i].y) /
                (shapePoints[j].y - shapePoints[i].y) + shapePoints[i].x))
            {
                inside = !inside;
            }
        }
        return inside;
    }

    void CreateMesh()
    {
        mesh = new Mesh { name = "RandomIsland" };
        int segments = size * 2;
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uv = new List<Vector2>();

        // Генерация вершин
        for (int z = 0; z <= segments; z++)
        {
            for (int x = 0; x <= segments; x++)
            {
                Vector2 point = new Vector2(
                    (x - size) * segmentSize,
                    (z - size) * segmentSize
                );

                Vector2 normalizedPoint = new Vector2(
                    x / (float)segments,
                    z / (float)segments
                );

                float height = 0;
                if (IsInShape(normalizedPoint * size * 2 - Vector2.one * size))
                {
                    // Расчет высоты с учетом краев
                    float edgeDistance = CalculateEdgeDistance(normalizedPoint);
                    height = sharpEdges ?
                        edgeDistance < edgeSteps ? maxHeight * (edgeDistance / edgeSteps) : maxHeight :
                        Mathf.Lerp(0, maxHeight, edgeDistance / edgeSteps);

                    // Добавление шума
                    if (useNoise && height > 0)
                    {
                        height += Mathf.PerlinNoise(x * noiseScale, z * noiseScale) * noiseHeight;
                    }
                }

                vertices.Add(new Vector3(point.x, height, point.y));
                uv.Add(new Vector2(x / (float)segments, z / (float)segments));
            }
        }

        // Генерация треугольников
        for (int z = 0; z < segments; z++)
        {
            for (int x = 0; x < segments; x++)
            {
                int i = z * (segments + 1) + x;
                if (vertices[i].y > 0 || vertices[i + 1].y > 0 ||
                    vertices[i + segments + 1].y > 0 || vertices[i + segments + 2].y > 0)
                {
                    triangles.Add(i);
                    triangles.Add(i + segments + 1);
                    triangles.Add(i + 1);

                    triangles.Add(i + 1);
                    triangles.Add(i + segments + 1);
                    triangles.Add(i + segments + 2);
                }
            }
        }

        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.SetUVs(0, uv);
        mesh.RecalculateNormals();

        GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<MeshCollider>().sharedMesh = mesh;

        // Назначение материала
        if (islandMaterial != null)
        {
            GetComponent<MeshRenderer>().material = islandMaterial;
        }
    }

    float CalculateEdgeDistance(Vector2 point)
    {
        // Находим минимальное расстояние до края формы
        float minDistance = float.MaxValue;
        for (int i = 0, j = shapePoints.Count - 1; i < shapePoints.Count; j = i++)
        {
            Vector2 edgeStart = shapePoints[j];
            Vector2 edgeEnd = shapePoints[i];
            float distance = DistanceToSegment(point, edgeStart, edgeEnd);
            if (distance < minDistance) minDistance = distance;
        }
        return minDistance;
    }

    float DistanceToSegment(Vector2 p, Vector2 a, Vector2 b)
    {
        Vector2 ab = b - a;
        Vector2 ap = p - a;
        float projection = Vector2.Dot(ap, ab) / ab.sqrMagnitude;
        projection = Mathf.Clamp01(projection);
        Vector2 closest = a + projection * ab;
        return Vector2.Distance(p, closest);
    }

#if UNITY_EDITOR
    void SaveMeshAsset()
    {
        if (!AssetDatabase.IsValidFolder(savePath))
        {
            AssetDatabase.CreateFolder("Assets", "GeneratedMeshes");
        }

        string path = $"{savePath}/Island_{System.DateTime.Now:yyyyMMddHHmmss}.asset";
        AssetDatabase.CreateAsset(mesh, path);
        AssetDatabase.SaveAssets();
        Debug.Log($"Mesh saved to {path}");
    }

    [CustomEditor(typeof(RandomIslandGenerator))]
    public class RandomIslandGeneratorEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("Generate Island"))
            {
                ((RandomIslandGenerator)target).GenerateIsland();
            }
        }
    }
#endif
}