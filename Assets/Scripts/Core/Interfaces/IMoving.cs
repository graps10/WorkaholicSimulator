using Region;
using UnityEngine;

namespace Core.Interfaces
{
    public interface IMoving
    {
        public void AddVisibleObject() => VisibleEntitiesManager.AddActingObject(this);

        public Bounds Bounds { get; }

        public Transform LocationParent { get; }

        public Transform SelfTransform { get; }

        public Entities.Entity Entity { get; }

        public bool IsOutOfSector { get;}

        public bool IsVisible { get; set; }
        
        public bool IsSectorLoaded { get; }

        public void UnloadIfOutOfBounds();

        public void SetSector(Sector sector);
    }
}