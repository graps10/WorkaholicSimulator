using System.Collections.Generic;
using System.Linq;
using Hypertonic.Modules.UltimateSockets.Sockets;
using UnityEngine;

namespace Components.PlacementSystem
{
    public class RoomController: MonoBehaviour
    {
        [Header("Identity")]
        [SerializeField] private string roomID; 

        [Header("Sockets")]
        [SerializeField] private List<Socket> roomSockets = new();

        public string RoomID => roomID;
        
        [ContextMenu("Auto Collect Sockets")]
        private void CollectSockets()
        {
            roomSockets = GetComponentsInChildren<Socket>(true).ToList();
        }

        public void SetSocketsVisibility(bool isVisible)
        {
            foreach (var socket in roomSockets)
            {
                if (socket == null) continue;
                
                socket.enabled = isVisible;
                
                var visual = socket.transform.GetChild(0);
                if (visual != null)
                    visual.gameObject.SetActive(isVisible);
            }
        }
        
        public List<Socket> GetAllSockets() => roomSockets;
    }
}