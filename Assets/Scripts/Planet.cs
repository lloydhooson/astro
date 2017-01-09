using UnityEngine;
using System.Collections.Generic;
using System.Diagnostics;

public class Planet : MonoBehaviour
{
    //--Debug--
    public bool debugMode;
    public bool showField, showChunks;
    public bool debugChunks;
    public bool debugCache;

    //--Main Switches--
    public bool enableFog;

    //--Main Stuff--
    public Transform player, fog;
    public Material mat;
    public int layerMask;
    public int num, size;
    private int rS;
    public int startHeight;
    private AnimationCurve globalHeightFunction;
    private Vector3 c;

    //--Noise--
    public NOISETYPE noiseType;
    public enum NOISETYPE { FRACTAL, TURBULENT, RIDGEDMULTIFRACTAL };
    public ComputeShader noiseComputer;
    private ComputeBuffer noiseComputeBuffer;
    private float[] noiseBuffer;
    private PerlinNoise noise;
    public string seed;
    public float frequency, lacunarity, gain, amplitude;

    //--Gravity--
    public float gravity;

    //--Chunks--
    public Chunk[] chunks;

    //--Cache--
    public int radius;
    private Vector3 worldPos;
    private HashSet<Vector3> nearChunkLocations;

    //--Diagnostics--
    private Stopwatch watch;

    void Start()
    {
        rS = size + 1;

        int m = (num / 2) * size + (size / 2);
        c = new Vector3(m, m, m);

        if(enableFog)
        {
            fog.position = c;
            fog.localScale = new Vector3(num * size, num * size, num * size);
        }

        chunks = new Chunk[num * num * num];

        noise = new PerlinNoise(GetSeed());
        noise.LoadResourcesFor3DNoise();

        noiseBuffer = new float[rS * rS * rS];

        //Create Chunks
        for (int x = 0; x < num; x++)
            for (int y = 0; y < num; y++)
                for (int z = 0; z < num; z++)
                    chunks[x + y * num + z * num * num] = new Chunk(this, transform, num, size, x, y, z, mat, layerMask);

        //Setup Chunks
        for (int x = 0; x < num; x++)
            for (int y = 0; y < num; y++)
                for (int z = 0; z < num; z++)
                    chunks[x + y * num + z * num * num].SetNeighbourChunks();

        worldPos = GetTargetPosition(player.position);
        nearChunkLocations = new HashSet<Vector3>();

        watch = new Stopwatch();
    }

    void Update()
    {
        Vector3 p = player.position;
        bool moveFlag = false;

        if      (p.x < worldPos.x)
            moveFlag = true;
        else if (p.x > worldPos.x + size)
            moveFlag = true;
        else if (p.y < worldPos.y)
            moveFlag = true;
        else if (p.y > worldPos.y + size)
            moveFlag = true;
        else if (p.z < worldPos.z)
            moveFlag = true;
        else if (p.z > worldPos.z + size)
            moveFlag = true;

        if(moveFlag)
        {
            worldPos = GetTargetPosition(player.position);
            nearChunkLocations = GetPositionsInRadius();
        }
    }

