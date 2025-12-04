using Entities.Molds;
using UnityEngine;

namespace Entities.Constructors
{
    public class EntityConstructor : AsyncObjectConstructor<Entity, Mold>
    {
        public static EntityConstructor Instance => instance;
        private static EntityConstructor instance = new();

        public override void LoadImmediately<T>(Mold entityMold, Transform parentToSet, out T result)
        {
            Entity pooledObject;

            switch (entityMold)
            {
                case PlayerMold playerMold:
                    pooledObject = LoadPlayer(playerMold, parentToSet);
                    break;
                case FurnitureMold furnitureMold:
                    pooledObject = LoadFurniture(furnitureMold, parentToSet);
                    break;
                default:
                    pooledObject = LoadEntity(entityMold, parentToSet);
                    break;
            }
            
            if (pooledObject != null) // temporary
            {
                //pooledObject.SwitchGraphics(false); 
               // pooledObject.ToggleLogic(false);  
            }

            result = (T)pooledObject;
        }

        public Entity LoadEntity(Mold mold, Transform parentToSet)
        {
            var pooledObject = TakeFromPool(mold.PrefabPoolInfo, parentToSet);
            var entity = pooledObject.GetComponent<Entity>();

            entity.LoadEntity(mold);

            return entity;
        }
        
        public Entity LoadPlayer(PlayerMold mold, Transform parentToSet)
        {
            var pooledObject = TakeFromPool(mold.PrefabPoolInfo, parentToSet);
            var playerEntity = pooledObject.GetComponent<PlayerEntity>();

            playerEntity.LoadEntity(mold);

            return playerEntity;
        }
        
        public Entity LoadFurniture(FurnitureMold mold, Transform parentToSet)
        {
            var pooledObject = TakeFromPool(mold.PrefabPoolInfo, parentToSet);
            var furnitureEntity = pooledObject.GetComponent<FurnitureEntity>();

            furnitureEntity.LoadEntity(mold);

            return furnitureEntity;
        }
    }
}
