using TypeOEngine.Typedeaf.Basic2d.Engine.Services;
using TypeOEngine.Typedeaf.Core.Engine;

namespace TypeOEngine.Typedeaf.Basic2d
{
    /// <summary>
    /// Module for drawing and handling 2d, need a module that access low level hardwares
    /// </summary>
    public class Basic2dModule : Module<ModuleOption>
    {
        /// <inheritdoc/>
        protected override void Initialize() { }

        /// <inheritdoc/>
        protected override void Cleanup() { }

        /// <inheritdoc/>
        protected override void LoadExtensions(TypeO typeO)
        {
            typeO.AddService<BasicCamera2dService>();
        }
    }
}
