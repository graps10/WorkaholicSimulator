#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using UnityEditor;
using UnityEditor.SceneManagement;

namespace Region.Editors
{
    [CustomEditor(typeof(Region))]
    [CanEditMultipleObjects]
    public class RegionEditor : SectorEditor
    {
        protected override string iconPath => "Assets/Textures/Icons/Editor/RegionIcon.png";
        
        private bool _showUtilitiesOptions = true;
        private bool _showSectorsList = true;

        private SerializedProperty _level;

        protected override void OnEnable()
        {
            base.OnEnable();
            Undo.undoRedoPerformed += RefreshChildrenSectors;

            _level = serializedObject.FindProperty("<Level>k__BackingField");
        }
        
        private void OnDisable() => Undo.undoRedoPerformed -= RefreshChildrenSectors;
        
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(_level);
            EditorGUILayout.Space();

            DrawSectorsList();
            DrawCustomSettings();
            DrawUnityEvents();
            RefreshChildrenSectors();

            serializedObject.ApplyModifiedProperties();

            if (GUI.changed)
                location.RefreshEditorEntities();

            Region region = (Region)target;
            
            DrawVisibilityCheckbox(region);
            DrawOverlapCheckButton(region);
            
            SetCustomIcon();
        }
        
        private void DrawSectorsList()
        {
            Region region = (Region)target;
            _showSectorsList = EditorGUILayout.Foldout(_showSectorsList, "Sectors List", true);
            
            if (!_showSectorsList) return;
            
            EditorGUILayout.Space();

            if (region.GetSectors() != null)
            {
                for (int i = 0; i < region.GetSectors().Count; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    
                    region.GetSectors()[i].name = EditorGUILayout.TextField("Sector " + i, region.GetSectors()[i].name);

                    if (GUILayout.Button("Remove", GUILayout.Width(60)))
                    {
                        Undo.DestroyObjectImmediate(region.GetSectors()[i].gameObject);
                        break;
                    }
                    
                    EditorGUILayout.EndHorizontal();
                }
            }
            
            if (GUILayout.Button("Add Sector"))
                AddNewSector();
            
            EditorGUILayout.Space();
        }

        private void AddNewSector()
        {
            Region region = (Region)target;

            GameObject newSector = new GameObject("New Sector " + (region.GetSectors() == null ? 0 : region.GetSectors().Count + 1));
            Undo.RegisterCreatedObjectUndo(newSector, "Create new sector");
            newSector.transform.SetParent(region.transform);
            Sector sector = newSector.AddComponent<Sector>();
            
            if(region.GetSectors() == null)
                region.InitializeNewSectorsList();
            
            region.GetSectors().Add(sector);
            sector.CalculateBounds();
        }

        private void RefreshChildrenSectors()
        {
            Region region = (Region)target;
            
            if (region.GetSectors() == null)
                return;
            
            region.GetSectors().Clear();
            
            var sectors = region.GetComponentsInChildren<Sector>(false);
            foreach (var sector in sectors)
                if(!(sector is Region) && !region.GetSectors().Contains(sector))
                    region.GetSectors().Add(sector);
        }

        private static void DrawVisibilityCheckbox(Region region)
        {
            EditorGUILayout.Space();
            region.ShowRegion = EditorGUILayout.Toggle("Show Region", region.ShowRegion);
    
            bool allSectorsVisible = region.GetSectors().All(sector => sector.ShowSector);
            bool anySectorVisible = region.GetSectors().Any(sector => sector.ShowSector);
            bool sectorsToggleState = allSectorsVisible ? true : anySectorVisible ? false : false;
            bool newSectorsToggleState = EditorGUILayout.Toggle("Show Sectors", sectorsToggleState);
    
            if (newSectorsToggleState != sectorsToggleState)
            {
                foreach (var sector in region.GetSectors())
                    sector.ShowSector = newSectorsToggleState;
            }
    
            bool allLocationsVisible = region.GetSectors().SelectMany(sector => sector.GetLocations()).All(location => location.ShowLocation);
            bool anyLocationVisible = region.GetSectors().SelectMany(sector => sector.GetLocations()).Any(location => location.ShowLocation);
            bool locationsToggleState = allLocationsVisible ? true : anyLocationVisible ? false : false;
            bool newLocationsToggleState = EditorGUILayout.Toggle("Show Locations", locationsToggleState);

            if (newLocationsToggleState == locationsToggleState) return;
            
            foreach (var sector in region.GetSectors())
            {
                foreach (var location in sector.GetLocations())
                    location.ShowLocation = newLocationsToggleState;
            }
        }

        private void DrawOverlapCheckButton(Region region)
        {
            EditorGUILayout.Space();

            _showUtilitiesOptions = EditorGUILayout.Foldout(_showUtilitiesOptions, "Utilities", true);

            if (!_showUtilitiesOptions) return;
            
            if (GUILayout.Button("Check for Overlapping Sectors"))
                CheckForOverlappingSectors(region);

            if (GUILayout.Button("Check for Overlapping Locations"))
                CheckForOverlappingLocations(region);
        }

        private static void CheckForOverlappingSectors(Region region)
        {
            bool overlapDetected = false;

            for (int i = 0; i < region.GetSectors().Count; i++)
            {
                for (int j = i + 1; j < region.GetSectors().Count; j++)
                {
                    region.GetSectors()[i].CalculateBounds();
                    region.GetSectors()[j].CalculateBounds();
                    
                    if (!region.GetSectors()[i].Bounds.Intersects(region.GetSectors()[j].Bounds)) 
                        continue;
                    
                    Debug.LogWarning($"Sectors {region.GetSectors()[i].name} and {region.GetSectors()[j].name} are overlapping.");
                    overlapDetected = true;
                }
            }

            if (!overlapDetected)
                Debug.Log("No overlapping sectors found.");
        }

        private static void CheckForOverlappingLocations(Region region)
        {
            bool overlapDetected = false;

            List<Location> allLocations = new List<Location>();
            foreach (var sector in region.GetSectors())
                allLocations.AddRange(sector.GetLocations());

            for (int i = 0; i < allLocations.Count; i++)
            {
                for (int j = i + 1; j < allLocations.Count; j++)
                {
                    allLocations[i].CalculateBounds();
                    allLocations[j].CalculateBounds();
                    
                    if (!allLocations[i].Bounds.Intersects(allLocations[j].Bounds)) 
                        continue;
                    
                    Debug.LogWarning($"Locations {allLocations[i].name} and {allLocations[j].name} are overlapping.");
                    overlapDetected = true;
                }
            }

            if (!overlapDetected)
                Debug.Log("No overlapping locations found.");
        }

        protected override void CalculateBoundsFromRenderers()
        {
            // Cast the target as a Region
            Region region = (Region)target;
            
            if (region.GetSectors().Count == 0)
            {
                Debug.LogWarning("No Location components found in the Sector.");
                return;
            }

            region.CalculateBounds();

            EditorSceneManager.MarkSceneDirty(region.gameObject.scene);
            Debug.Log("All locations has been calculated.");
        }

        protected override void DrawAutomaticallyCalculatedBoundsButton()
        {
            EditorGUILayout.Space();
            
            if (GUILayout.Button("Calculate Bounds from Locations"))
                CalculateBoundsFromRenderers();
        }
    }
}
#endif