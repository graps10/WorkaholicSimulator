using System.Collections.Generic;
using Hypertonic.Modules.UltimateSockets.Sockets;
using UnityEngine;

namespace Components.PlacementSystem
{
    public class EditModeController : MonoBehaviour
    {
        public static EditModeController Instance { get; private set; }

        private List<Socket> _allSockets = new();

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            _allSockets.AddRange(FindObjectsOfType<Socket>());
            
            SetEditMode(false);
        }

        public void SetEditMode(bool isEditMode)
        {
            foreach (var socket in _allSockets)
            {
                /*var visuals = socket.transform.Find("Visuals");
                if (visuals)
                {
                    visuals.gameObject.SetActive(isEditMode);
                }*/
                
                socket.enabled = isEditMode; 
            }
        }
        
        public void RegisterSocket(Socket socket)
        {
            if (!_allSockets.Contains(socket)) _allSockets.Add(socket);
        }
    }
}