using System.Collections.Generic;
using Components.CameraSystem;
using Core.PlayerSystem;
using UnityEngine;

namespace Region
{
    public static class RegionCoordinator
    {
        private static Vector3 PlayerPosition
        {
            get
            {
                if (Player.Instance == null || Player.Instance.EntityGameObjectIsNull) 
                    return Vector3.zero;
                
                return Player.Instance.PlayerEntityGameObject.transform.position;
            }
        }

        public static void FindCurrentPlayerLocation()
        {
            if(Player.Instance == null || Player.Instance.EntityGameObjectIsNull)
                return;
            
            RegionManager.UpdateVisibleLocations(GetVisibleLocations());
        }

        public static List<Location> GetLocationsFromPosition(Vector3 position, List<Sector> sectors = null)
        {
            sectors ??= GetSectorsFromPosition(position);
            
            var locations = new List<Location>();
            foreach (var sector in sectors)
            {
                foreach (var location in sector.GetLocations())
                    if (location.IsInsideBounds(position))
                        locations.Add(location);
            }
            
            return locations;
        }

        public static List<Sector> GetSectorsFromPosition(Vector3 position, Region region = null)
        {
            var sectors = new List<Sector>();
            if (region == null) region = GetRegionFromPosition(position);
            if (region == null) return sectors;

            foreach (var sectorInRegion in region.GetSectors())
                if (sectorInRegion.IsInsideBounds(position))
                    sectors.Add(sectorInRegion);
            
            return sectors;
        }

        public static Region GetRegionFromPosition(Vector3 position)
        {
            foreach (var region in RegionManager.Regions)
                if (region.IsInsideBounds(position))
                    return region;
            
            return null;
        }

        private static List<Location> GetVisibleLocations()
        {
            var locationsToKeep = new HashSet<Location>();
            
            if (RegionManager.CurrentRegion != null)
                locationsToKeep.Add(RegionManager.CurrentRegion);
            
            var currentSectors = RegionManager.CurrentSectors;
            foreach (var sector in currentSectors)
                locationsToKeep.Add(sector);

            var currentLocations = RegionManager.CurrentLocations;
            foreach (var location in currentLocations)
                locationsToKeep.Add(location);

            foreach (var region in RegionManager.Regions)
            {
                bool isRegionInside = locationsToKeep.Contains(region);
                bool isRegionVisible = CameraManager.IsBoundsInCameraView(region.Bounds);

                if (!isRegionInside && !isRegionVisible)
                    continue;

                locationsToKeep.Add(region);

                foreach (var sector in region.GetSectors())
                {
                    bool isSectorInside = locationsToKeep.Contains(sector);
                    bool isSectorVisible = CameraManager.IsBoundsInCameraView(sector.Bounds);

                    if (!isSectorInside && !isSectorVisible)
                        continue;

                    locationsToKeep.Add(sector);

                    foreach (var location in sector.GetLocations())
                    {
                        if (locationsToKeep.Contains(location)) 
                            continue;

                        if (CameraManager.IsBoundsInCameraView(location.Bounds))
                            locationsToKeep.Add(location);
                    }
                }
            }
            
            return new List<Location>(locationsToKeep);
        }
    }
}


