using System;
using System.Data.SqlTypes;
using VL.Lib.Collections;
using Xenko.Core.Collections;
using Xenko.Core.Mathematics;
using Xenko.Engine;

namespace VL.Xenko.Layer
{
    /// <summary>
    /// Manages the children.
    /// </summary>
    public abstract class ChildrenManagerBase<TParent, TChild, TLink> : IDisposable where TLink : IDisposable
    {
        readonly TParent parent;
        readonly FastList<TLink> links = new FastList<TLink>();
        Spread<TChild> children;
        FastList<TChild> invalidChildren = new FastList<TChild>();

        public ChildrenManagerBase(TParent parent)
        {
            if (parent == null)
                throw new NullReferenceException(parent.ToString());
            this.parent = parent;
        }

        protected abstract TLink CreateLink(TParent parent);

        protected abstract TParent GetParent(TChild child);

        protected abstract void SetChild(TLink link, TChild child);

        public virtual TParent Update(Spread<TChild> children)
        {
            // Quick change check
            if (children != this.children)
            {
                this.children = children;
                invalidChildren.Clear();
            }
            else
            {
                if (invalidChildren.Count > 0)
                    CheckInvalidChildren();

                return parent;
            }

            // Synchronize our entity links
            var array = children._array;
            var newChildren = 0;
            for (int i = 0; i < array.Length; i++)
            {
                var child = array[i];
                if (child != null)
                {
                    var p = GetParent(child);
                    if (p == null || p.Equals(this.parent))
                    {
                        var link = links.ElementAtOrDefault(i);
                        if (link == null)
                        {
                            link = CreateLink(parent);
                            links.Add(link);
                        }
                        SetChild(link, child);
                        newChildren++;
                    }
                    else //different parent
                    {
                        //increae child warning
                        invalidChildren.Add(child);
                    }
                }
            }

            for (int i = links.Count - 1; i >= newChildren; i--)
            {
                var link = links[i];
                link.Dispose();
                links.RemoveAt(i);
            }

            return parent;
        }

        private void CheckInvalidChildren()
        {
            for (int i = invalidChildren.Count - 1; i >= 0; i--)
            {
                var child = invalidChildren[i];
                if (GetParent(child) == null)
                {
                    var link = CreateLink(parent);
                    links.Add(link);
                    SetChild(link, child);
                    invalidChildren.RemoveAt(i);
                    //decrease child warning
                }
            }
        }

        public void Dispose()
        {
            foreach (var link in links)
                link.Dispose();
        }
    }

    /// <summary>
    /// Manages the children of an <see cref="Entity"/>.
    /// </summary>
    public sealed class EntityChildrenManager : ChildrenManagerBase<Entity, Entity, EntityLink>
    {
        public EntityChildrenManager(Entity entity)
            : base(entity)
        {
        }

        protected override EntityLink CreateLink(Entity parent)
        {
            return new EntityLink(parent);
        }

        protected override Entity GetParent(Entity child)
        {
            return child.GetParent();
        }

        protected override void SetChild(EntityLink link, Entity child)
        {
            link.Child = child;
        }

        public override Entity Update(Spread<Entity> children)
        {
            return base.Update(children);
        }
    }

    /// <summary>
    /// Manages the components of an <see cref="Entity"/>.
    /// </summary>
    public sealed class EntityComponentsManager : ChildrenManagerBase<Entity, EntityComponent, ComponentLink>
    {
        public EntityComponentsManager(Entity entity)
            : base(entity)
        {
        }

        protected override ComponentLink CreateLink(Entity parent)
        {
            return new ComponentLink(parent);
        }

        protected override Entity GetParent(EntityComponent child)
        {
            return child.Entity;
        }

        protected override void SetChild(ComponentLink link, EntityComponent child)
        {
            link.Component = child;
        }

        public override Entity Update(Spread<EntityComponent> components)
        {
            return base.Update(components);
        }
    }

    /// <summary>
    /// Manages the entities of a <see cref="Scene"/>.
    /// </summary>
    public sealed class SceneEntitiesManager : ChildrenManagerBase<Scene, Entity, EntitySceneLink>
    {
        public SceneEntitiesManager(Scene scene)
            : base(scene)
        {
        }

        protected override EntitySceneLink CreateLink(Scene parent)
        {
            return new EntitySceneLink(parent);
        }

        protected override Scene GetParent(Entity child)
        {
            return child.Scene;
        }

        protected override void SetChild(EntitySceneLink link, Entity child)
        {
            link.Child = child;
        }

        public override Scene Update(Spread<Entity> entities)
        {
            return base.Update(entities);
        }
    }

    /// <summary>
    /// Manages the child scenes of a <see cref="Scene"/>.
    /// </summary>
    public sealed class SceneChildrenManager : ChildrenManagerBase<Scene, Scene, SceneLink>
    {
        public SceneChildrenManager(Scene scene)
            : base(scene)
        {
        }

        protected override SceneLink CreateLink(Scene parent)
        {
            return new SceneLink(parent);
        }

        protected override Scene GetParent(Scene child)
        {
            return child.Parent;
        }

        protected override void SetChild(SceneLink link, Scene child)
        {
            link.Child = child;
        }
        public override Scene Update(Spread<Scene> children)
        {
            return base.Update(children);
        }
    }
}
