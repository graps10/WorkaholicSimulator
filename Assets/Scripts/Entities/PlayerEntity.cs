using Core.Interfaces;
using Entities.Molds;
using UnityEngine;
using UnityEngine.TextCore.Text;

namespace Entities
{
    public class PlayerEntity : Entity, IUpdatable
    {
        public override void LoadEntity(Mold entityMold)
        {
            base.LoadEntity(entityMold);
            
             Initialize(entityMold as PlayerMold);
        }

        private void Initialize(PlayerMold mold) { }

        public void OnUpdate() { }
    }
}
