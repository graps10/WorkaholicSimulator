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
                default:
                    pooledObject = LoadEntity(entityMold, parentToSet);
                    break;
            }
            
            if (pooledObject != null)
            {
                pooledObject.ToggleRenderersEnabled(false); 
                pooledObject.ToggleLogic(false);  
            }

            result = (T)pooledObject;
        }

        public Entity LoadEntity(Mold mold, Transform parentToSet)
        {
            var pooledObject = TakeFromPool(mold.PrefabPoolInfo, parentToSet);
            var artor = pooledObject.GetComponent<Entity>();

            artor.LoadEntity(mold);

            return artor;
        }
    }
}
