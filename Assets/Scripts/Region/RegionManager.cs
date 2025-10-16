using System.Collections.Generic;
using Core;
using UnityEngine;

namespace Region
{
    public static class RegionManager
    {
        public static List<Region> Regions { get; private set; } = new();
        
        private static Region currentRegion;
        private static List<Sector> currentSectors = new();
        private static List<Location> currentLocations = new();
        private static List<Location> visibleLocations = new();
        
        private const float TimeForReposition = 3f;
        private const float LocationHidingDelay = 8f;

        private static Vector3 lastSavedSafePlayerPosition;
        private static float timeSinceLastPlayerPositionSave;
        
        public static void AddToRegions(Region region)
        {
            if (region != null && !Regions.Contains(region))
                Regions.Add(region);
        }

        public static void UpdateVisibleLocations(List<Location> newVisibleLocations)
        {
            //if player see new Region. All environments of locations, sectors and regions are loaded first
            //If see new sector activate logic in entire sector and activate his graphic
            //If see new location activate his graphic 

            foreach (var location in visibleLocations)
                if (!newVisibleLocations.Contains(location))
                {
                    if (location == null)
                        continue;
                    
                    if (location is Sector)
                        location.SwitchLogic(false);

                    location.HideGraphicsWithDelay(LocationHidingDelay);
                }

            foreach (var location in newVisibleLocations)
                if (!visibleLocations.Contains(location))
                {
                    location.CancelDelayedHiding();
                    
                    if (location is Sector)
                        location.Load();
                    
                    location.SwitchGraphics(true);
                    
                    if (location is Sector)
                        location.SwitchLogic(true);
                }

            visibleLocations.Clear();
            visibleLocations = newVisibleLocations;
        }

        public static void UpdateCurrentLocation<T>(List<T> currentLocations, List<T> newLocations) where T : Location
        {
            var previousLocations = new List<T>(currentLocations);

            currentLocations.Clear();
            currentLocations.AddRange(newLocations);

            foreach(var location in currentLocations)
                if(!previousLocations.Contains(location))
                    location.Enter();

            foreach (var location in previousLocations)
                if (!currentLocations.Contains(location))
                    location.Exit();
        }

        public static void UpdatePlayerPositionWithRepositionDelay()
        {
            if (Player.Instance.PlayerEntityGameObject == null)
                return;

            var playerPosition = Player.Instance.PlayerEntityGameObject.transform.position;

            var newRegion = RegionCoordinator.GetRegionFromPosition(playerPosition);
            var newSectors = RegionCoordinator.GetSectorsFromPosition(playerPosition,newRegion);  
            var newLocations = RegionCoordinator.GetLocationsFromPosition(playerPosition,newSectors);

            UpdateCurrentLocation(currentSectors,newSectors);
            UpdateCurrentLocation(currentLocations, newLocations);


            timeSinceLastPlayerPositionSave += Time.deltaTime;

            if (timeSinceLastPlayerPositionSave < TimeForReposition)
                return;

            if (currentSectors.Count > 0)
            {
                lastSavedSafePlayerPosition = playerPosition;
            }
            else
            {
                Player.Instance.PlayerEntityGameObject.transform.position = lastSavedSafePlayerPosition;
                Physics.SyncTransforms();
            }

            timeSinceLastPlayerPositionSave = 0;
        }

        public static void LoadLocationOnPosition(Vector3 position)
        {
            var sectors = RegionCoordinator.GetSectorsFromPosition(position);
            if (sectors.Count == 0) return;
            
            var currentSector = sectors[0];

            if (currentSector != null)
            {
                currentSector.MyRegion.Load();
                currentSector.Load();
                currentSector.SwitchGraphics(true);
            }
            else
            {
                Debug.LogWarning("Sector not found");
            }
        }

        public static bool IsPlayerInLocation(Location location)
        {
            return currentLocations.Contains(location);
        }

        public static void DebugPlayerPosition() => Debug.Log($"Region: {currentRegion}, Sector: {currentSectors[0]}");
    }
}