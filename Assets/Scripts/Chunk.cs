using UnityEngine;
using System;
using System.Collections.Generic;

using UniRx;

public class Chunk
{
    //--Obj Ref--
    public GameObject obj;

    //--Scalar Field--
    public bool performTriangulation;
    public float[] field;

    //--Neighbour Chunks--
    public List<Chunk> neighbourChunks;

    //--Planet Ref--
    private Planet planet;

    //--Triangulation--
    private MarchingCubes marchingCubes;

    //--Mesh--
    private List<Vector3> vertBuffer;
    private Mesh m;
    private MeshFilter mf;
    private MeshRenderer mr;
    private MeshCollider mc;

    //--Private Stuff
    private int num, size, realSize;
    public int x, y, z;

    //--Constructor--
    public Chunk(Planet planet, Transform par, int num, int size, int x, int y, int z, Material mat, int layerMask)
    {
        obj = new GameObject(string.Format("{0} / {1} / {2}", x, y, z), typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider));
        obj.transform.position = new Vector3(x * size, y * size, z * size);
        obj.transform.parent = par;
        obj.layer = layerMask;

        neighbourChunks = new List<Chunk>();

        performTriangulation = true;
        field = new float[(size + 1) * (size + 1) * (size + 1)];

        this.planet = planet;
        marchingCubes = new MarchingCubes(size, 0.5f, new int[] { 0, 1, 2 });

        vertBuffer = new List<Vector3>();

        m = new Mesh();
        mf = obj.GetComponent<MeshFilter>();
        mr = obj.GetComponent<MeshRenderer>();
        mc = obj.GetComponent<MeshCollider>();

        m.Clear();
        mf.sharedMesh = null;
        mr.material = mat;
        mc.sharedMesh = null;

        this.num = num;
        this.size = size;

        this.x = x;
        this.y = y;
        this.z = z;
    }

    //--Public Methods--
    public void SetNeighbourChunks()
    {
        for (int ix = x - 1; ix <= x + 1; ix++)
            for (int iy = y - 1; iy <= y + 1; iy++)
                for (int iz = z - 1; iz <= z + 1; iz++)
                    if (ix >= 0 && iy >= 0 && iz >= 0 && ix < num && iy < num && iz < num)
                        neighbourChunks.Add(planet.chunks[ix + iy * num + iz * num * num]);
    }
    public Vector3 GetPosition()
    {
        return new Vector3(x * size, y * size, z * size);
    }
    public Vector3 GetCenter()
    {
        return new Vector3(x * size + (size / 2), y * size + size / 2, z * size + size / 2);
    }
    public void Generate()
    {
        field = new float[(size + 1) * (size + 1) * (size + 1)];
        planet.GenerateEmptySurface(this);
    }
    public void Build()
    {
        if (!performTriangulation)
            return;

        marchingCubes.CreateMesh(field);

        vertBuffer.Clear();
        for (int i = 0; i < marchingCubes.mb.vertices.Count; i++)
        {
            TVector3 vert = marchingCubes.mb.vertices[i];
            vertBuffer.Add(new Vector3(vert.X, vert.Y, vert.Z));
        }

        m.Clear();
        m.SetVertices(vertBuffer);
        m.SetTriangles(marchingCubes.mb.triangles, 0);
        m.RecalculateNormals();

        mf.sharedMesh = m;
        mc.sharedMesh = m;
    }
    public void BuildAsync()
    {
        if (!performTriangulation)
            return;

        var msa = Observable.Start(() =>
        {
            marchingCubes.CreateMesh(field);

            System.Threading.Thread.Sleep(TimeSpan.FromSeconds(5));
            return 5;
        });

        Observable.WhenAll(msa).ObserveOnMainThread().Subscribe(x =>
        {
            ConvertMesh();
        });
    }
    public void ConvertMesh()
    {
        vertBuffer.Clear();
        for(int i = 0; i < marchingCubes.mb.vertices.Count; i++)
        {
            TVector3 vert = marchingCubes.mb.vertices[i];
            vertBuffer.Add(new Vector3(vert.X, vert.Y, vert.Z));
        }

        m.Clear();
        m.SetVertices(vertBuffer);
        m.SetTriangles(marchingCubes.mb.triangles, 0);
        m.RecalculateNormals();

        mf.sharedMesh = m;
        mc.sharedMesh = m;
    }
    public void Debug()
    {
        Vector3 p = new Vector3(x * size + size / 2, y * size + size / 2, z * size + size / 2);
        Vector3 s = new Vector3(size, size, size);

        if(performTriangulation)
            Gizmos.DrawWireCube(p, s);
    }
}
