using System.Collections.Generic;
using UnityEngine;

namespace Core.SaveSystem
{
    public class ApartmentData
    {
        [ES3Serializable] public Dictionary<string, RoomSaveData> Rooms = new();
    }
    
    public class RoomSaveData
    {
        [ES3Serializable] public List<SocketData> Sockets = new();
    }
    
    public struct SocketData
    {
        public int SocketIndex;
        public string FurnitureID;
        public Quaternion Rotation;
    }
}