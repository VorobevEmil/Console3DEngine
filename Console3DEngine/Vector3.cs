namespace Console3DEngine;

public struct Vector3
{
    public float X;
    public float Y;
    public float Z;

    public static Vector3 Zero => new(0, 0, 0);
    public static Vector3 One => new(1, 1, 1);
    public static Vector3 UnitX => new(1, 0, 0);
    public static Vector3 UnitY => new(0, 1, 0);
    public static Vector3 UnitZ => new(0, 0, 1);

    public Vector3(float x, float y, float z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public float Length => MathF.Sqrt(X * X + Y * Y + Z * Z);
    public float LengthSquared => X * X + Y * Y + Z * Z;
    public Vector3 Normalized => this / Length;

    public static Vector3 operator +(Vector3 a, Vector3 b) => new(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
    public static Vector3 operator -(Vector3 a, Vector3 b) => new(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
    public static Vector3 operator -(Vector3 v) => new(-v.X, -v.Y, -v.Z);
    public static Vector3 operator *(Vector3 v, float scalar) => new(v.X * scalar, v.Y * scalar, v.Z * scalar);
    public static Vector3 operator *(float scalar, Vector3 v) => v * scalar;
    public static Vector3 operator /(Vector3 v, float scalar) => new(v.X / scalar, v.Y / scalar, v.Z / scalar);

    public static bool operator ==(Vector3 a, Vector3 b) =>
        MathF.Abs(a.X - b.X) < 1e-6f &&
        MathF.Abs(a.Y - b.Y) < 1e-6f &&
        MathF.Abs(a.Z - b.Z) < 1e-6f;

    public static bool operator !=(Vector3 a, Vector3 b) => !(a == b);

    public static float Dot(Vector3 a, Vector3 b) => a.X * b.X + a.Y * b.Y + a.Z * b.Z;

    public static Vector3 Cross(Vector3 a, Vector3 b) =>
        new(a.Y * b.Z - a.Z * b.Y,
            a.Z * b.X - a.X * b.Z,
            a.X * b.Y - a.Y * b.X);

    public static float Distance(Vector3 a, Vector3 b) => (a - b).Length;

    public override bool Equals(object? obj) => obj is Vector3 v && this == v;
    public override int GetHashCode() => HashCode.Combine(X, Y, Z);
    public override string ToString() => $"({X:0.###}, {Y:0.###}, {Z:0.###})";
}