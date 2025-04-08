using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class IslandMeshGenerator : MonoBehaviour
{
    public int minSize = 1;  // Минимальный размер (1 = 30 юнитов)
    public int maxSize = 10; // Максимальный размер (10 = 300 юнитов)
    public float segmentSize = 30f; // Размер сегмента

    private Mesh mesh;
    private Vector3[] vertices;
    private int[] triangles;

    void Start()
    {
        GenerateIsland();
    }

    void GenerateIsland()
    {
        // Случайные размеры острова (в сегментах 30x30)
        int widthSegments = Random.Range(minSize, maxSize + 1);
        int lengthSegments = Random.Range(minSize, maxSize + 1);

        // Расчёт вершин и треугольников
        int vertexCount = (widthSegments + 1) * (lengthSegments + 1);
        vertices = new Vector3[vertexCount];
        triangles = new int[widthSegments * lengthSegments * 6];

        // Генерация вершин
        for (int z = 0, i = 0; z <= lengthSegments; z++)
        {
            for (int x = 0; x <= widthSegments; x++, i++)
            {
                // Определяем, находится ли вершина на краю
                bool isBorder = x == 0 || z == 0 || x == widthSegments || z == lengthSegments;

                // Если вершина на краю, плавно снижаем высоту под 45°
                if (isBorder)
                {
                    int distanceToEdge = Mathf.Min(x, widthSegments - x, z, lengthSegments - z);
                    float height = Mathf.Lerp(0f, segmentSize, distanceToEdge / (float)Mathf.Max(widthSegments, lengthSegments));
                    vertices[i] = new Vector3(x * segmentSize, height, z * segmentSize);
                }
                else
                {
                    // Внутри острова — фиксированная высота 30
                    vertices[i] = new Vector3(x * segmentSize, segmentSize, z * segmentSize);
                }
            }
        }

        // Генерация треугольников
        for (int ti = 0, vi = 0, z = 0; z < lengthSegments; z++, vi++)
        {
            for (int x = 0; x < widthSegments; x++, ti += 6, vi++)
            {
                triangles[ti] = vi;
                triangles[ti + 1] = vi + widthSegments + 1;
                triangles[ti + 2] = vi + 1;
                triangles[ti + 3] = vi + 1;
                triangles[ti + 4] = vi + widthSegments + 1;
                triangles[ti + 5] = vi + widthSegments + 2;
            }
        }

        // Создание меша
        mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<MeshCollider>().sharedMesh = mesh; // Добавь коллайдер, если нужно
    }
}