    //--Public Methods--
    public void Generate()
    {
        watch.Start();

        if(chunks != null)
            foreach (Chunk chunk in chunks)
                chunk.Generate();

        watch.Stop();
        UnityEngine.Debug.Log(string.Format("Noise Generation: {0}ms", watch.ElapsedMilliseconds));
        watch.Reset();
    }
    public void Build()
    {
        watch.Start();

        if (chunks != null)
            foreach (Chunk chunk in chunks)
                chunk.Build();

        watch.Stop();
        UnityEngine.Debug.Log(string.Format("Triangulation: {0}ms", watch.ElapsedMilliseconds));
        watch.Reset();
    }
    public void BuildAsync()
    {
        if (chunks != null)
            foreach (Chunk chunk in chunks)
                chunk.BuildAsync();
    }
    public void GenerateEmptySurface(Chunk chunk)
    {
        noiseComputeBuffer = new ComputeBuffer(rS * rS * rS, sizeof(float));

        noiseComputer.SetInt("_Width", rS);
        noiseComputer.SetInt("_Height", rS);
        noiseComputer.SetInt("_Xoff", chunk.x * size);
        noiseComputer.SetInt("_Yoff", chunk.y * size);
        noiseComputer.SetInt("_Zoff", chunk.z * size);
        noiseComputer.SetInt("_NoiseType", GetNoiseType());
        noiseComputer.SetFloat("_Frequency", frequency);
        noiseComputer.SetFloat("_Lacunarity", lacunarity);
        noiseComputer.SetFloat("_Gain", gain);
        noiseComputer.SetTexture(0, "_PermTable", noise.GetPermutationTable2D());
        noiseComputer.SetTexture(0, "_Gradient", noise.GetGradient3D());
        noiseComputer.SetBuffer(0, "_Result", noiseComputeBuffer);

        noiseComputer.Dispatch(0, rS / 8, rS / 8, rS / 8);
        noiseComputeBuffer.GetData(noiseBuffer);
        noiseComputeBuffer.Release();

        bool hasValuesAboveBorder = false;
        bool hasValuesBelowBorder = false;

        for (int x = 0; x < rS; x++)
            for (int y = 0; y < rS; y++)
                for (int z = 0; z < rS; z++)
                {
                    Vector3 p = chunk.GetPosition() + new Vector3(x, y, z);
                    float d = (c - p).magnitude;
                    float t = 1f - 1f / (1f + d);

                    float n = noiseBuffer[x + y * rS + z * rS * rS] * amplitude;
                    globalHeightFunction = AnimationCurve.Linear(0, startHeight + n, 1, 0);
                    float vol = globalHeightFunction.Evaluate(t);

                    if (vol >= 0.5)
                        hasValuesAboveBorder = true;
                    else
                        hasValuesBelowBorder = true;

                    chunk.field[x + y * rS + z * rS * rS] = vol;
                }

        chunk.performTriangulation = (hasValuesAboveBorder && hasValuesBelowBorder);
    }
    public void Attract(Rigidbody rb)
    {
        Vector3 gravityUp = (rb.position - c).normalized;
        Vector3 localUp = rb.transform.up;

        rb.AddForce(gravityUp * gravity);
        rb.rotation = Quaternion.FromToRotation(localUp, gravityUp) * rb.rotation;
    }
    public Chunk GetChunkByGameObject(GameObject obj)
    {
        foreach (Chunk chunk in chunks)
            if (chunk.obj.Equals(obj))
                return chunk;

        return null;
    }

    //--Private Methods--
    private int GetSeed()
    {
        return (seed == "") ? Random.Range(int.MinValue, int.MaxValue) : seed.GetHashCode();
    }
    private int GetNoiseType()
    {
        switch(noiseType)
        {
            case NOISETYPE.FRACTAL: return 0;
            case NOISETYPE.TURBULENT: return 1;
            case NOISETYPE.RIDGEDMULTIFRACTAL: return 2;
            default: return 0;
        }
    }
    private Vector3 GetTargetPosition(Vector3 pos)
    {
        int x = (int)Mathf.Floor(pos.x / size) * size;
        int y = (int)Mathf.Floor(pos.y / size) * size;
        int z = (int)Mathf.Floor(pos.z / size) * size;

        return new Vector3(x, y, z);
    }
    private HashSet<Vector3> GetPositionsInRadius()
    {
        HashSet<Vector3> results = new HashSet<Vector3>();

        for (int x = -radius; x <= radius; x++)
            for (int y = -radius; y <= radius; y++)
                for (int z = -radius; z <= radius; z++)
                    if (x * x + y * y + z * z < radius * radius)
                        results.Add(new Vector3(worldPos.x + (x * size), worldPos.y + (y * size), worldPos.z + (z * size)));

        return results;
    }

    void OnDrawGizmos()
    {
        if (!debugMode)
            return;

        if(debugCache && nearChunkLocations != null)
        {
            Gizmos.color = Color.green;

            foreach(Vector3 pos in nearChunkLocations)
            {
                Vector3 pR = pos + new Vector3(size / 2, size / 2, size / 2);
                Vector3 sR = new Vector3(size, size, size);

                Gizmos.DrawWireCube(pR, sR);
            }
        }

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
