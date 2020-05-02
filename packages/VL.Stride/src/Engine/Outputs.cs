using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Stride.Engine;
using VL.Lib.Collections.TreePatching;

namespace VL.Stride.Engine
{
    /// <summary>
    /// This Process Node exposes the <see cref="EntityComponent"/> as a potential child component of an <see cref="Entity"/>.
    /// Entity patches work with a <see cref="TreeNodeChildrenManager{Entity, EntityComponent}"/> in order to talk to this output.
    /// </summary>    
    public class ComponentOutput<T> : TreeNodeParentManager<Entity, EntityComponent>
        where T : EntityComponent
    {
        public ComponentOutput(T entityComponent)
            : base(entityComponent)
        { }

        // this is just here for being able to configure the process node directly in the type forward
        public new bool ManyWannaBeParents => base.ManyWannaBeParents;

        // this is just here for being able to configure the process node directly in the type forward
        public new void Dispose() => base.Dispose();

        protected override void AttachToParent()
        {
            Parent.Add(node);
        }

        protected override void DetachFromParent()
        {
            Parent.Remove(node);
        }
    }

    /// <summary>
    /// This Process Node exposes the Entity as a potential child. 
    /// Parent of this <see cref="Entity"/> can be an <see cref="Entity"/> or a <see cref="Scene"/>.
    /// Therefore <see cref="Entity"/> and <see cref="Scene"/> patches have to work with a <see cref="TreeNodeChildrenManager{object, Entity}"/>
    /// with its TNode argument set to <see cref="object"/> in order to talk to this output.
    /// </summary>
    public class EntityOutput : TreeNodeParentManager<object, Entity>
    {
        public EntityOutput(Entity entity)
            : base(entity)
        { }

        // this is just here for being able to configure the process node directly in the type forward
        public new bool ManyWannaBeParents => base.ManyWannaBeParents;

        // this is just here for being able to configure the process node directly in the type forward
        public new void Dispose() => base.Dispose();

        protected override void AttachToParent()
        {
            if (Parent is Entity e)
                node.SetParent(e);
            else
            if (Parent is Scene s)
                node.Scene = s;
        }

        protected override void DetachFromParent()
        {
            node.SetParent(null);
            node.Scene = null;
        }
    }

    /// <summary>
    /// This Process Node exposes the <see cref="Scene"/> as a potential child scene of another <see cref="Scene"/>.
    /// Scene patches work with a <see cref="TreeNodeChildrenManager{Scene, Scene}"/> in order to talk to this output.
    /// </summary>   
    public class SceneOutput : TreeNodeParentManager<Scene, Scene>
    {
        public SceneOutput(Scene scene)
            : base(scene)
        { }

        // this is just here for being able to configure the process node directly in the type forward
        public new bool ManyWannaBeParents => base.ManyWannaBeParents;

        // this is just here for being able to configure the process node directly in the type forward
        public new void Dispose() => base.Dispose();


        protected override void AttachToParent()
        {
            node.Parent = Parent;
        }

        protected override void DetachFromParent()
        {
            node.Parent = null;
        }
    }
}
