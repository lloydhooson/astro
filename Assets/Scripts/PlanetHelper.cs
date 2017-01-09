using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class PlanetHelper
{
    public static void Save(int globalSize, string file, Chunk[] chunks)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        Dictionary<int, float[]> seperatedFields = new Dictionary<int, float[]>();

        byte[] seperatedFieldsBytes = null;

        foreach(Chunk chunk in chunks)
        {
            int idx = chunk.x + chunk.y * globalSize + chunk.z * globalSize * globalSize;
            seperatedFields.Add(idx, chunk.field);
        }

        using (MemoryStream stream = new MemoryStream())
        {
            formatter.Serialize(stream, seperatedFields);
            seperatedFieldsBytes = stream.ToArray();
        }

        File.WriteAllBytes(file, seperatedFieldsBytes);
    }
	
    public static void Load(int globalSize, string file, Chunk[] chunks)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        Dictionary<int, float[]> seperatedFields = new Dictionary<int, float[]>();

        byte[] seperatedFieldsBytes = File.ReadAllBytes(file);

        using (MemoryStream stream = new MemoryStream())
        {
            stream.Write(seperatedFieldsBytes, 0, seperatedFieldsBytes.Length);
            stream.Position = 0;

            seperatedFields = (Dictionary<int, float[]>)formatter.Deserialize(stream);
        }

        foreach(Chunk chunk in chunks)
        {
            int idx = chunk.x + chunk.y * globalSize + chunk.z * globalSize * globalSize;
            float[] field = null;

            if (seperatedFields.TryGetValue(idx, out field))
                if (field != null)
                    chunk.field = field;

            bool hasValuesAboveBorder = false;
            bool hasValuesBelowBorder = false;

            for (int i = 0; i < chunk.field.Length; i++)
                if (chunk.field[i] >= 0.5)
                    hasValuesAboveBorder = true;
                else
                    hasValuesBelowBorder = true;

            chunk.performTriangulation = (hasValuesAboveBorder && hasValuesBelowBorder);
        }
    }
}
