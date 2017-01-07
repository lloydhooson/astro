using UnityEngine;

public class Tool : MonoBehaviour
{
    //--Main Stuff--
    public Planet planet;
    public LayerMask mask;
    public int size;

    //--Tool Settings--
    public float distance;
    public float range;
    public float strenght;
    public AnimationCurve brush;

    //--Private Stuff--
    private int rS;

    void Start()
    {
        rS = size + 1;
    }

    void Update()
    {
        //Add Terrain
        if (Input.GetMouseButton(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
            RaycastHit hit;

            if(Physics.Raycast(ray, out hit, Mathf.Infinity, mask))
                if(planet.chunkCache != null)
                    foreach(Chunk chunk in planet.chunkCache)
                    {
                        float deltaChunk = ((chunk.pos + new Vector3(size / 2, size / 2, size / 2)) - hit.point).magnitude;

                        if(deltaChunk < size)
                        {
                            for(int x = 0; x < rS; x++)
                                for(int y = 0; y < rS; y++)
                                    for(int z = 0; z < rS; z++)
                                    {
                                        float deltaField = ((chunk.pos + new Vector3(x, y, z)) - hit.point).magnitude;
                                        float t = 1f - 1f / (1f + deltaField);

                                        if (deltaField < range)
                                            chunk.field[x + y * rS + z * rS * rS] += brush.Evaluate(t) * strenght;
                                    }

                            chunk.Build();
                        }
                    }
        }

        //Subtract Terrain
        if (Input.GetMouseButton(1))
        {
            Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, mask))
                if (planet.chunkCache != null)
                    foreach (Chunk chunk in planet.chunkCache)
                    {
                        float deltaChunk = ((chunk.pos + new Vector3(size / 2, size / 2, size / 2)) - hit.point).magnitude;

                        if (deltaChunk < size)
                        {
                            for (int x = 0; x < rS; x++)
                                for (int y = 0; y < rS; y++)
                                    for (int z = 0; z < rS; z++)
                                    {
                                        float deltaField = ((chunk.pos + new Vector3(x, y, z)) - hit.point).magnitude;
                                        float t = 1f - 1f / (1f + deltaField);

                                        if (deltaField < range)
                                            chunk.field[x + y * rS + z * rS * rS] -= brush.Evaluate(t) * strenght;
                                    }

                            chunk.Build();
                        }
                    }
        }
    }
}
