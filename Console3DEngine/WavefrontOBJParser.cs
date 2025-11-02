namespace Console3DEngine;

public static class WavefrontOBJParser
{
    public static (Vector3[] vectors, (int a, int b, int c)[] faces) Parse(string path)
    {
        string file = File.ReadAllText(path);
        string[] lines = file.Split('\n');

        List<Vector3> vectors = new();
        List<(int a, int b, int c)> faces = new();
        foreach (string line in lines)
        {
            if (line.Contains("v"))
            {
                string[] vectorParts = line.Split(' ');
                vectors.Add(new Vector3(float.Parse(vectorParts[1]), float.Parse(vectorParts[2]),
                    float.Parse(vectorParts[3])));
            }
            else if (line.Contains("f"))
            {
                string[] faceParts = line.Split(' ');
                faces.Add((int.Parse(faceParts[1]) - 1, int.Parse(faceParts[2]) - 1, int.Parse(faceParts[3]) - 1));
            }
        }

        return (vectors.ToArray(), faces.ToArray());
    }
}