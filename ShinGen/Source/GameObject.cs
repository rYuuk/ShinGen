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
            Transform.GameObject = this;
        }

        public void AddComponent(IComponent component)
        {
            Components.Add(component);
            component.GameObject = this;
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
