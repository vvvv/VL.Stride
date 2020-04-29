using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xenko.Engine;

namespace VL.Xenko.Engine
{
    public class ComponentOutput : IDisposable
    {
        EntityComponent entityComponent;
        List<Entity> wannaBeParents = new List<Entity>();

        static Dictionary<EntityComponent, ComponentOutput> LookUp = new Dictionary<EntityComponent, ComponentOutput>();

        public ComponentOutput(EntityComponent entityComponent)
        {
            this.entityComponent = entityComponent;
            LookUp.Add(entityComponent, this);
        }

        public static void AddComponentToEntity(EntityComponent entityComponent, Entity parentEntity)
        {
            if (entityComponent != null && LookUp.TryGetValue(entityComponent, out var output))
                output.wannaBeParents.Add(parentEntity);
        }

        public static void RemoveComponentFromEntity(EntityComponent entityComponent, Entity parentEntity)
        {
            if (entityComponent != null && LookUp.TryGetValue(entityComponent, out var output))
                output.wannaBeParents.Remove(parentEntity);
        }

        internal static void Connect(EntityComponent entityComponent)
        {
            if (entityComponent != null && LookUp.TryGetValue(entityComponent, out var output))
                output.ConnectToFirst();
        }        
        
        void ConnectToFirst()
        {
            var parentEntity = wannaBeParents.FirstOrDefault();
            if (entityComponent.Entity != parentEntity)
            {
                if (entityComponent.Entity != null)
                    entityComponent.Entity.Remove(entityComponent);
                if (parentEntity != null)
                    parentEntity.Add(entityComponent);
            }
        }

        public void Update(out bool ok)
        {
            ok = !wannaBeParents.Distinct().Many();
            //if (!ok)
            //    throw new Exception("Component should only be connected to one Entity.");
        }

        public void Dispose()
        {
            if (entityComponent.Entity != null)
                entityComponent.Entity.Remove(entityComponent);
            LookUp.Remove(entityComponent);
        }
    }


    public class EntityOutput : IDisposable
    {
        Entity entity;
        List<object> wannaBeParents = new List<object>();

        static Dictionary<Entity, EntityOutput> LookUp = new Dictionary<Entity, EntityOutput>();

        public EntityOutput(Entity entity)
        {
            this.entity = entity;
            LookUp.Add(this.entity, this);
        }

        public static void AddToEntity(Entity entity, object parentEntityOrScene)
        {
            if (entity != null && LookUp.TryGetValue(entity, out var output))
                output.wannaBeParents.Add(parentEntityOrScene);
        }

        public static void RemoveFromEntity(Entity entity, object parentEntityOrScene)
        {
            if (entity != null && LookUp.TryGetValue(entity, out var output))
                output.wannaBeParents.Remove(parentEntityOrScene);
        }

        internal static void Connect(Entity entity)
        {
            if (entity != null && LookUp.TryGetValue(entity, out var output))
                output.ConnectToFirst();
        }

        void ConnectToFirst()
        {
            var parentEntity = wannaBeParents.FirstOrDefault();
            var currentParent = parentEntity is Entity ? (object)entity.GetParent() : entity.Scene;
            if (currentParent != parentEntity)
            {
                if (entity.GetParent() != null)
                    entity.SetParent(null);
                if (entity.Scene != null)
                    entity.Scene = null;
                if (parentEntity != null)
                {
                    if (parentEntity is Entity e)
                        entity.SetParent(e);
                    else
                    if (parentEntity is Scene s)
                        entity.Scene = s;
                }
            }
        }

        public void Update(out bool ok)
        {
            ok = !wannaBeParents.Distinct().Many();
            //if (!ok)
            //    throw new Exception("Entity should only be connected to one parent entity or scene.");
        }

        public void Dispose()
        {
            var currentParent = entity.GetParent();
            if (currentParent != null)
            {
                entity.SetParent(null);
                entity.Scene = null;
            }
            LookUp.Remove(entity);
        }
    }

    public class SceneOutput : IDisposable
    {
        Scene scene;
        List<Scene> wannaBeParents = new List<Scene>();

        static Dictionary<Scene, SceneOutput> LookUp = new Dictionary<Scene, SceneOutput>();

        public SceneOutput(Scene scene)
        {
            this.scene = scene;
            LookUp.Add(this.scene, this);
        }

        public static void AddToScene(Scene scene, Scene parentScene)
        {
            if (scene != null && LookUp.TryGetValue(scene, out var output))
                output.wannaBeParents.Add(parentScene);
        }

        public static void RemoveFromScene(Scene scene, Scene parentScene)
        {
            if (scene != null && LookUp.TryGetValue(scene, out var output))
                output.wannaBeParents.Remove(parentScene);
        }

        internal static void Connect(Scene scene)
        {
            if (scene != null && LookUp.TryGetValue(scene, out var output))
                output.ConnectToFirst();
        }

        void ConnectToFirst()
        {
            var parentScene = wannaBeParents.FirstOrDefault();
            var currentParent = scene.Parent;
            if (currentParent != parentScene)
            {
                if (currentParent != null)
                    scene.Parent = null;
                if (parentScene != null)
                    scene.Parent = parentScene;
            }
        }

        public void Update(out bool ok)
        {
            ok = !wannaBeParents.Distinct().Many();
            //if (!ok)
            //    throw new Exception("Scene should only be connected to one parent scene.");
        }

        public void Dispose()
        {
            var currentParent = scene.Parent;
            if (currentParent != null)
                scene.Parent = null;
            LookUp.Remove(scene);
        }
    }

}
