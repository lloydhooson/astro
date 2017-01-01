using System.Collections.Generic;

public class MeshBuffer
{
    public List<TVector3> vertices;
    public List<int> triangles;

    public MeshBuffer()
    {
        vertices = new List<TVector3>();
        triangles = new List<int>();
    }

    public void Clear()
    {
        vertices.Clear();
        triangles.Clear();
    }
}
