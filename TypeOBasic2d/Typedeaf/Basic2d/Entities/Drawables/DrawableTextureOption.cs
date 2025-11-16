using TypeOEngine.Typedeaf.Core.Engine.Contents;

namespace TypeOEngine.Typedeaf.TypeOBasic2d
{
    namespace Entities.Drawables
    {
        /// <summary>
        /// Represents an option for configuring a <see cref="DrawableTexture"/> object, including its associated texture.
        /// </summary>
        /// <remarks>This class provides a way to specify a <see cref="Texture"/> that can be applied to
        /// a <see cref="DrawableTexture"/> during its creation. It extends the functionality of
        /// <see cref="Drawable2dOption{T}"/> to include texture-specific configuration.</remarks>
        public class DrawableTextureOption : Drawable2dOption<DrawableTexture>
        {
            /// <summary>
            /// Gets or sets the texture associated with the object.
            /// </summary>
            public Texture Texture { get; set; }

            /// <summary>
            /// Creates and initializes a <see cref="DrawableTexture"/> object.
            /// </summary>
            /// <remarks>If the <see cref="Texture"/> property is not <see langword="null"/>, it will
            /// be assigned to the <paramref name="obj"/> instance.</remarks>
            /// <param name="obj">The <see cref="DrawableTexture"/> instance to be initialized.</param>
            /// <returns><see langword="true"/> if the object was successfully created and initialized; otherwise,
            /// <see langword="false"/>.</returns>
            public override bool Create(DrawableTexture obj)
            {
                if(!base.Create(obj)) return false;

                if(Texture != null)
                    obj.Texture = Texture;

                return true;
            }
        }
    }
}
