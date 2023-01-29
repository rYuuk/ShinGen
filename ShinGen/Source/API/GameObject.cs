namespace ShinGen
{
    public sealed class GameObject
    {
        public Transform Transform { get; }
        private List<IComponent> Components { get; }

        public GameObject()
        {
            Components = new List<IComponent>();
            Transform = new Transform();
        }

        public T AddComponent<T>() where T : IComponent, new()
        {
            var component = new T();
            Components.Add(component);
            component.Transform = Transform;
            return component;
        }

        public T? GetComponent<T>() where T : IComponent
        {
            var component = Components.FirstOrDefault(x => x.GetType() == typeof(T));
            if (component != null) return (T) component;
            return default;
        }

        public List<IComponent> GetAllComponents() => Components;
    }
}
