using TypeOEngine.Typedeaf.Basic2d.Engine.Graphics;
using TypeOEngine.Typedeaf.Core.Attributes;
using TypeOEngine.Typedeaf.Core.Common;
using TypeOEngine.Typedeaf.Core.Engine.Contents;
using TypeOEngine.Typedeaf.Core.Engine.Graphics.Interfaces;

namespace TypeOEngine.Typedeaf.TypeOBasic2d
{
    namespace Entities.Drawables
    {
        /// <summary>
        /// Represents a drawable 2D texture that can be rendered on a canvas with configurable properties such as
        /// scale, rotation, color, and flipping.
        /// </summary>
        /// <remarks>This class extends <see cref="Drawable2d"/> and provides additional functionality for
        /// rendering textures. The texture can be customized using properties such as <see cref="Scale"/>,
        /// <see cref="Rotation"/>, <see cref="Color"/>, and <see cref="Flipped"/>. The <see cref="Draw(ICanvas)"/> method is
        /// used to render the texture on the specified canvas.</remarks>
        public class DrawableTexture : Drawable2d
        {
            /// <summary>
            /// Gets or sets the texture associated with the object.
            /// </summary>
            public Texture Texture { get; set; }

            /// <summary>
            /// Gets or sets the scale of the object as a two-dimensional vector.
            /// </summary>
            public Vec2 Scale { get; set; }

            /// <summary>
            /// Gets or sets the rotation angle, in degrees.
            /// </summary>
            [TypeOProperty("Gets or sets the rotation angle of the object in degrees.", 0)]
            public double Rotation { get; set; }

            /// <summary>
            /// Gets or sets the color associated with the object.
            /// </summary>
            public Color Color { get; set; }

            /// <summary>
            /// Gets or sets the flipped state of the object.
            /// </summary>
            public Flipped Flipped { get; set; }

            /// <summary>
            /// Gets the size of the texture as a two-dimensional vector.
            /// </summary>
            public override Vec2 Size { get { return Texture.Size; } set { } }

            /// <summary>
            /// Represents a drawable texture with properties for scaling, rotation, color, and flipping.
            /// </summary>
            /// <remarks>This constructor initializes the drawable texture with default values:
            /// <list type="bullet"> <item><description><see cref="Scale"/> is set to
            /// <see cref="Vec2.One"/>.</description></item> <item><description><see cref="Rotation"/> is set to 0.
            /// </description></item> <item><description><see cref="Color"/> is set to
            /// <see cref="Color.White"/>.</description></item> <item><description><see cref="Flipped"/> is set to
            /// <see cref="Flipped.None"/>.</description></item> </list></remarks>
            public DrawableTexture() : base()
            {
                Scale = Vec2.One;
                Rotation = 0;
                Color = Color.White;
                Flipped = Flipped.None;
            }

            /// <summary>
            /// Performs initialization tasks required for the component.
            /// </summary>
            /// <remarks>This method is called to set up the component before it is used. Override
            /// this method to include any custom initialization logic specific to the derived class. Ensure that any
            /// required resources are properly initialized within this method.</remarks>
            protected override void Initialize() { }

            /// <summary>
            /// Releases resources or performs cleanup operations specific to the derived class.
            /// </summary>
            /// <remarks>This method is called to allow the derived class to release unmanaged
            /// resources, reset state, or perform other cleanup tasks. Override this method to implement custom
            /// cleanup logic.</remarks>
            protected override void Cleanup()
            {
                //Texture?.Cleanup();
            }

            /// <summary>
            /// Draws the current entity on the specified canvas using the configured texture, position, scale,
            /// rotation, color, and other properties.
            /// </summary>
            /// <remarks>This method renders the entity's texture on the provided canvas, applying
            /// transformations such as scaling, rotation, and flipping. If the <see cref="Texture"/> property is
            /// <see langword="null"/>, the method does not perform any drawing.</remarks>
            /// <param name="canvas">The canvas on which the entity will be drawn. Cannot be <see langword="null"/>.</param>
            public override void Draw(ICanvas canvas)
            {
                if (Texture == null) return;
                canvas.DrawImage(
                    Texture,
                    Position,
                    scale: Scale,
                    rotation: Rotation,
                    color: Color,
                    flipped: Flipped,
                    anchor: Entity as Entity2d
                );
            }
        }
    }
}
