using TypeOEngine.Typedeaf.Core.Common;
using TypeOEngine.Typedeaf.Core.Engine.Graphics.Interfaces;
using TypeOEngine.Typedeaf.Core.Engine.Services;

namespace TypeOEngine.Typedeaf.Basic2d
{
    namespace Engine.Services
    {
        /// <summary>
        /// Provides basic 2D camera functionality for managing the position and translation of a canvas in a 2D world.
        /// </summary>
        /// <remarks>This service allows setting and retrieving the camera's position in the 2D world
        /// space through the <see cref="Position"/> property. It requires an <see cref="ICanvas"/> instance to be set
        /// using the <see cref="SetCanvas(ICanvas)"/> method before use.</remarks>
        public class BasicCamera2dService : Service
        {
            /// <summary>
            /// Gets or sets the canvas used for rendering operations.
            /// </summary>
            protected ICanvas Canvas { get; set; }

            /// <summary>
            /// Gets or sets the position of the canvas in world coordinates.
            /// </summary>
            public Vec2 Position { get { return new Vec2(Canvas.WorldTranslation.X, Canvas.WorldTranslation.Y); } set { Canvas.WorldTranslation = new Vec3(value.X, value.Y, 0); }  }

            /// <summary>
            /// Sets the canvas to be used for drawing operations.
            /// </summary>
            /// <param name="canvas">The canvas instance to set. Cannot be <see langword="null"/>.</param>
            public void SetCanvas(ICanvas canvas)
            {
                Canvas = canvas;
            }

            /// <summary>
            /// Performs initialization logic for the derived class.
            /// </summary>
            /// <remarks>This method is called during the initialization phase and can be overridden
            /// to provide custom setup logic. Ensure that any required resources are properly configured before the
            /// method completes.</remarks>
            protected override void Initialize()
            {
            }

            /// <summary>
            /// Releases resources and performs cleanup operations specific to the derived class.
            /// </summary>
            /// <remarks>This method is called to allow the derived class to release unmanaged
            /// resources, dispose of disposable objects, or perform other cleanup tasks. Override this method to
            /// implement custom cleanup logic.</remarks>
            protected override void Cleanup()
            {
            }
        }
    }
}
