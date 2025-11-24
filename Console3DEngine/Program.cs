using System.Numerics;
using Console3DEngine;

Console.CursorVisible = false;
int height = Console.WindowHeight - 1;
int width = Console.WindowWidth;
float originX = (float)width / 2;
float originY = (float)height / 2;
Vector3[] cube =
[
    new(-1, -1, -1),
    new(1, -1, -1),
    new(1, 1, -1),
    new(-1, 1, -1),

    new(-1, -1, 1),
    new(1, -1, 1),
    new(1, 1, 1),
    new(-1, 1, 1),
];

int[][] cubeFaces =
[
    // Front (z = -1) – смотрит на камеру
    [1, 3, 2],
    [1, 4, 3],

    // Back (z = +1)
    [5, 6, 7],
    [5, 7, 8],

    // Left (x = -1)
    [1, 5, 8],
    [1, 8, 4],

    // Right (x = +1)
    [2, 3, 7],
    [2, 7, 6],

    // Top (y = +1)
    [4, 8, 7],
    [4, 7, 3],

    // Bottom (y = -1)
    [1, 2, 6],
    [1, 6, 5]
];


Vector3 cubePos = new(0, 0, 0);

Vector3[] world =
{
    new(-5, -2, -5),
    new(5, -2, -5),
    new(-5, -2, 5),
    new(5, -2, 5)
};

int[][] worldFaces =
[
    [1, 3, 2],
    [3, 4, 2]
];


// (Vector3[] world, int[][] faces) = WavefrontOBJParser.Parse("teapot.obj");


Vector3 camPos = new(0, 0.0f, -6f);

const float nearP = 0.1f;
const float farP = 50.0f;
const float fov = MathF.PI / 3f; // 60°
float f = 0.5f * width / MathF.Tan(fov / 2f);

float angleOfFigure = 1 * MathF.PI / 180;

float[,] rotateX = GetRotateX(angleOfFigure);
float[,] rotateY = GetRotateY(angleOfFigure);
float[,] rotateZ = GetRotateZ(angleOfFigure);
float[,] rotateYCamWorld = GetRotateY(0); // для движения (cam -> world)
float[,] rotateYView = GetRotateY(0); // для рендера (world -> cam)

float[,] rotateXView = GetRotateX(0); // для рендера (world -> cam)

_ = Task.Run(() =>
{
    float camYawDeg = 0;
    float camXawDeg = 0;

    while (true)
    {
        ConsoleKey key = Console.ReadKey(true).Key;

        if (key is ConsoleKey.LeftArrow or ConsoleKey.RightArrow or ConsoleKey.UpArrow or ConsoleKey.DownArrow)
        {
            camYawDeg += key switch
            {
                ConsoleKey.LeftArrow => 10f,
                ConsoleKey.RightArrow => -10f,
                _ => 0f
            };

            float camXawDegK = camXawDeg + key switch
            {
                ConsoleKey.UpArrow => 10f,
                ConsoleKey.DownArrow => -10f,
                _ => 0f
            };

            float yawRad = ToRadians(camYawDeg);

            // поворот камеры в мире (куда смотрит)
            rotateYCamWorld = GetRotateY(yawRad);

            // поворот мира в координаты камеры (view) — угол с минусом
            rotateYView = GetRotateY(-yawRad);


            if (camXawDegK is > -90 and <= 90)
            {
                camXawDeg = camXawDegK;
                float xawRad = ToRadians(camXawDeg);

                // поворот мира в координаты камеры (view) — угол с минусом
                rotateXView = GetRotateX(-xawRad);
            }
        }

        // считаем forward/right в мировой системе координат
        Vector3 forward = Mul(rotateYCamWorld, new Vector3(0, 0, 1));
        Vector3 right = Mul(rotateYCamWorld, new Vector3(1, 0, 0));

        // ходим только по земле
        forward = forward with { Y = 0 };
        right = right with { Y = 0 };

        if (forward.LengthSquared() > 0)
            forward /= forward.Length();

        if (right.LengthSquared() > 0)
            right /= right.Length();

        float step = 0.5f;

        switch (key)
        {
            case ConsoleKey.W: camPos += step * forward; break;
            case ConsoleKey.S: camPos -= step * forward; break;
            case ConsoleKey.A: camPos -= step * right; break;
            case ConsoleKey.D: camPos += step * right; break;
            case ConsoleKey.Q: camPos += new Vector3(0, -step, 0); break;
            case ConsoleKey.E: camPos += new Vector3(0, step, 0); break;
        }
    }
});

