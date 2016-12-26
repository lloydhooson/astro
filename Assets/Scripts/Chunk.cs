using UnityEngine;

public class Chunk
{
    public GameObject obj;
    public Vector3 pos;
    public float[] field;

    private Planet planet;
    private MarchingCubes marchingCubes;

    private Mesh m;
    private MeshFilter mf;
    private MeshRenderer mr;
    private MeshCollider mc;

    private int num, size, realSize;

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
    }

    public void Build()
    {
        planet.GenerateEmptySurface(field, pos);

        m = marchingCubes.CreateMesh(field);
        m.RecalculateNormals();

        mf.sharedMesh = m;
        mc.sharedMesh = m;
    }

    public void Debug()
    {
        Vector3 p = new Vector3(pos.x + size / 2, pos.y + size / 2, pos.z + size / 2);
        Vector3 s = new Vector3(size, size, size);

        Gizmos.DrawWireCube(p, s);
    }
}
