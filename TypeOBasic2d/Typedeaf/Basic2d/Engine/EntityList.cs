using System;
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
            public static Entity Create(this Core.Engine.EntityList entityList, Type type, Vec2? position, Vec2? scale = null, double rotation = 0, Vec2? origin = null, bool pushToUpdateLoop = true, bool pushToDrawStack = true) //TODO: Split out
            {
                var entity = entityList.Create(type, pushToUpdateLoop, pushToDrawStack) as Entity2d;

                entity.Position = position ?? entity.Position;
                entity.Scale = scale ?? entity.Scale;
                entity.Rotation = rotation;
                entity.Origin = origin ?? entity.Origin;

                return entity;
            }

            public static E Create<E>(this Core.Engine.EntityList entityList, Vec2? position, Vec2? scale = null, double rotation = 0, Vec2? origin = null, bool pushToUpdateLoop = true, bool pushToDrawStack = true) where E : Entity2d, new() //TODO: Split out
            {
                var entity = entityList.Create<E>(pushToUpdateLoop, pushToDrawStack) as Entity2d;

                entity.Position = position ?? entity.Position;
                entity.Scale = scale ?? entity.Scale;
                entity.Rotation = rotation;
                entity.Origin = origin ?? entity.Origin;

                return entity as E;
            }
        }
    }
}