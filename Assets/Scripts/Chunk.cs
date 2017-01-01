using UnityEngine;
using System;
using System.Collections.Generic;

using UniRx;

public class Chunk
{
    //--Main Stuff--
    public GameObject obj;
    public Vector3 pos;
    public float[] field;

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
    private bool terrainIsReady;

    //--Constructor--
    public Chunk(Planet planet, string name, Vector3 pos, Transform par, int num, int size, Material mat, int layerMask)
    {
        obj = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider));
        obj.transform.position = pos;
        obj.transform.parent = par;
        obj.layer = layerMask;

        this.pos = pos;
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

        terrainIsReady = false;
    }

    //--Public Methods--
    public void Generate()
    {
        planet.GenerateEmptySurface(field, pos);

        //m = marchingCubes.CreateMesh(field);
        m.RecalculateNormals();

        mf.sharedMesh = m;
        mc.sharedMesh = m;
    }
    public void Build()
    {
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
        var msa = Observable.Start(() =>
        {
            marchingCubes.CreateMesh(field);

            System.Threading.Thread.Sleep(TimeSpan.FromSeconds(10));
            return 5;
        });

        Observable.WhenAll(msa).ObserveOnMainThread().Subscribe(x =>
        {
            ConvertMesh();
            terrainIsReady = true;
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
        Vector3 p = new Vector3(pos.x + size / 2, pos.y + size / 2, pos.z + size / 2);
        Vector3 s = new Vector3(size, size, size);

        if(terrainIsReady)
            Gizmos.DrawWireCube(p, s);
    }
}
