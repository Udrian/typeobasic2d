namespace TypeOEngine.Typedeaf.TypeOBasic2d
{
    namespace Entities
    {
        /// <summary>Specifies how a drawable or entity uses its size bounds.</summary>
        public enum SizeMode
        {
            /// <summary>The content is clipped to the size boundaries.</summary>
            Clip,
            /// <summary>The content is stretched to fit the entire size.</summary>
            Stretch,
            /// <summary>The content is drawn without clipping or stretching.</summary>
            None,
            /// <summary>An entity's size is calculated from its children; drawables use their natural size.</summary>
            Auto
        }
    }
}
