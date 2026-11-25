using System.Text.Json;
using TypeD.Helpers;
using TypeDBasic2d.Components;
using TypeDBasic2d.Models.Data;
using TypeDBasic2d.Models.Data.PropertyDefinitions;
using TypeOBasic2d.Typedeaf.Basic2d.Entities.Drawables.Primitives;
using TypeOEngine.Typedeaf.Basic2d.Engine.Graphics;
using TypeOEngine.Typedeaf.Core.Common;
using TypeOEngine.Typedeaf.TypeOBasic2d.Entities.Drawables;

namespace TypeOBasic2dTest;

public class DrawablePolygonTests
{
    [Fact]
    public void TextureCoordinates_StretchSpreadsPointsAroundTheTextureBorder()
    {
        var square = new[] { new Vec2(0, 0), new Vec2(10, 0), new Vec2(10, 10), new Vec2(0, 10) };
        var uvs = DrawablePolygon.GetTextureCoordinates(square, new Vec2(10, 10), new Vec2(1, 1), PolygonTextureMode.Stretch);
        Assert.Equal(new Vec2(0, 0), uvs[0]);
        Assert.Equal(new Vec2(1, 0), uvs[1]);
        Assert.Equal(new Vec2(1, 1), uvs[2]);
        Assert.Equal(new Vec2(0, 1), uvs[3]);
    }

    [Fact]
    public void TextureCoordinates_BoundsUsesBoundsAndClipUsesTextureSize()
    {
        var points = new[] { new Vec2(50, 0), new Vec2(100, 100) };
        var stretch = DrawablePolygon.GetTextureCoordinates(points, new Vec2(100, 100), new Vec2(200, 400), PolygonTextureMode.Bounds);
        var clip = DrawablePolygon.GetTextureCoordinates(points, new Vec2(100, 100), new Vec2(200, 400), PolygonTextureMode.Clip);
        Assert.Equal(new Vec2(0.5, 0), stretch[0]);
        Assert.Equal(new Vec2(1, 1), stretch[1]);
        Assert.Equal(new Vec2(0.25, 0), clip[0]);
        Assert.Equal(new Vec2(0.5, 0.25), clip[1]);
    }

    [Fact]
    public void BlendPointColor_TransparentUsesComponentColor()
    {
        var component = new Color(200, 10, 20, 30);
        Assert.Equal(component, DrawablePolygon.BlendPointColor(component, new Color(0, 255, 255, 255)));
    }

    [Fact]
    public void BlendPointColor_OpaqueUsesPointColorAndKeepsComponentAlpha()
    {
        var blended = DrawablePolygon.BlendPointColor(new Color(128, 10, 20, 30), new Color(255, 200, 100, 50));
        Assert.Equal(new Color(128, 200, 100, 50), blended);
    }

    [Fact]
    public void BlendPointColor_ModesCombineChannels()
    {
        var component = new Color(255, 100, 200, 0);
        var point = new Color(255, 200, 100, 255);
        Assert.Equal(new Color(255, 78, 78, 0), DrawablePolygon.BlendPointColor(component, point, PolygonColorBlend.Multiply));
        Assert.Equal(new Color(255, 255, 255, 255), DrawablePolygon.BlendPointColor(component, point, PolygonColorBlend.Add));
        Assert.Equal(new Color(255, 100, 100, 0), DrawablePolygon.BlendPointColor(component, point, PolygonColorBlend.Darken));
    }

    [Fact]
    public void GetPointColor_PointBlendOverridesPolygonBlend()
    {
        var polygon = new DrawablePolygon
        {
            Color = new Color(255, 100, 200, 0),
            Blend = PolygonColorBlend.Add,
            Points = new List<PolygonPoint>
            {
                new PolygonPoint(new Vec2(0, 0), new Color(255, 200, 100, 255)),
                new PolygonPoint(new Vec2(10, 0), new Color(255, 200, 100, 255), PolygonColorBlend.Darken),
                new PolygonPoint(new Vec2(0, 10))
            }
        };
        Assert.Equal(new Color(255, 255, 255, 255), polygon.GetPointColor(0));
        Assert.Equal(new Color(255, 100, 100, 0), polygon.GetPointColor(1));
        Assert.Equal(polygon.Color, polygon.GetPointColor(2));
    }

    [Fact]
    public void Size_CoversNegativePoints_AndDrawingUsesBoundsMin()
    {
        var polygon = new DrawablePolygon
        {
            Origin = new Vec2(0.5, 0.5),
            Points = Points((-10, 0), (40, -20), (30, 30))
        };

        Assert.Equal(new Vec2(50, 50), polygon.Size);
        Assert.Equal(new Vec2(-10, -20), polygon.BoundsMin);
    }

    [Fact]
    public void TryNormalize_ShiftsPointsAndPosition_OnlyForTopLeftOrigin()
    {
        var points = new List<Vec2> { new(-10, 0), new(40, -20), new(30, 30) };
        var polygon = new DrawablePolygon { Position = new Vec2(100, 100), Scale = new Vec2(2, 2) };

        Assert.True(polygon.TryNormalize(points, out var normalized, out var position));
        Assert.Equal(new[] { new Vec2(0, 20), new Vec2(50, 0), new Vec2(40, 50) }, normalized);
        Assert.Equal(new Vec2(80, 60), position);

        polygon.Origin = new Vec2(0.5, 0.5);
        Assert.False(polygon.TryNormalize(points, out _, out _));
        polygon.Origin = Vec2.Zero;
        Assert.False(polygon.TryNormalize(normalized, out _, out _));
    }

