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

        /// <summary>
        /// Indicates whether the quest associated with this entity has been completed.
        /// </summary>
        public bool FromCompletedQuest { get; private set; }

        /// <summary>
        /// If true, the entity will not be added to the Location's entity dictionary and will be disposed using quest logic.
        /// </summary>
        public bool IsQuestEntity { get; private set; }

        public void SetSpawnOptions(bool enableLogic = true, bool allowMultipleEntitiesInSingleParent = false, bool fromCompletedQuest = false, bool isQuestEntity = false)
        {
            EnableLogic = enableLogic;
            AllowMultipleEntitiesInSingleParent = allowMultipleEntitiesInSingleParent;
            FromCompletedQuest = fromCompletedQuest;
            IsQuestEntity = isQuestEntity;
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