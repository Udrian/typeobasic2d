using TypeOEngine.Typedeaf.Core.Common;
using TypeOEngine.Typedeaf.Core.Entities;
using TypeOEngine.Typedeaf.TypeOBasic2d.Entities;

namespace TypeOEngine.Typedeaf.Basic2d
{
    namespace Engine
    {
        /// <summary>
        /// Basic2d extension class of Core EntityList
        /// </summary>
        public static class EntityList
        {
            /// <summary>
            /// Creates an <see cref="Entity2d"/> of the given type and applies the transform values.
            /// </summary>
            /// <param name="entityList">The entity list to create the entity in.</param>
            /// <param name="type">The entity type to create.</param>
            /// <param name="position">The position, or <c>null</c> to keep the default.</param>
            /// <param name="scale">The scale, or <c>null</c> to keep the default.</param>
            /// <param name="rotation">The rotation.</param>
            /// <param name="origin">The origin, or <c>null</c> to keep the default.</param>
            /// <param name="id">The optional entity id.</param>
            /// <param name="pushToUpdateLoop">Whether to add the entity to the update loop.</param>
            /// <param name="pushToDrawStack">Whether to add the entity to the draw stack.</param>
            /// <returns>The created entity.</returns>
            public static Entity Create(this Core.Engine.EntityList entityList, Type type, Vec2? position, Vec2? scale = null, double rotation = 0, Vec2? origin = null, string id = null, bool pushToUpdateLoop = true, bool pushToDrawStack = true) //TODO: Split out
            {
                var entity = entityList.Create(type, id, pushToUpdateLoop, pushToDrawStack) as Entity2d;

                entity.Position = position ?? entity.Position;
                entity.Scale = scale ?? entity.Scale;
                entity.Rotation = rotation;
                entity.Origin = origin ?? entity.Origin;

                return entity;
            }

            /// <summary>
            /// Creates an entity of type <typeparamref name="E"/> and applies the transform values.
            /// </summary>
            /// <typeparam name="E">The 2D entity type to create.</typeparam>
            /// <param name="entityList">The entity list to create the entity in.</param>
            /// <param name="position">The position, or <c>null</c> to keep the default.</param>
            /// <param name="scale">The scale, or <c>null</c> to keep the default.</param>
            /// <param name="rotation">The rotation.</param>
            /// <param name="origin">The origin, or <c>null</c> to keep the default.</param>
            /// <param name="id">The optional entity id.</param>
            /// <param name="pushToUpdateLoop">Whether to add the entity to the update loop.</param>
            /// <param name="pushToDrawStack">Whether to add the entity to the draw stack.</param>
            /// <returns>The created entity.</returns>
            public static E Create<E>(this Core.Engine.EntityList entityList, Vec2? position, Vec2? scale = null, double rotation = 0, Vec2? origin = null, string id = null, bool pushToUpdateLoop = true, bool pushToDrawStack = true) where E : Entity2d, new() //TODO: Split out
            {
                var entity = entityList.Create<E>(id, pushToUpdateLoop, pushToDrawStack) as Entity2d;

                entity.Position = position ?? entity.Position;
                entity.Scale = scale ?? entity.Scale;
                entity.Rotation = rotation;
                entity.Origin = origin ?? entity.Origin;

                return entity as E;
            }
        }
    }
}