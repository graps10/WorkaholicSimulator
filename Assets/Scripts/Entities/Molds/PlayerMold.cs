using System;
using UnityEngine;

namespace Entities.Molds
{
    [CreateAssetMenu(fileName = "PlayerMold", menuName = "Entities/Molds/PlayerMold")]
    public class PlayerMold: SimpleEntityMold
    {
        public Action OnMoldChange;
        private void OnValidate() => OnMoldChange?.Invoke();
    }
}