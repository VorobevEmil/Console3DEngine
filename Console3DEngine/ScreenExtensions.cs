namespace Console3DEngine;

public static class ScreenExtensions
{
    public static void DrawLine(this char[,] screen,
        int x0, int y0, int x1, int y1, char ch = '*')
    {
        int height = screen.GetLength(0);
        int width = screen.GetLength(1);

        int dx = Math.Abs(x1 - x0);
        int sx = x0 < x1 ? 1 : -1;
        int dy = -Math.Abs(y1 - y0);
        int sy = y0 < y1 ? 1 : -1;
        int err = dx + dy;

        while (true)
        {
            if (x0 >= 0 && x0 < width && y0 >= 0 && y0 < height)
            {
                screen[y0, x0] = ch;
            }

            if (x0 == x1 && y0 == y1)
                break;

            int e2 = 2 * err;
            if (e2 >= dy)
            {
                err += dy;
                x0 += sx;
            }

            if (e2 <= dx)
            {
                err += dx;
                y0 += sy;
            }
        }
    }
}