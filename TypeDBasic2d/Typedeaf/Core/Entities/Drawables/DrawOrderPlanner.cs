using TypeD.Models.Data;
using TypeOEngine.Typedeaf.Core.Common;
using TypeOEngine.Typedeaf.Core.Entities.Interfaces;
using TypeOEngine.Typedeaf.TypeOBasic2d.Entities.Drawables;

namespace TypeDBasic2d.Typedeaf.Core.Entities.Drawables;

internal enum DrawOrderMove { Forward, Front, Backward, Back }

internal static class DrawOrderPlanner
{
    private sealed record Entry(IDrawable Drawable, Debug2DDrawable Overlay, Property Property, int Order);

    internal static List<ChangePropertyCommandData> Plan(Project project, Component component,
        IReadOnlyList<Debug2DDrawable> overlays, DrawOrderMove move)
    {
        var target = overlays.FirstOrDefault(overlay => ReferenceEquals(overlay.Component, component));
        if (project == null || component == null || component.ReadOnly || component.Removed
            || target?.TypeOObject is not Drawable2d drawable || drawable.Hidden || drawable.DrawStack == null)
            return null;

        var overlayByDrawable = overlays
            .Where(overlay => overlay.TypeOObject is IDrawable)
            .ToDictionary(overlay => (IDrawable)overlay.TypeOObject, overlay => overlay);
        var entries = drawable.DrawStack.GetDrawablesInInsertionOrder()
            .Select(item =>
            {
                overlayByDrawable.TryGetValue(item, out var overlay);
                var property = overlay?.Component?.Properties.FirstOrDefault(p => p.Name == nameof(IDrawable.DrawOrder));
                return new Entry(item, overlay, property, property?.Value is int value ? value : item.DrawOrder);
            })
            .OrderBy(entry => entry.Order)
            .ToList();
        var targetIndex = entries.FindIndex(entry => ReferenceEquals(entry.Drawable, drawable));
        if (targetIndex < 0 || !CanChange(entries[targetIndex]))
            return null;

        var forward = move is DrawOrderMove.Forward or DrawOrderMove.Front;
        var all = move is DrawOrderMove.Front or DrawOrderMove.Back;
        var indices = forward
            ? Enumerable.Range(targetIndex + 1, entries.Count - targetIndex - 1)
            : Enumerable.Range(0, targetIndex).Reverse();
        var candidates = indices.Where(index => entries[index].Overlay != null
            && entries[index].Drawable is Drawable2d { Hidden: false }
            && Overlaps(target, entries[index].Overlay)).ToList();
        if (candidates.Count == 0)
            return null;

        var neighbor = entries[all ? candidates[^1] : candidates[0]];
        var selected = entries[targetIndex];
        entries.RemoveAt(targetIndex);
        var insertion = entries.IndexOf(neighbor) + (forward ? 1 : 0);
        entries.Insert(insertion, selected);

        // Find the smallest editable interval with enough integer values between its untouched neighbors.
        for (var count = 1; count <= entries.Count; count++)
        {
            for (var left = Math.Max(0, insertion - count + 1); left <= Math.Min(insertion, entries.Count - count); left++)
            {
                var right = left + count - 1;
                var lower = left == 0 ? (long)int.MinValue - 1 : entries[left - 1].Order;
                var upper = right == entries.Count - 1 ? (long)int.MaxValue + 1 : entries[right + 1].Order;
                if (upper - lower - 1 < count)
                    continue;

                long? fixedStart = null;
                var valid = true;
                for (var i = left; i <= right; i++)
                {
                    if (CanChange(entries[i]))
                        continue;
                    var required = (long)entries[i].Order - (i - left);
                    if (fixedStart != null && fixedStart != required)
                    {
                        valid = false;
                        break;
                    }
                    fixedStart = required;
                }
                if (!valid)
                    continue;

                var start = fixedStart ?? Math.Clamp((long)entries[left].Order, lower + 1, upper - count);
                if (start <= lower || start + count - 1 >= upper)
                    continue;
                var changes = new List<ChangePropertyCommandData>();
                for (var i = left; i <= right; i++)
                {
                    var value = (int)(start + i - left);
                    if (entries[i].Order != value)
                        changes.Add(new ChangePropertyCommandData
                        {
                            Project = project,
                            Component = entries[i].Overlay.Component,
                            Property = entries[i].Property,
                            Value = value
                        });
                }
                if (changes.Count > 0)
                    return changes;
            }
        }
        return null;
    }

    private static bool CanChange(Entry entry) =>
        entry.Overlay?.Component is { ReadOnly: false, Removed: false } && entry.Property is { ReadOnly: false, Value: int };

    private static bool Overlaps(Debug2DDrawable first, Debug2DDrawable second)
    {
        var a = Corners(first);
        var b = Corners(second);
        return HasArea(a) && HasArea(b) && !Separated(a, b) && !Separated(b, a);
    }

    private static Vec2[] Corners(Debug2DDrawable drawable)
    {
        var (topLeft, topRight, bottomRight, bottomLeft) = drawable.GetWorldCorners();
        return new[] { topLeft, topRight, bottomRight, bottomLeft };
    }

    private static bool HasArea(Vec2[] points)
    {
        var a = points[1] - points[0];
        var b = points[3] - points[0];
        return Math.Abs(a.X * b.Y - a.Y * b.X) > 1e-8;
    }

    private static bool Separated(Vec2[] axesFrom, Vec2[] other)
    {
        for (var i = 0; i < axesFrom.Length; i++)
        {
            var edge = axesFrom[(i + 1) % axesFrom.Length] - axesFrom[i];
            var axis = new Vec2(-edge.Y, edge.X);
            var first = axesFrom.Select(point => point.X * axis.X + point.Y * axis.Y);
            var second = other.Select(point => point.X * axis.X + point.Y * axis.Y);
            if (first.Max() <= second.Min() || second.Max() <= first.Min())
                return true;
        }
        return false;
    }
}
