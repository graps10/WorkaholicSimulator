using UnityEngine;

namespace Entities
{
    public class RigidbodyEntity: Entity
    {
        [SerializeField] protected Rigidbody controllerRigidbody;
        
        public Rigidbody GetRigidbody() => controllerRigidbody;
    }
}