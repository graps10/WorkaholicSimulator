#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Region.Editors
{
    [CustomEditor(typeof(Location), true)]
    public class LocationEditor : Editor
    {
        protected virtual string iconPath => "Assets/Textures/Icons/Editor/LocationIcon.png";
        
		protected Location location;
        
        private Sector _parentSector;
        private Color _previousColor;
        
		private bool _showCustomSettings = true;
        private bool _showUnityEvents;
        
        protected virtual void OnEnable()
        {
            InitializeProperties();
            _parentSector = location.GetComponentInParent<Sector>();
        }
        
        protected virtual void InitializeProperties() => location = (Location)target;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            _previousColor = location.GetBoundsColor();
            
            DrawCustomFields("entityPresets", "boundsColor", "boundsMaterial");
            DrawCustomSettings();
            DrawUnityEvents();
            
            EditorGUILayout.Space();
            
            location.ShowLocation = EditorGUILayout.Toggle("Show Location", location.ShowLocation);

            serializedObject.ApplyModifiedProperties();

            if (_previousColor != location.GetBoundsColor())
                location.RefreshBoundsColor();
            
            if(GUI.changed)
                location.RefreshEditorEntities();
            SetCustomIcon();
        }

        private void SetLocationVisibility(bool visible)
        {
            location.ToggleDisplayBounds(visible);
            EditorUtility.SetDirty(location);
        }
        
        protected void SetCustomIcon()
        { 
            Texture2D icon = AssetDatabase.LoadAssetAtPath<Texture2D>(iconPath);
            if (icon != null) EditorGUIUtility.SetIconForObject(target, icon);
        }
        
        protected void DrawCustomFields(params string[] fields)
		{
            foreach (var field in fields)
            {
                var property = serializedObject.FindProperty(field);
                if (property == null)
                {
                    Debug.LogError($"Field with name {field} is not found. Change it's name to the appropriate class.");
                    continue;
                }
                
                EditorGUILayout.PropertyField(property, true);
            }
		}

		protected void DrawCustomSettings()
        {
            EditorGUILayout.Space();
            _showCustomSettings = EditorGUILayout.Foldout(_showCustomSettings, "Custom Location Settings", true);

            if (!_showCustomSettings) 
                return;
            
            EditorGUILayout.Space();
            DrawAutomaticallyCalculatedBoundsButton();
        }

		protected virtual void DrawAutomaticallyCalculatedBoundsButton()
        {
            EditorGUILayout.Space();
            if (!GUILayout.Button("Calculate Bounds from Renderers")) 
                return;
            
            Undo.RecordObject(_parentSector, "Calculate Bounds from Renderers");
                
            location.CalculateBounds();
                
            if(!_parentSector.GetLocations().Contains(location))
                _parentSector.GetLocations().Add(location);
                
            EditorSceneManager.MarkSceneDirty(location.gameObject.scene);
            Debug.Log("Location bounds calculated");
        }

        protected void DrawUnityEvents()
        {
            EditorGUILayout.Space();
            _showUnityEvents = EditorGUILayout.Foldout(_showUnityEvents, "Unity Events", true);

            if (_showUnityEvents)
                EditorGUILayout.HelpBox("These events are triggered when the _location is entered or exited.", MessageType.Info);
        }

        protected virtual void CalculateBoundsFromRenderers()
        {
            // Check if the location has children
            if (location.transform.childCount == 0)
            {
                Debug.LogWarning("Location has no child objects to calculate bounds from.");
                return;
            }

            // Initialize a new Bounds with zero size
            Bounds combinedBounds = new Bounds(location.transform.position, Vector3.zero);
            Renderer[] renderers = location.GetComponentsInChildren<Renderer>();

            if (renderers.Length == 0)
            {
                Debug.LogWarning("No renderers found in the location's children.");
                return;
            }

            // Iterate through each renderer to encapsulate their bounds
            foreach (Renderer renderer in renderers)
                combinedBounds.Encapsulate(renderer.bounds);

            Debug.Log("Bounds calculated, scale factor updated, and child objects adjusted.");
        }
    }
}
#endif
