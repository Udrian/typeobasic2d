using System.Linq;
using TypeOEngine.Typedeaf.Basic2d;
using TypeOEngine.Typedeaf.Basic2d.Services;
using TypeOEngine.Typedeaf.Core;
using TypeOEngine.Typedeaf.Core.Engine;
using Xunit;

namespace TypeOBasic2dTest
{
    public class Basic2dModuleTest
    {
        public string GameName { get; set; } = "test";

        public class TestGame : Game
        {
            public override void Initialize()
            {
            }

            public override void Update(double dt)
            {
                Exit();
            }

            public override void Draw()
            {
            }

            public override void Cleanup()
            {
            }
        }

        [Fact]
        public void LoadBasic2dModule()
        {
            var typeO = TypeO.Create<TestGame>(GameName)
                 .LoadModule<Basic2dModule>() as TypeO;
            typeO.Start();
            var module = typeO.Context.Modules.FirstOrDefault(m => m.GetType() == typeof(Basic2dModule)) as Basic2dModule;
            Assert.NotNull(module);
            Assert.IsType<Basic2dModule>(module);
            Assert.NotEmpty(typeO.Context.Modules);

            Assert.NotEmpty(typeO.Context.Services);

            Assert.NotNull(typeO.Context.Services[typeof(BasicCamera2dService)]);
        }
    }
}
