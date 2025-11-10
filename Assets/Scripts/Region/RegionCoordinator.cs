using System.Collections.Generic;
using Components;
using Core;
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
            var visibleLocation = new List<Location>();
            foreach (var region in RegionManager.Regions)
            {
                if (!CameraManager.IsBoundsInCameraView(region.Bounds))
                    continue;

                visibleLocation.Add(region);

                foreach (var sector in region.GetSectors())
                {
                    if (!CameraManager.IsBoundsInCameraView(sector.Bounds))
                        continue;

                    visibleLocation.Add(sector);

                    foreach (var location in sector.GetLocations())
                    {
                        if (!CameraManager.IsBoundsInCameraView(location.Bounds))
                            continue;
                        
                        visibleLocation.Add(location);
                    }
                }
            }
            
            return visibleLocation;
        }
    }
}


