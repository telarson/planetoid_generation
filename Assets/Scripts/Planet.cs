/* Tristan Larson 2021 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Planet : MonoBehaviour
{

    private List<Polygon> m_Polygons;
    private List<Vector3> m_Vertices;

    private Mesh mesh;

    [Range(0,8)]
    public int Subdivisions = 3;

    public float Max_Height = 1;

    public float Min_Height = 0;

    public float redistribution = 2f;

    public int Roughness_Passes = 1;

    public HeightGenMethods HeightGenMethod = HeightGenMethods.Perlin;

    public Gradient coloring;

    public Vector3 offsetVector = new Vector3(0,0,0);

    public bool generate = true;


    public SimplexNoiseGenerator simplexNoise = new SimplexNoiseGenerator();
    public delegate Vector3 HeightGenerator(Vector3 spherePoint, Vector3 seedVec);
    public Dictionary<string, HeightGenerator> heightGenerators = new Dictionary<string, HeightGenerator>();
    private Color[] colors;


    void Start()
    {
        heightGenerators.Add("Perlin", GenerateHeightPerlin);
        heightGenerators.Add("FBM", GenerateHeightFBM);
        heightGenerators.Add("Simplex", GenerateHeightSimplex);
        
        InitAsIcosohedron();
        Subdivide(Subdivisions);
        GenerateMesh();
        GetComponent<MeshFilter>().mesh = mesh;
    }

    // Update is called once per frame
    void Update()
    {
        if (!Input.GetKeyDown(KeyCode.R))
        {
            this.transform.Rotate(Vector3.up * (30.0f * Time.deltaTime));
        }
    }

    public Vector3 GenerateHeightPerlin(Vector3 spherePoint, Vector3 seedVec)
    {
        var nx = spherePoint + seedVec;
        float height = 1f * Perlin.Noise(nx) +
                     0.5f * Perlin.Noise(2 * nx) +
                     0.25f * Perlin.Noise(4 * nx);

        height = Mathf.Pow(height, redistribution);

        if (height > Max_Height)
            height = Max_Height;

        if (height < Min_Height)
            height = Min_Height;

        return spherePoint * (1 + height);
    }

    public Vector3 GenerateHeightFBM(Vector3 spherePoint, Vector3 seedVec)
    {
        var nx = spherePoint + seedVec;
        float height = 1f * Perlin.Fbm(nx, 1) +
                     0.5f * Perlin.Fbm(2 * nx, 2) +
                     0.25f * Perlin.Fbm(4 * nx, 3);

        height = Mathf.Pow(height, redistribution);

        if (height > Max_Height)
            height = Max_Height;

        if (height < Min_Height)
            height = Min_Height;

        return spherePoint * (1 + height);
    }

    public Vector3 GenerateHeightSimplex(Vector3 spherePoint, Vector3 seedVec)
    {
        var nx = spherePoint + seedVec;
        float height = 1f * (float)simplexNoise.noise(nx.x, nx.y, nx.z) +
                     0.5f * (float)simplexNoise.noise(2 * nx.x, 2 * nx.y, 2 * nx.z) +
                     0.25f * (float)simplexNoise.noise(4 * nx.x, 4 * nx.y, 4 * nx.z);

        height = Mathf.Pow(height, redistribution);

        if (height > Max_Height)
            height = Max_Height;

        if (height < Min_Height)
            height = Min_Height;

        return spherePoint * (1f + height);
    }

    public void GenerateMesh()
    {
        mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        int vertexCount = m_Polygons.Count * 3; 

        int[] indices = new int[vertexCount]; 

        Vector3[] vertices = new Vector3[vertexCount];
        Vector3[] normals = new Vector3[vertexCount];
        colors = new Color[vertexCount];



        for (int i = 0; i < m_Polygons.Count; i++)
        {
            var poly = m_Polygons[i]; 
            indices[i * 3 + 0] = i * 3 + 0;
            indices[i * 3 + 1] = i * 3 + 1;
            indices[i * 3 + 2] = i * 3 + 2; 
            vertices[i * 3 + 0] = m_Vertices[poly.m_Vertices[0]];
            vertices[i * 3 + 1] = m_Vertices[poly.m_Vertices[1]];
            vertices[i * 3 + 2] = m_Vertices[poly.m_Vertices[2]];   
        }

        if (generate)
        {
            var heightGenner = heightGenerators[HeightGenMethod.ToString()];

            for (int i = 0; i < vertices.Length; i++)
            {
                Vector3 old = vertices[i];
                vertices[i] = heightGenner(old, offsetVector);
                colors[i] = coloring.Evaluate(Vector3.Distance(old, vertices[i]) / Max_Height);
            }

            for (int k = 0; k < Roughness_Passes; k++)
            {
                for (int i = 0; i < vertices.Length; i++)
                {
                    Vector3 old = vertices[i];
                    vertices[i] += 0.10f * heightGenner(2 * old, offsetVector);
                }
            }
        }
        else
        {
            for (int i = 0; i < vertices.Length; i++)
            {
                colors[i] = Color.white;
            }
        }
        
        mesh.vertices = vertices;
        mesh.SetTriangles(indices, 0);
        mesh.RecalculateNormals();
        mesh.colors = colors;   
    }


    public void InitAsIcosohedron()
    {
        m_Polygons = new List<Polygon>();
        m_Vertices = new List<Vector3>();

        float t = (1.0f + Mathf.Sqrt(5.0f)) / 2.0f;

        m_Vertices.Add(new Vector3(-1, t, 0).normalized);
        m_Vertices.Add(new Vector3(1, t, 0).normalized);
        m_Vertices.Add(new Vector3(-1, -t, 0).normalized);
        m_Vertices.Add(new Vector3(1, -t, 0).normalized);
        m_Vertices.Add(new Vector3(0, -1, t).normalized);
        m_Vertices.Add(new Vector3(0, 1, t).normalized);
        m_Vertices.Add(new Vector3(0, -1, -t).normalized);
        m_Vertices.Add(new Vector3(0, 1, -t).normalized);
        m_Vertices.Add(new Vector3(t, 0, -1).normalized);
        m_Vertices.Add(new Vector3(t, 0, 1).normalized);
        m_Vertices.Add(new Vector3(-t, 0, -1).normalized);
        m_Vertices.Add(new Vector3(-t, 0, 1).normalized);

        m_Polygons.Add(new Polygon(0, 11, 5));
        m_Polygons.Add(new Polygon(0, 5, 1));
        m_Polygons.Add(new Polygon(0, 1, 7));
        m_Polygons.Add(new Polygon(0, 7, 10));
        m_Polygons.Add(new Polygon(0, 10, 11));
        m_Polygons.Add(new Polygon(1, 5, 9));
        m_Polygons.Add(new Polygon(5, 11, 4));
        m_Polygons.Add(new Polygon(11, 10, 2));
        m_Polygons.Add(new Polygon(10, 7, 6));
        m_Polygons.Add(new Polygon(7, 1, 8));
        m_Polygons.Add(new Polygon(3, 9, 4));
        m_Polygons.Add(new Polygon(3, 4, 2));
        m_Polygons.Add(new Polygon(3, 2, 6));
        m_Polygons.Add(new Polygon(3, 6, 8));
        m_Polygons.Add(new Polygon(3, 8, 9));
        m_Polygons.Add(new Polygon(4, 9, 5));
        m_Polygons.Add(new Polygon(2, 4, 11));
        m_Polygons.Add(new Polygon(6, 2, 10));
        m_Polygons.Add(new Polygon(8, 6, 7));
        m_Polygons.Add(new Polygon(9, 8, 1));
    }

    public void Subdivide(int recursions)
    {
        var midPointCache = new Dictionary<int, int>();

        for (int i = 0; i < recursions; i++)
        {
            var newPolys = new List<Polygon>();
            foreach (var poly in m_Polygons)
            {
                int a = poly.m_Vertices[0];
                int b = poly.m_Vertices[1];
                int c = poly.m_Vertices[2];

                int ab = GetMidPointIndex(midPointCache, a, b);
                int bc = GetMidPointIndex(midPointCache, b, c);
                int ca = GetMidPointIndex(midPointCache, c, a);

                newPolys.Add(new Polygon(a, ab, ca));
                newPolys.Add(new Polygon(b, bc, ab));
                newPolys.Add(new Polygon(c, ca, bc));
                newPolys.Add(new Polygon(ab, bc, ca));
            }

            m_Polygons = newPolys;
        }
    }
    public int GetMidPointIndex(Dictionary<int, int> cache, int indexA, int indexB)
    {

        int smallerIndex = Mathf.Min(indexA, indexB);
        int greaterIndex = Mathf.Max(indexA, indexB);
        int key = (smallerIndex << 16) + greaterIndex;

        int ret;
        if (cache.TryGetValue(key, out ret))
            return ret;

        Vector3 p1 = m_Vertices[indexA];
        Vector3 p2 = m_Vertices[indexB];
        Vector3 middle = Vector3.Lerp(p1, p2, 0.5f).normalized;

        ret = m_Vertices.Count;
        m_Vertices.Add(middle);

        cache.Add(key, ret);
        return ret;
    }


    public enum HeightGenMethods
    {
        Perlin,
        FBM,
        Simplex
    }

}
