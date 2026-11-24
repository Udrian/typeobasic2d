namespace TypeOEngine.Typedeaf.TypeOBasic2d
{
    namespace Entities
    {
        /// <summary>Specifies how a drawable is rendered within its size bounds.</summary>
        public enum SizeMode
        {
            /// <summary>The drawable is drawn at its natural size, clipped at the size boundaries.</summary>
            Clip,
            /// <summary>The drawable is stretched to fit the entire size.</summary>
            Stretch,
            /// <summary>The drawable is drawn at its natural size without clipping or stretching.</summary>
            None
        }
    }
}
