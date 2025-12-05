using System.Collections.Generic;
using System.Linq;
using Core.SaveSystem;
using Entities;
using Entities.Constructors;
using Entities.Databases;
using Entities.Molds;
using Hypertonic.Modules.UltimateSockets.PlaceableItems;
using Hypertonic.Modules.UltimateSockets.Sockets;
using UnityEngine;

namespace Components.PlacementSystem
{
    public class ApartmentController: MonoBehaviour
    {
        public static ApartmentController Instance { get; private set; }

        [Header("References")]
        [SerializeField] private List<RoomController> rooms = new();
        [SerializeField] private FurnitureDatabase furnitureDatabase;
        
        public bool IsDecorationModeActive { get; private set; }

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void Start()
        {
            SetDecorationMode(false);
            
            LoadApartmentState();
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
        
        public void RequestSave() => Core.Utilities.UtilsProvider.WaitAndRun(SaveApartmentState, true);
        
        private void SaveApartmentState()
        {
            var data = SaveManager.Progress.ApartmentData;
            data.Rooms.Clear();

            foreach (var room in rooms)
            {
                var roomData = new RoomSaveData();
                var sockets = room.GetAllSockets();

                for (int i = 0; i < sockets.Count; i++)
                {
                    var socket = sockets[i];
                    
                    if (socket.IsHoldingItem && socket.PlacedItem != null)
                    {
                        var entity = socket.PlacedItem.GetComponent<FurnitureEntity>();
                        if (entity != null && entity.SourceMold != null)
                        {
                            var socketData = new SocketData
                            {
                                SocketIndex = i,
                                FurnitureID = entity.SourceMold.ID,
                                Rotation = socket.PlacedItem.transform.localRotation
                            };
                            roomData.Sockets.Add(socketData);
                        }
                    }
                }

                if (roomData.Sockets.Count > 0)
                    data.Rooms.Add(room.RoomID, roomData);
            }

            SaveManager.SaveProgress();
            Debug.Log("[ApartmentController] Apartment state saved.");
        }

        private void LoadApartmentState()
        {
            var data = SaveManager.Progress.ApartmentData;
            if (data == null || data.Rooms == null) return;

            foreach (var room in rooms)
            {
                if (data.Rooms.TryGetValue(room.RoomID, out RoomSaveData roomData))
                {
                    var sockets = room.GetAllSockets();

                    foreach (var socketSaveData in roomData.Sockets)
                    {
                        if (socketSaveData.SocketIndex >= sockets.Count) continue;

                        Socket targetSocket = sockets[socketSaveData.SocketIndex];
                        FurnitureMold mold = furnitureDatabase.GetMoldById(socketSaveData.FurnitureID);

                        if (mold != null && targetSocket != null)
                            SpawnAndPlaceFurniture(targetSocket, mold, socketSaveData.Rotation);
                    }
                }
            }
            Debug.Log("[ApartmentController] Apartment state loaded.");
        }

        private void SpawnAndPlaceFurniture(Socket socket, FurnitureMold mold, Quaternion rotation)
        {
            EntityConstructor.Instance.LoadImmediately(mold, socket.transform, out Entity entity);
            
            var placeableItem = entity.GetComponent<PlaceableItem>();
            socket.PlaceItem(placeableItem);
            
            placeableItem.transform.localRotation = rotation;
            
            // change layer
        }
    }
}