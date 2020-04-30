using System;
using System.Collections.Generic;
using System.Linq;
using VL.Lib.Collections;
using VL.Stride.Games;
using Stride.Core.Collections;
using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Games;

namespace VL.Stride.Engine
{
    /// <summary>
    /// Manages the children of an <see cref="Entity"/>.
    /// </summary>
    public sealed class EntityChildrenManager : IDisposable
    {
        readonly Entity entity;
        Spread<Entity> children = Spread<Entity>.Empty;

        public EntityChildrenManager(Entity entity)
        {
            this.entity = entity;
        }

        public Entity Update(Spread<Entity> children)
        {
            if (children != this.children)
            {
                foreach (var item in this.children)
                    EntityOutput.RemoveFromEntity(item, entity);

                var allAffectedEntities = new HashSet<Entity>(this.children);
                allAffectedEntities.AddRange(children);
                this.children = children;

                foreach (var item in this.children)
                    EntityOutput.AddToEntity(item, entity);

                foreach (var item in allAffectedEntities)
                    EntityOutput.Connect(item);
            }
            return entity;
        }

        public void Dispose()
        {
            foreach (var item in this.children)
                EntityOutput.RemoveFromEntity(item, entity);
        }
    }

    /// <summary>
    /// Manages the components of an <see cref="Entity"/>.
    /// </summary>
    public sealed class EntityComponentsManager : IDisposable
    {
        readonly Entity entity;
        Spread<EntityComponent> components = Spread<EntityComponent>.Empty;

        public EntityComponentsManager(Entity entity)
        {
            this.entity = entity;
        }

        public Entity Update(Spread<EntityComponent> components)
        {
            if (components != this.components)
            {
                foreach (var item in this.components)
                    ComponentOutput.RemoveComponentFromEntity(item, entity);

                var allAffectedComponents = new HashSet<EntityComponent>(this.components);
                allAffectedComponents.AddRange(components);
                this.components = components;

                foreach (var item in this.components)
                    ComponentOutput.AddComponentToEntity(item, entity);

                foreach (var item in allAffectedComponents)
                    ComponentOutput.Connect(item);
            }
            return entity;
        }

        public void Dispose()
        {
            foreach (var item in this.components)
                ComponentOutput.RemoveComponentFromEntity(item, entity);
        }
    }

    /// <summary>
    /// Manages the entities of a <see cref="Scene"/>.
    /// </summary>
    public sealed class SceneEntitiesManager : IDisposable
    {
        readonly Scene scene;
        Spread<Entity> entities = Spread<Entity>.Empty;

        public SceneEntitiesManager(Scene scene)
        {
            this.scene = scene;
        }

        public Scene Update(Spread<Entity> entities)
        {
            if (entities != this.entities)
            {
                foreach (var item in this.entities)
                    EntityOutput.RemoveFromEntity(item, scene);

                var allAffectedEntities = new HashSet<Entity>(this.entities);
                allAffectedEntities.AddRange(entities);
                this.entities = entities;

                foreach (var item in this.entities)
                    EntityOutput.AddToEntity(item, scene);

                foreach (var item in allAffectedEntities)
                    EntityOutput.Connect(item);
            }
            return scene;
        }

        public void Dispose()
        {
            foreach (var item in this.entities)
                EntityOutput.RemoveFromEntity(item, scene);
        }
    }

    /// <summary>
    /// Manages the child scenes of a <see cref="Scene"/>.
    /// </summary>
    public sealed class SceneChildrenManager : IDisposable
    {
        readonly Scene scene;
        Spread<Scene> children = Spread<Scene>.Empty;

        public SceneChildrenManager(Scene scene)
        {
            this.scene = scene;
        }

        public Scene Update(Spread<Scene> children)
        {
            if (children != this.children)
            {
                foreach (var item in this.children)
                    SceneOutput.RemoveFromScene(item, scene);

                var allAffectedEntities = new HashSet<Scene>(this.children);
                allAffectedEntities.AddRange(children);
                this.children = children;

                foreach (var item in this.children)
                    SceneOutput.AddToScene(item, scene);

                foreach (var item in allAffectedEntities)
                    SceneOutput.Connect(item);
            }
            return scene;
        }

        public void Dispose()
        {
            foreach (var item in this.children)
                SceneOutput.RemoveFromScene(item, scene);
        }
    }
}
