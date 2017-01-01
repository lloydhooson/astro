

public class TVector3
{
    public float X { get; set; }
    public float Y { get; set; }
    public float Z { get; set; }

    public TVector3()
    {
        X = 0;
        Y = 0;
        Z = 0;
    }

    public TVector3(float x, float y, float z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public void SetZero()
    {
        X = 0;
        Y = 0;
        Z = 0;
    }
}