using System.Collections.Generic;
using System.Linq;
using Entities.Molds;
using UnityEngine;

namespace Entities.Databases
{
    [CreateAssetMenu(fileName = "FurnitureDatabase", menuName = "Entities/Furniture Database")]
    public class FurnitureDatabase : ScriptableObject
    {
        [SerializeField] private List<FurnitureMold> allFurniture;

        public FurnitureMold GetMoldById(string id) => allFurniture.FirstOrDefault(x => x.ID == id);
        
        public List<FurnitureMold> GetAllFurniture() => allFurniture;
    }
}
