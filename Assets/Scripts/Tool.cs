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

    //--Other Stuff--
    private Chunk sourceChunk;
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
            {
                sourceChunk = planet.GetChunkByGameObject(hit.transform.gameObject);

                if(sourceChunk != null)
                {
                    foreach (Chunk chunk in sourceChunk.neighbourChunks)
                    {
                        float deltaChunk = (chunk.GetCenter() - hit.point).magnitude;

                        if (deltaChunk < size)
                        {
                            bool performTriangulation = false;

                            for (int x = 0; x < rS; x++)
                                for (int y = 0; y < rS; y++)
                                    for (int z = 0; z < rS; z++)
                                    {
                                        float deltaField = ((chunk.GetPosition() + new Vector3(x, y, z)) - hit.point).magnitude;
                                        float t = 1f - 1f / (1f + deltaField);
                                        float vol = brush.Evaluate(t) * strenght;


                                        if (deltaField < range)
                                        {
                                            chunk.field[x + y * rS + z * rS * rS] += vol;

                                            if (chunk.field[x + y * rS + z * rS * rS] >= 0.5f)
                                                performTriangulation = true;
                                        }
                                    }

                            chunk.performTriangulation = performTriangulation;
                            chunk.Build();
                        }
                    }
                }
            }
        }

        //Subtract Terrain
        if (Input.GetMouseButton(1))
        {
            Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, mask))
            {
                sourceChunk = planet.GetChunkByGameObject(hit.transform.gameObject);

                if (sourceChunk != null)
                {
                    foreach (Chunk chunk in sourceChunk.neighbourChunks)
                    {
                        float deltaChunk = (chunk.GetCenter() - hit.point).magnitude;

                        if (deltaChunk < size)
                        {
                            bool performTriangulation = false;

                            for (int x = 0; x < rS; x++)
                                for (int y = 0; y < rS; y++)
                                    for (int z = 0; z < rS; z++)
                                    {
                                        float deltaField = ((chunk.GetPosition() + new Vector3(x, y, z)) - hit.point).magnitude;
                                        float t = 1f - 1f / (1f + deltaField);
                                        float vol = brush.Evaluate(t) * strenght;

                                        if (deltaField < range)
                                        {
                                            chunk.field[x + y * rS + z * rS * rS] -= vol;

                                            if (chunk.field[x + y * rS + z * rS * rS] >= 0.5f)
                                                performTriangulation = true;
                                        }
                                    }

                            chunk.performTriangulation = performTriangulation;
                            chunk.Build();
                        }
                    }
                }
            }
        }

        if (Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1))
            sourceChunk = null;
    }
}
