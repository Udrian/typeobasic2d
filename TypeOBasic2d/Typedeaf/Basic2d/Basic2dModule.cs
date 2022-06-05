using TypeOEngine.Typedeaf.Basic2d.Services;
using TypeOEngine.Typedeaf.Core.Engine;

namespace TypeOEngine.Typedeaf.Basic2d
{
    public class Basic2dModule : Module<ModuleOption>
    {
        public Basic2dModule() : base()
        {

        }

        public override void Initialize()
        {
        }

        public override void Cleanup()
        {
        }

        public override void LoadExtensions()
        {
            TypeO.AddService<BasicCamera2dService>();
        }
    }
}
