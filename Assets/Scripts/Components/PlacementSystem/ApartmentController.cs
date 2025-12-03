using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Components.PlacementSystem
{
    public class ApartmentController: MonoBehaviour
    {
        public static ApartmentController Instance { get; private set; }

        [Header("Rooms")] 
        [SerializeField] private List<RoomController> rooms = new();
        
        public bool IsDecorationModeActive { get; private set; }

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        [ContextMenu("Auto Collect Rooms")]
        private void CollectRooms()
        {
            rooms = GetComponentsInChildren<RoomController>(true).ToList();
        }
        
        public void SetDecorationMode(bool isActive)
        {
            IsDecorationModeActive = isActive;
            
            foreach (var room in rooms)
                room.SetSocketsVisibility(isActive);
        }
        
        public RoomController GetRoom(string id) => rooms.FirstOrDefault(r => r.RoomID == id);
    }
}