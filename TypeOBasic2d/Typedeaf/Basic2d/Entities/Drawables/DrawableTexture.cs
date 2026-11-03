using TypeOEngine.Typedeaf.Basic2d.Engine.Graphics;
using TypeOEngine.Typedeaf.Core.Attributes;
using TypeOEngine.Typedeaf.Core.Common;
using TypeOEngine.Typedeaf.Core.Engine.Contents;
using TypeOEngine.Typedeaf.Core.Engine.Graphics.Interfaces;
using TypeOEngine.Typedeaf.Core.Entities;

namespace TypeOEngine.Typedeaf.TypeOBasic2d
{
    namespace Entities.Drawables
    {
        /// <summary>
        /// Represents a drawable 2D texture that can be rendered on a canvas with configurable properties such as
        /// scale, rotation, color, and flipping.
        /// </summary>
        /// <remarks>This class extends <see cref="Drawable2d"/> and provides additional functionality for
        /// rendering textures. The texture can be customized using properties such as scale, rotation, color, and flipping.
        /// The <see cref="Draw(ICanvas)"/> method is used to render the texture on the specified canvas.</remarks>
        public class DrawableTexture : Drawable2d
        {
            /// <summary>
            /// Gets or sets the texture associated with the object.
            /// </summary>
            [TypeOProperty("Gets or sets the texture associated with the object.")]
            public Texture Texture { get; set; }

            /// <summary>
            /// Gets or sets the color associated with the object.
            /// </summary>
            [TypeOProperty("Gets or sets the color associated with the object.", null)]
            public Color Color { get; set; }

            /// <summary>
            /// Gets or sets the flipped state of the object.
            /// </summary>
            [TypeOProperty("Gets or sets the flipped state of the object.")]
            public Flipped Flipped { get; set; }

            /// <summary>
            /// Gets or sets the size of the texture display area. If set to zero, the texture's natural size is used.
            /// </summary>
            public override Vec2 Size 
            { 
                get { return base.Size == Vec2.Zero ? (Texture?.Size ?? Vec2.Zero) : base.Size; }
                set { base.Size = value; }
            }

            /// <summary>
            /// Gets or sets how the texture is rendered within the Size bounds.
            /// </summary>
            [TypeOProperty("Gets or sets how the texture is rendered within the Size bounds.")]
            public SizeMode SizeMode { get; set; }

            /// <summary>
            /// Represents a drawable texture with properties for scaling, rotation, color, and flipping.
            /// </summary>
            /// <remarks>This constructor initializes the drawable texture with default values from the base Drawable2d class,
            /// plus color set to white and flipping set to None.</remarks>
            public DrawableTexture() : base()
            {
                Color = Color.White;
                Flipped = Flipped.None;
                SizeMode = SizeMode.Clip;
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
            /// transformations such as scaling, rotation, flipping, and origin offset. If the <see cref="Texture"/> property is
            /// <see langword="null"/>, the method does not perform any drawing.</remarks>
            /// <param name="canvas">The canvas on which the entity will be drawn. Cannot be <see langword="null"/>.</param>
            public override void Draw(ICanvas canvas)
            {
                if (Texture == null) return;

                var displaySize = Size;
                var drawScale = Scale;
                var actualSize = Texture.Size * drawScale;

                if (SizeMode == SizeMode.Stretch && displaySize != Vec2.Zero)
                {
                    var stretchScale = displaySize / Texture.Size;
                    drawScale = stretchScale * Scale;
                    actualSize = displaySize * Scale;
                }

                var originOffset = actualSize * Origin;

                // When SizeMode is Clip and Size is set, use the bounding box as the clipping region
                Rectangle? sourceRect = null;
                if (SizeMode == SizeMode.Clip && displaySize != Vec2.Zero)
                {
                    sourceRect = new Rectangle(Position - originOffset, displaySize * drawScale);
                }

                canvas.DrawImage(
                    Texture,
                    Position - originOffset,
                    scale: drawScale,
                    rotation: Rotation,
                    color: Color,
                    flipped: Flipped,
                    source: sourceRect,
                    anchor: Parent as Entity2d
                );
            }
        }
    }
}
