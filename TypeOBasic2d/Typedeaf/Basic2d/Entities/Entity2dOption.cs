using TypeOEngine.Typedeaf.Core.Engine;
using TypeOEngine.Typedeaf.TypeOBasic2d.Entities;

namespace TypeOEngine.Typedeaf.Basic2d.Entities
{
    /// <summary>
    /// Represents an abstract base class for creating options specific to 2D entities.
    /// </summary>
    /// <typeparam name="E">The type of the 2D entity for which the options are being created. Must derive from <see cref="Entity2d"/>.</typeparam>
    public abstract class Entity2dOption<E> : CreateOption<E> where E : Entity2d { }
}
