using Console3DEngine;

Console.CursorVisible = false;

float camYawDeg = 0f;
Vector3 camPos = new(0, 0, 3f);

(Vector3[] modelInit, (int a, int b, int c)[] faces) = WavefrontOBJParser.Parse("teapot.obj");
(int, int)[] edges = faces.SelectMany(f => new[] { (f.a, f.b), (f.b, f.c), (f.c, f.a) }).ToArray();

while (true)
{
    int h = Console.WindowHeight - 1;
    int w = Console.WindowWidth;
    char[,] board = new char[h, w];
    for (int y = 0; y < h; y++)
    for (int x = 0; x < w; x++)
        board[y, x] = ' ';

    float originX = w / 2f, originY = h / 2f;

    // --- VIEW: p_view = R_cam^T * (p_world - camPos)
    float yaw = camYawDeg * MathF.PI / 180f;
    float cy = MathF.Cos(yaw), sy = MathF.Sin(yaw);

    float[,] Rcam =
    {
        { cy, 0f, sy },
        { 0f, 1f, 0f },
        { -sy, 0f, cy }
    };

    float[,] RcamT =
    {
        { cy, 0f, -sy },
        { 0f, 1f, 0f },
        { sy, 0f, cy }
    };

    Vector3 Mul(float[,] m, Vector3 v)
        => new(m[0, 0] * v.X + m[0, 1] * v.Y + m[0, 2] * v.Z,
            m[1, 0] * v.X + m[1, 1] * v.Y + m[1, 2] * v.Z,
            m[2, 0] * v.X + m[2, 1] * v.Y + m[2, 2] * v.Z);

    Vector3[] world = modelInit;

    Vector3[] view = world
        .Select(p => Mul(RcamT, p - camPos)).ToArray();

    // --- Перспектива
    float near = 0.1f;
    float fov = 60f * MathF.PI / 180f;
    float scale = 0.5f * w / MathF.Tan(fov / 2f);
    float aspectFixY = 0.6f;

    (int x, int y)?[] screenPts = new (int, int)?[view.Length];
    for (int i = 0; i < view.Length; i++)
    {
        Vector3 p = view[i];
        if (p.Z > near)
        {
            float xs = originX + scale * (p.X / p.Z);
            float ys = originY - scale * aspectFixY * (p.Y / p.Z);
            screenPts[i] = ((int)MathF.Round(xs), (int)MathF.Round(ys));
        }
        else
        {
            screenPts[i] = null;
        }
    }

    foreach (var (a, b) in edges)
    {
        (int x, int y)? pa = screenPts[a];
        (int x, int y)? pb = screenPts[b];
        if (pa.HasValue && pb.HasValue)
        {
            DrawLineBresenham(pa.Value.x, pa.Value.y, pb.Value.x, pb.Value.y, '*');
        }
    }

    Console.SetCursorPosition(0, 0);
    for (int y = 0; y < h; y++)
    {
        for (int x = 0; x < w; x++) Console.Write(board[y, x]);
        if (y < h - 1) Console.Write('\n');
    }

    if (Console.KeyAvailable)
    {
        ConsoleKey key = Console.ReadKey(true).Key;
        Vector3 forward = Mul(Rcam, new Vector3(0, 0, 1));
        Vector3 right = Mul(Rcam, new Vector3(1, 0, 0));
        forward = new Vector3(forward.X, 0, forward.Z).Normalized;

        float step = 0.5f;
        switch (key)
        {
            case ConsoleKey.W: camPos += step * forward; break;
            case ConsoleKey.S: camPos -= step * forward; break;
            case ConsoleKey.A: camPos -= step * right; break;
            case ConsoleKey.D: camPos += step * right; break;
            case ConsoleKey.Q: camPos += new Vector3(0, -step, 0); break;
            case ConsoleKey.E: camPos += new Vector3(0, step, 0); break;
            case ConsoleKey.LeftArrow: camYawDeg -= 3f; break;
            case ConsoleKey.RightArrow: camYawDeg += 3f; break;
        }

        while (Console.KeyAvailable) Console.ReadKey(true);
    }

    void DrawLineBresenham(int x0, int y0, int x1, int y1, char ch = '*')
    {
        int W = board.GetLength(1), H = board.GetLength(0);
        bool steep = Math.Abs(y1 - y0) > Math.Abs(x1 - x0);
        if (steep)
        {
            (x0, y0, x1, y1) = (y0, x0, y1, x1);
        }

        if (x0 > x1)
        {
            (x0, x1) = (x1, x0);
            (y0, y1) = (y1, y0);
        }

        int dx = x1 - x0, dy = Math.Abs(y1 - y0), err = dx / 2, ystep = y0 < y1 ? 1 : -1;
        int y = y0;
        for (int x = x0; x <= x1; x++)
        {
            int px = steep ? y : x, py = steep ? x : y;
            if (0 <= px && px < W && 0 <= py && py < H) board[py, px] = ch;
            err -= dy;
            if (err < 0)
            {
                y += ystep;
                err += dx;
            }
        }
    }
}