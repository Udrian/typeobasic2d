namespace TypeOEngine.Typedeaf.TypeOBasic2d
{
    namespace Entities.Drawables
    {
        /// <summary>Specifies how a texture is mapped onto a polygon.</summary>
        public enum PolygonTextureMode
        {
            /// <summary>The texture's border is wrapped around the points, spread evenly along the polygon outline, so the texture is stretched between the points.</summary>
            Stretch,
            /// <summary>The texture is stretched over the bounding box of the points and only the part inside the polygon is drawn.</summary>
            Bounds,
            /// <summary>The texture is drawn at its natural size from the top-left of the bounding box and clipped to the polygon, leaving what the texture does not cover undrawn.</summary>
            Clip
        }
    }
}
