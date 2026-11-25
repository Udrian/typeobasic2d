namespace TypeOEngine.Typedeaf.TypeOBasic2d
{
    namespace Entities.Drawables
    {
        /// <summary>Specifies how a polygon point color is blended with the polygon color.</summary>
        public enum PolygonColorBlend
        {
            /// <summary>For a point, uses the blend of the polygon. For the polygon, behaves like <see cref="Normal"/>.</summary>
            Default,
            /// <summary>The point color replaces the polygon color by the alpha of the point color.</summary>
            Normal,
            /// <summary>The colors are multiplied, which darkens.</summary>
            Multiply,
            /// <summary>The colors are added, which brightens.</summary>
            Add,
            /// <summary>The point color is subtracted from the polygon color.</summary>
            Subtract,
            /// <summary>The inverted colors are multiplied, which lightens.</summary>
            Screen,
            /// <summary>Multiplies dark parts and screens light parts of the polygon color.</summary>
            Overlay,
            /// <summary>The darkest of each color channel is used.</summary>
            Darken,
            /// <summary>The lightest of each color channel is used.</summary>
            Lighten,
            /// <summary>The absolute difference of each color channel is used.</summary>
            Difference
        }
    }
}
