using UnityEngine;

public class Planet : MonoBehaviour
{
    //--Debug--
    public bool debugMode;
    public bool showField, showChunks;
    public bool debugChunks;

    //--Main Stuff--
    public Material mat;
    public int layerMask;
    public int num, size;
    public int startHeight;

    //--Noise--
    public string seed;
    public int oct;
    public float frq, amp;

    //--Gravity--
    public float gravity;

    //--Chunks--
    [HideInInspector] public Chunk[] chunks;

    //--Private Stuff--
    private AnimationCurve globalHeightFunction;
    private Vector3 c;
    private PerlinNoise noise;

    void Start()
    {
        int m = (num / 2) * size + (size / 2);
        c = new Vector3(m, m, m);

        chunks = new Chunk[num * num * num];
        noise = new PerlinNoise(GetSeed());

        for (int x = 0; x < num; x++)
            for (int y = 0; y < num; y++)
                for (int z = 0; z < num; z++)
                {
                    string name = string.Format("{0} / {1} / {2}", x, y, z);
                    Vector3 pos = new Vector3(x * size, y * size, z * size);

                    chunks[x + y * num + z * num * num] = new Chunk(this, name, pos, transform, num, size, mat, layerMask);
                }
    }

    //--Public Methods--
    public void Generate()
    {
        if(chunks != null)
            foreach (Chunk chunk in chunks)
                chunk.Generate();
    }
    public void Build()
    {
        if (chunks != null)
            foreach (Chunk chunk in chunks)
                chunk.Build();
    }
    public void GenerateEmptySurface(float[] field, Vector3 pos)
    {
        int rS = size + 1;

        for (int x = 0; x < rS; x++)
            for (int y = 0; y < rS; y++)
                for (int z = 0; z < rS; z++)
                {
                    Vector3 p = pos + new Vector3(x, y, z);
                    float d = (c - p).magnitude;
                    float t = 1f - 1f / (1f + d);

                    float n = noise.FractalNoise3D(pos.x + x, pos.y + y, pos.z + z, oct, frq, amp);

                    globalHeightFunction = AnimationCurve.Linear(0, startHeight + n, 1, 0);

                    field[x + y * rS + z * rS * rS] = globalHeightFunction.Evaluate(t);
                }
    }
    public void Attract(Rigidbody rb)
    {
        Vector3 gravityUp = (rb.position - c).normalized;
        Vector3 localUp = rb.transform.up;

        rb.AddForce(gravityUp * gravity);
        rb.rotation = Quaternion.FromToRotation(localUp, gravityUp) * rb.rotation;
    }

    //--Private Methods--
    private int GetSeed()
    {
        return (seed == "") ? Random.Range(int.MinValue, int.MaxValue) : seed.GetHashCode();
    }

    void OnDrawGizmos()
    {
        if (!debugMode)
            return;

        if(showField)
        {
            Gizmos.color = Color.yellow;

            Vector3 pF = new Vector3(num * size / 2, num * size / 2, num * size / 2);
            Vector3 sF = new Vector3(num * size, num * size, num * size);

            Gizmos.DrawWireCube(pF, sF);
        }

        if(showChunks)
        {
            Gizmos.color = Color.cyan;

            for (int x = 0; x < num; x++)
                for (int y = 0; y < num; y++)
                    for (int z = 0; z < num; z++)
                    {
                        Vector3 pC = new Vector3(x * size + (size / 2), y * size + (size / 2), z * size + (size / 2));
                        Vector3 sC = new Vector3(size, size, size);

                        Gizmos.DrawWireCube(pC, sC);
                    }
        }

        if(debugChunks && chunks != null)
        {
            Gizmos.color = Color.red;

            foreach (Chunk chunk in chunks)
                chunk.Debug();
        }
    }

}
