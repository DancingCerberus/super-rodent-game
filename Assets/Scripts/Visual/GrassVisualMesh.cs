using UnityEngine;

namespace SuperRodentGame.Visual
{
    /// <summary>
    /// Generates a subdivided plane with Perlin-noise vertex displacement for
    /// visual grass bumps. No MeshCollider is added, so physics is unaffected.
    /// Place this on a child GameObject of the arena root, just above Y=0.
    /// </summary>
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class GrassVisualMesh : MonoBehaviour
    {
        [SerializeField] private Vector2 fieldSize = new Vector2(36f, 36f);
        [SerializeField] private int gridX = 48;
        [SerializeField] private int gridZ = 48;
        [SerializeField] private float noiseScale    = 0.10f;
        [SerializeField] private float noiseAmplitude = 0.20f;
        [SerializeField] private Vector2 noiseOffset;

        private void Awake()
        {
            GetComponent<MeshFilter>().mesh = BuildMesh();
        }

        private Mesh BuildMesh()
        {
            var mesh = new Mesh { name = "GrassVisual" };
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

            int vx = gridX + 1;
            int vz = gridZ + 1;
            var verts = new Vector3[vx * vz];
            var uvs   = new Vector2[vx * vz];

            float dx = fieldSize.x / gridX;
            float dz = fieldSize.y / gridZ;
            float ox = -fieldSize.x * 0.5f;
            float oz = -fieldSize.y * 0.5f;

            for (int z = 0; z < vz; z++)
            {
                for (int x = 0; x < vx; x++)
                {
                    float wx = ox + x * dx;
                    float wz = oz + z * dz;
                    float ny = Mathf.PerlinNoise(
                        (wx + noiseOffset.x) * noiseScale,
                        (wz + noiseOffset.y) * noiseScale) * noiseAmplitude;

                    int idx = z * vx + x;
                    verts[idx] = new Vector3(wx, ny, wz);
                    uvs[idx]   = new Vector2((float)x / gridX, (float)z / gridZ);
                }
            }

            var tris = new int[gridX * gridZ * 6];
            int t = 0;
            for (int z = 0; z < gridZ; z++)
            {
                for (int x = 0; x < gridX; x++)
                {
                    int bl = z * vx + x;
                    int br = bl + 1;
                    int tl = bl + vx;
                    int tr = tl + 1;
                    tris[t++] = bl; tris[t++] = tl; tris[t++] = br;
                    tris[t++] = br; tris[t++] = tl; tris[t++] = tr;
                }
            }

            mesh.vertices  = verts;
            mesh.uv        = uvs;
            mesh.triangles = tris;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }
    }
}
