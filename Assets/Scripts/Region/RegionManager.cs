using System.Collections.Generic;
using Core;
using Core.PlayerSystem;
using UnityEngine;

namespace Region
{
    public static class RegionManager
    {
        private const float Time_For_Reposition = 3f;
        private const float Location_Hiding_Delay = 8f;
        
        private const float Max_Distance_To_Keep_Sector = 75f;
        
        public static List<Region> Regions { get; private set; } = new();
        
        public static Region CurrentRegion => currentRegion;
        public static IReadOnlyList<Sector> CurrentSectors => currentSectors;
        public static IReadOnlyList<Location> CurrentLocations => currentLocations;
        
        private static Region currentRegion;
        private static List<Sector> currentSectors = new();
        private static List<Location> currentLocations = new();
        private static List<Location> visibleLocations = new();
        
        private static Vector3 lastSavedSafePlayerPosition;
        private static float timeSinceLastPlayerPositionSave;
        
        public static void AddToRegions(Region region)
        {
            if (region != null && !Regions.Contains(region))
                Regions.Add(region);
        }

        public static void UpdateVisibleLocations(List<Location> newVisibleLocations)
        {
            // if player see new Region. All environments of locations, sectors and regions are loaded first
            // if see new sector activate logic in entire sector and activate his graphic
            // if see new location activate his graphic 

            foreach (var location in visibleLocations)
                if (!newVisibleLocations.Contains(location))
                {
                    if (location == null)
                        continue;
                    
                    if (location is Sector)
                        location.SwitchLogic(false);

                    location.HideGraphicsWithDelay(Location_Hiding_Delay);
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
            if (Player.Instance.EntityGameObjectIsNull)
                return;

            var playerPosition = Player.Instance.PlayerEntityGameObject.transform.position;

            var newRegion = RegionCoordinator.GetRegionFromPosition(playerPosition);
            if (newRegion == null && currentSectors.Count > 0)
                newRegion = currentRegion; 
            
            var newSectors = RegionCoordinator.GetSectorsFromPosition(playerPosition,newRegion); 
            if (newSectors.Count == 0 && currentSectors.Count > 0)
            {
                float distToCurrent = Vector3.Distance(playerPosition, currentSectors[0].transform.position);
                
                if (distToCurrent < Max_Distance_To_Keep_Sector)
                    return; 
            }
            
            var newLocations = RegionCoordinator.GetLocationsFromPosition(playerPosition,newSectors);
            
            if (currentRegion != newRegion)
            {
                currentRegion?.Exit();
                currentRegion = newRegion;
                currentRegion?.Enter();
            }
            
            UpdateCurrentLocation(currentSectors,newSectors);
            UpdateCurrentLocation(currentLocations, newLocations);

            timeSinceLastPlayerPositionSave += Time.deltaTime;
            if (timeSinceLastPlayerPositionSave < Time_For_Reposition)
                return;

            if (currentSectors.Count > 0)
                lastSavedSafePlayerPosition = playerPosition;
            else
            {
                Debug.LogWarning("Teleporting Player: No sectors found and distance check failed.");
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
            	Debug.LogWarning("Sector not found");
        }

        public static bool IsPlayerInLocation(Location location) => currentLocations.Contains(location);
    }
}