    private static double TriangleArea(IReadOnlyList<Vec2> points, int[] indices)
    {
        double total = 0;
        for (var i = 0; i < indices.Length; i += 3)
        {
            var a = points[indices[i]];
            var b = points[indices[i + 1]];
            var c = points[indices[i + 2]];
            total += Math.Abs((b.X - a.X) * (c.Y - a.Y) - (c.X - a.X) * (b.Y - a.Y)) / 2;
        }
        return total;
    }

    [Fact]
    public void Triangulate_ConcavePolygon_CoversExactlyItsArea()
    {
        // An L shape with area 3
        var points = new List<Vec2> { new(0, 0), new(2, 0), new(2, 1), new(1, 1), new(1, 2), new(0, 2) };

        var indices = PolygonTriangulator.Triangulate(points);

        Assert.Equal(4 * 3, indices.Length);
        Assert.Equal(3, TriangleArea(points, indices), 6);
    }

    [Fact]
    public void Triangulate_ClockwiseAndCounterClockwise_GiveTheSameArea()
    {
        var points = new List<Vec2> { new(0, 0), new(4, 0), new(4, 4), new(2, 1), new(0, 4) };
        var reversed = points.AsEnumerable().Reverse().ToList();

        Assert.Equal(
            TriangleArea(points, PolygonTriangulator.Triangulate(points)),
            TriangleArea(reversed, PolygonTriangulator.Triangulate(reversed)), 6);
    }

    [Fact]
    public void Triangulate_FewerThanThreePoints_IsEmpty()
    {
        Assert.Empty(PolygonTriangulator.Triangulate(new List<Vec2> { new(0, 0), new(1, 1) }));
    }

    [Fact]
    public void Size_ScalesThePointsToFitTheNewSize_KeepingTheShape()
    {
        var polygon = new DrawablePolygon { Points = Points((10, 5), (40, 20), (0, 30)) };

        polygon.Size = new Vec2(80, 50);

        Assert.Equal(new Vec2(80, 50), polygon.Size);
        Assert.Equal(new[] { new Vec2(20, 5), new Vec2(80, 35), new Vec2(0, 55) }, polygon.Points.Select(p => p.Position));
    }

    [Fact]
    public void Size_KeepsThePointColorsWhenScaling()
    {
        var color = new Color(255, 1, 2, 3);
        var polygon = new DrawablePolygon { Points = new List<PolygonPoint> { new(new Vec2(0, 0), color), new(new Vec2(10, 0)), new(new Vec2(0, 10)) } };

        polygon.Size = new Vec2(20, 20);

        Assert.Equal(color, polygon.Points[0].Color);
        Assert.Equal(new Vec2(20, 0), polygon.Points[1].Position);
    }

    private static List<PolygonPoint> Points(params (double X, double Y)[] positions)
    {
        return positions.Select(p => new PolygonPoint(new Vec2(p.X, p.Y))).ToList();
    }

    [Fact]
    public void TryNormalize_ShrinkingFromTopLeft_MovesPositionToKeepBoundsTight()
    {
        var polygon = new DrawablePolygon { Position = new Vec2(100, 100) };

        Assert.True(polygon.TryNormalize(new List<Vec2> { new(20, 10), new(60, 10), new(40, 50) }, out var normalized, out var position));

        Assert.Equal(new[] { new Vec2(0, 0), new Vec2(40, 0), new Vec2(20, 40) }, normalized);
        Assert.Equal(new Vec2(120, 110), position);
    }

    [Fact]
    public void ToCode_WritesPointPositionsAndColors()
    {
        var code = new PolygonPointListPropertyDefinition().ToCode(new List<PolygonPoint>
        {
            new(new Vec2(1, 2), new Color(255, 1, 2, 3), PolygonColorBlend.Multiply),
            new(new Vec2(3.5, 4))
        });

        Assert.StartsWith("new System.Collections.Generic.List<TypeOBasic2d.Typedeaf.Basic2d.Entities.Drawables.Primitives.PolygonPoint> {", code);
        Assert.Contains("Vec2(1", code);
        Assert.Contains("Vec2(3.5", code);
        Assert.Contains("(255, 1, 2, 3)", code);
        Assert.Contains("PolygonColorBlend.Multiply", code);
        Assert.Contains("PolygonColorBlend.Default", code);
    }

    [Fact]
    public void Points_RoundTripThroughJson()
    {
        var points = new List<PolygonPoint> { new(new Vec2(1, 2), new Color(255, 9, 8, 7)), new(new Vec2(3.5, 4)), new(new Vec2(0, 9)) };

        var element = JsonSerializer.SerializeToElement(points, typeof(List<PolygonPoint>), JSON.CreateOptions());
        var restored = element.Deserialize<List<PolygonPoint>>(JSON.CreateOptions(web: true));

        Assert.Equal(points, restored);
    }

    [Fact]
    public void Component_IsADrawableWithDrawableChildren()
    {
        var component = Basic2dComponent.DrawablePolygonComponent();

        Assert.Equal("TypeOEngine.Typedeaf.Core.Entities.Drawables.Drawable", component.RootBaseComponent.FullName);
        var filter = new FilterHelper();
        component.Template.ChildrenFilter(filter);
        Assert.Contains("TypeOEngine.Typedeaf.Core.Entities.Drawables.Drawable;", filter.Filters);
    }

    [Fact]
    public void Template_SummarizesThePointCount()
    {
        Assert.Equal("3 points", new PolygonPointsPropertyTemplate().GetSummary(new List<PolygonPoint> { new(), new(), new() }));
    }
}
