namespace Xenko.Engine
{
    public static class EntityExtensions
    {
        public static void SetParent(this Entity entity, Entity parent)
        {
            entity.Transform.Parent = parent?.Transform;
        }
    }
}
