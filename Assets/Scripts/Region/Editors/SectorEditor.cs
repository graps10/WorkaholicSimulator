#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Region.Editors
{
    [CustomEditor(typeof(Sector))]
    [CanEditMultipleObjects]
    public class SectorEditor : LocationEditor
    {
        protected override string iconPath => "Assets/Textures/Icons/Editor/SectorIcon.png";
        
	    private Sector _sector;
	    private Region _parentRegion;
	    
	    private bool _showLocationsList = true;
	    
	    protected override void OnEnable()
	    {
		    InitializeProperties();
		    _parentRegion = _sector.GetComponentInParent<Region>();
		    
		    Undo.undoRedoPerformed += RefreshChildrenLocations;
	    }

	    private void OnDisable() => Undo.undoRedoPerformed -= RefreshChildrenLocations;

	    protected override void InitializeProperties()
	    {
		    base.InitializeProperties();
		    _sector = (Sector)target;
	    }
	    
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawLocationsList();
            DrawCustomSettings();
            DrawUnityEvents();
            RefreshChildrenLocations();

            serializedObject.ApplyModifiedProperties();

            if (GUI.changed)
	            location.RefreshEditorEntities();

            var sector = (Sector)target;
            DrawVisibilityCheckbox(sector);
            SetCustomIcon();
        }
       
        private void DrawVisibilityCheckbox(Sector sector)
        {
            sector.ShowSector = EditorGUILayout.Toggle("Show Sector", sector.ShowSector);
    
            bool allLocationsVisible = sector.GetLocations().All(location => location.ShowLocation);
            bool anyLocationVisible = sector.GetLocations().Any(location => location.ShowLocation);
            bool locationsToggleState = allLocationsVisible ? true : anyLocationVisible ? false : false;
            bool newLocationsToggleState = EditorGUILayout.Toggle("Show Locations", locationsToggleState);

            if (newLocationsToggleState == locationsToggleState) return;
            
            foreach (var location in sector.GetLocations())
	            location.ShowLocation = newLocationsToggleState;
        }
        private void DrawLocationsList()
	    {
		    EditorGUILayout.Space();
		    
		    _showLocationsList = EditorGUILayout.Foldout(_showLocationsList, "Locations List", true);
		    
		    if (!_showLocationsList) return;
		    
		    EditorGUILayout.Space();
		    
		    if (_sector.GetLocations() != null)
		    {
			    for (int i = 0; i < _sector.GetLocations().Count; i++)
			    {
				    EditorGUILayout.BeginHorizontal();
	            
				    _sector.GetLocations()[i].name = EditorGUILayout.TextField("Location " + i, _sector.GetLocations()[i].name);

				    if (GUILayout.Button("Remove", GUILayout.Width(60)))
				    {
					    Undo.RecordObject(_sector.GetLocations()[i].gameObject, "Remove Location");
					    
					    Undo.DestroyObjectImmediate(_sector.GetLocations()[i].gameObject);
					    break;
				    }
	            
				    EditorGUILayout.EndHorizontal();
			    }
		    }
		    
		    if (GUILayout.Button("Add Location"))
			    AddNewLocation();
		    
		    EditorGUILayout.Space();
	    }
	    
	    private void AddNewLocation()
	    {
		    GameObject newLocationObject = new GameObject("New Location " + (_sector.GetLocations() == null ? 0 :_sector.GetLocations().Count + 1));
		    Undo.RegisterCreatedObjectUndo(newLocationObject, "Add New Location");
		    newLocationObject.transform.SetParent(_sector.transform);
			Location newLocation = newLocationObject.AddComponent<Location>();
			newLocationObject.transform.localPosition = Vector3.zero;

			if(_sector.GetLocations() == null)
				_sector.InitializeNewLocationsList();
		    
		    _sector.GetLocations().Add(newLocation);
		    
		    newLocation.CalculateBounds();
	    }

	    private void RefreshChildrenLocations()
	    {
		    if (_sector.GetLocations() == null)
			     return;
		    
		    _sector.GetLocations().Clear();
		    
		    var locations = _sector.GetComponentsInChildren<Location>(false);
		    foreach (var item in locations)
		    {
			    if(!(item is Sector) && !_sector.GetLocations().Contains(item))
				    _sector.GetLocations().Add(item);
		    }
	    }

	    protected override void CalculateBoundsFromRenderers()
        {
	        Undo.RecordObject(_parentRegion, "Calculate Bounds from Renderers");
	        
            // Cast the target as a Sector
            _sector.CalculateBounds();
            
            if(!_parentRegion.GetSectors().Contains(_sector))
	            _parentRegion.GetSectors().Add(_sector);
            
            EditorSceneManager.MarkSceneDirty(_sector.gameObject.scene);
            Debug.Log("Sector bounds calculated, scale factor updated, and child objects adjusted.");
        }
    }
}
#endif