while (true)
{
    Console.Clear();
    char[,] screen = new char[height, width];
    for (int i = 0; i < height; i++)
    for (int j = 0; j < width; j++)
        screen[i, j] = ' ';

    // 1) Локальные повороты куба (в его собственной системе координат)
    Vector3[] cubeLocal = cube
        .Select(w => Mul(rotateX, w))
        .Select(w => Mul(rotateY, w))
        .Select(w => Mul(rotateZ, w))
        .ToArray();
    cube = cubeLocal;

    // 2) Перенос в мировые координаты
    Vector3[] cubeWorld = cubeLocal
        .Select(p => p + cubePos)
        .ToArray();

    // 3) Перевод мира в координаты камеры (view) — уже из cubeWorld
    Vector3[] viewCube = cubeWorld
        .Select(p => p - camPos)
        .Select(pCamPos => Mul(rotateYView, pCamPos))
        .Select(pCamPos => Mul(rotateXView, pCamPos))
        .ToArray();

    Vector3[] viewWorld = world
        .Select(p => p - camPos)
        .Select(pCamPos => Mul(rotateYView, pCamPos))
        .Select(pCamPos => Mul(rotateXView, pCamPos))
        .ToArray();


    RenderModel(viewWorld, worldFaces, screen, true);
    RenderModel(viewCube, cubeFaces, screen);


    for (int i = 0; i < height; i++)
    {
        for (int j = 0; j < width; j++)
        {
            Console.SetCursorPosition(j, i);
            Console.Write(screen[i, j]);
        }
    }

    Thread.Sleep(30);
}

Vector3 Mul(float[,] m, Vector3 v)
    => new(m[0, 0] * v.X + m[0, 1] * v.Y + m[0, 2] * v.Z,
        m[1, 0] * v.X + m[1, 1] * v.Y + m[1, 2] * v.Z,
        m[2, 0] * v.X + m[2, 1] * v.Y + m[2, 2] * v.Z);


float[,] GetRotateX(float angle) => new[,]
{
    { 1f, 0f, 0f },
    { 0f, MathF.Cos(angle), MathF.Sin(angle) },
    { 0f, -MathF.Sin(angle), MathF.Cos(angle) }
};

float[,] GetRotateY(float angle) => new[,]
{
    { MathF.Cos(angle), 0f, -MathF.Sin(angle) },
    { 0f, 1f, 0f },
    { MathF.Sin(angle), 0f, MathF.Cos(angle) }
};


float[,] GetRotateZ(float angle) => new[,]
{
    { MathF.Cos(angle), MathF.Sin(angle), 0f },
    { -MathF.Sin(angle), MathF.Cos(angle), 0f },
    { 0f, 0f, 1f }
};

float ToRadians(float degrees) => degrees * MathF.PI / 180f;

void RenderModel(Vector3[] model, int[][] faces,
    char[,] chars, bool twoSided = false)
{
    (int x, int y)?[] screenPts = new (int, int)?[model.Length];

    for (int i = 0; i < model.Length; i++)
    {
        Vector3 p = model[i];
        if (p.Z < nearP || p.Z > farP)
        {
            screenPts[i] = null;
            continue;
        }

        float xProj = f * (p.X / p.Z);
        float yProj = f * 0.4949f * (p.Y / p.Z);

        int sx = (int)(originX + Math.Round(xProj));
        int sy = (int)(originY - Math.Round(yProj));

        screenPts[i] = (sx, sy);
    }


    // === back-face culling ===
    List<(int a, int b)> visibleEdges = new();

    foreach (int[] face in faces)
    {
        int i0 = face[0] - 1;
        int i1 = face[1] - 1;
        int i2 = face[2] - 1;

        Vector3 v0 = model[i0];
        Vector3 v1 = model[i1];
        Vector3 v2 = model[i2];

        if (v0.Z <= 0 && v1.Z <= 0 && v2.Z <= 0)
            continue;

        Vector3 e1 = v1 - v0;
        Vector3 e2 = v2 - v0;
        Vector3 normal = Vector3.Cross(e1, e2);

        Vector3 center = (v0 + v1 + v2) / 3f;
        Vector3 viewDir = Vector3.Normalize(center);

        float d = Vector3.Dot(normal, viewDir);

        // back-face: нормаль смотрит от камеры
        if (!twoSided && d >= 0)
            continue;

        // Грань фронтальная → добавляем её рёбра
        visibleEdges.Add((face[0], face[1]));
        visibleEdges.Add((face[1], face[2]));
        visibleEdges.Add((face[2], face[0]));
    }

    foreach (var (a, b) in visibleEdges)
    {
        int ia = a - 1;
        int ib = b - 1;

        Vector3 va = model[ia];
        Vector3 vb = model[ib];

        if (va.Z < 0 || vb.Z < 0)
            continue;

        (int x, int y)? pa = screenPts[ia];
        (int x, int y)? pb = screenPts[ib];

        if (pa.HasValue && pb.HasValue)
        {
            chars.DrawLine(pa.Value.x, pa.Value.y, pb.Value.x, pb.Value.y);
        }
    }
}