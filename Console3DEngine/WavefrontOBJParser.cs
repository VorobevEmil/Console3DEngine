using System.Globalization;
using System.Numerics;

namespace Console3DEngine;

public static class WavefrontOBJParser
{
    public static (Vector3[] vectors, int[][] faces) Parse(string path)
    {
        string file = File.ReadAllText(path);
        string[] lines = file.Split('\n');

        List<Vector3> vectors = new();
        List<int[]> faces = new();
        foreach (string line in lines)
        {
            string[] lineParts = line.Split(' ');
            if (line.Contains("v"))
            {
                float x = float.Parse(lineParts[1], CultureInfo.InvariantCulture);
                float y = float.Parse(lineParts[2], CultureInfo.InvariantCulture);
                float z = float.Parse(lineParts[3], CultureInfo.InvariantCulture);

                vectors.Add(new Vector3(x, y, z));
            }
            else if (line.Contains("f"))
            {
                int[] f = [int.Parse(lineParts[1]), int.Parse(lineParts[2]), int.Parse(lineParts[3])];
                faces.Add(f);
            }
        }

        return (vectors.ToArray(), faces.ToArray());
    }
}