using TypeOEngine.Typedeaf.Core.Common;

namespace TypeOEngine.Typedeaf.Basic2d.Engine.Graphics
{
    /// <summary>
    /// Splits simple polygons into triangles using ear clipping.
    /// </summary>
    public static class PolygonTriangulator
    {
        /// <summary>
        /// Triangulates a simple polygon, in either winding order.
        /// </summary>
        /// <param name="points">The polygon vertices in order.</param>
        /// <returns>Vertex indices into <paramref name="points"/>, three per triangle. Empty when there are fewer than three points.</returns>
        public static int[] Triangulate(IReadOnlyList<Vec2> points)
        {
            var count = points?.Count ?? 0;
            if (count < 3)
                return Array.Empty<int>();

            var remaining = new List<int>(count);
            for (var i = 0; i < count; i++)
                remaining.Add(i);

            var clockwise = SignedArea(points) < 0;
            var result = new List<int>((count - 2) * 3);

            while (remaining.Count > 3)
            {
                var earFound = false;
                for (var i = 0; i < remaining.Count; i++)
                {
                    var prev = remaining[(i + remaining.Count - 1) % remaining.Count];
                    var curr = remaining[i];
                    var next = remaining[(i + 1) % remaining.Count];

                    if (!IsConvex(points[prev], points[curr], points[next], clockwise))
                        continue;
                    if (ContainsOtherPoint(points, remaining, prev, curr, next))
                        continue;

                    result.Add(prev);
                    result.Add(curr);
                    result.Add(next);
                    remaining.RemoveAt(i);
                    earFound = true;
                    break;
                }

                // Self-intersecting or degenerate input: fall back to a fan so something is still drawn
                if (!earFound)
                {
                    for (var i = 1; i < remaining.Count - 1; i++)
                    {
                        result.Add(remaining[0]);
                        result.Add(remaining[i]);
                        result.Add(remaining[i + 1]);
                    }
                    return result.ToArray();
                }
            }

            result.Add(remaining[0]);
            result.Add(remaining[1]);
            result.Add(remaining[2]);
            return result.ToArray();
        }

        private static double SignedArea(IReadOnlyList<Vec2> points)
        {
            double area = 0;
            for (var i = 0; i < points.Count; i++)
            {
                var a = points[i];
                var b = points[(i + 1) % points.Count];
                area += a.X * b.Y - b.X * a.Y;
            }
            return area / 2;
        }

        private static double Cross(Vec2 a, Vec2 b, Vec2 c)
        {
            return (b.X - a.X) * (c.Y - b.Y) - (b.Y - a.Y) * (c.X - b.X);
        }

        private static bool IsConvex(Vec2 a, Vec2 b, Vec2 c, bool clockwise)
        {
            var cross = Cross(a, b, c);
            return clockwise ? cross < 0 : cross > 0;
        }

        private static bool ContainsOtherPoint(IReadOnlyList<Vec2> points, List<int> remaining, int a, int b, int c)
        {
            foreach (var index in remaining)
            {
                if (index == a || index == b || index == c)
                    continue;
                if (InsideTriangle(points[index], points[a], points[b], points[c]))
                    return true;
            }
            return false;
        }

        private static bool InsideTriangle(Vec2 p, Vec2 a, Vec2 b, Vec2 c)
        {
            var d1 = Cross(a, b, p);
            var d2 = Cross(b, c, p);
            var d3 = Cross(c, a, p);
            var hasNegative = d1 < 0 || d2 < 0 || d3 < 0;
            var hasPositive = d1 > 0 || d2 > 0 || d3 > 0;
            return !(hasNegative && hasPositive);
        }
    }
}
