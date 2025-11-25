using System;
using Entities.Molds;
using UnityEngine;

namespace Entities
{
    [Serializable]
    public struct EntitySpawnPreset
    {
        [HideInInspector] public string Name;

        [SerializeField] internal Transform[] transforms;

        [SerializeField] internal Mold mold;

        /// <summary>
        /// If true, entity's logic will be automatically enabled when the entity is spawned
        /// </summary>
        public bool EnableLogic { get; private set; }

        /// <summary>
        /// If true, multiple entities can exist under one parent.
        /// </summary>
        public bool AllowMultipleEntitiesInSingleParent { get; private set; }

        public void SetSpawnOptions(bool enableLogic = true, bool allowMultipleEntitiesInSingleParent = false)
        {
            EnableLogic = enableLogic;
            AllowMultipleEntitiesInSingleParent = allowMultipleEntitiesInSingleParent;
        }
        
        public bool Equals(EntitySpawnPreset obj)
        {
            if (transforms.Length != obj.transforms.Length)
                return false;

            if (mold != obj.mold)
                return false;

            for (int i = 0; i < transforms.Length; i++)
                if (transforms[i] != obj.transforms[i])
                    return false;

            return true;
        }
    }
}