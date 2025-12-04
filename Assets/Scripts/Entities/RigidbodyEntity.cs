using Entities.Molds;
using UnityEngine;

namespace Entities
{
    public class RigidbodyEntity: Entity
    {
        [SerializeField] protected Rigidbody controlledRigidbody;
        [SerializeField] protected Collider[] controlledColliders;

        public override void LoadEntity(Mold entityMold)
        {
            base.LoadEntity(entityMold);
            SwitchColliders(true);
        }

        public void SwitchColliders(bool stateToSet)
        {
            if (controlledColliders == null)
                return;

            foreach (var c in controlledColliders)
                c.enabled = stateToSet;
        }
        
        public Rigidbody GetRigidbody() => controlledRigidbody;
    }
}