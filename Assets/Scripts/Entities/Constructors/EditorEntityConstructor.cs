using System.Collections.Generic;
using Entities.EditorSceneElements;
using Entities.Molds;
using Region;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.ProBuilder;

namespace Entities.Constructors
{
    [ExecuteInEditMode]  // Attribute for executing in editor mode
    public class EditorEntityConstructor : ObjectConstructor<GameObject, Mold>
    {
        private const string Postfix_For_Editor = "_EDITOR-Entity";
        private const string Tag_For_Editor = "EditorOnly";
        
        private const string Show_All_Editor_Entities_Menu_Item_Path = "Simulator/EditorEntities/Show all EditorEntities";

        private static readonly List<System.Type> requiredComponentsTypes = new()
        {
            typeof(Transform),
            typeof(RectTransform),
            typeof(MeshRenderer),
            typeof(Canvas),
            typeof(CanvasRenderer),
            typeof(EditorEntity),
            typeof(MeshFilter),
            typeof(SkinnedMeshRenderer),
            typeof(ProBuilderMesh),
            typeof(TextMeshPro),
            typeof(TextMeshProUGUI)
            #if UNITY_EDITOR
            #endif
        };

        private static EditorEntityConstructor instance;
        public static EditorEntityConstructor Instance
        {
            get => instance ??= new EditorEntityConstructor();

            private set
            {
                if (instance == null)
                    instance = value;
                else
                    Debug.LogWarning("Trying create EditorEntityConstructor");
            }
        }

        private static Dictionary<Location, List<GameObject>> _createdElements = new();
        private Location _transferLocation;

#if UNITY_EDITOR
        
        private void CreateElements(Location location, List<EntitySpawnPreset> entitySpawnPreset)
        {
            _transferLocation = location;

            foreach (var element in entitySpawnPreset)
            {
                foreach (var parentTransform in element.transforms)
                    LoadImmediately(element.mold, parentTransform, out GameObject instance);
            }

            if(!EditorApplication.isCompiling)
                AssetDatabase.SaveAssets();
        }
        
#endif

        public override void LoadImmediately<T>(Mold moldType, Transform transform, out T result)
        {
            result = null;

            if (moldType == null)
            {
                Debug.LogError("Mold type is null!");
                return;
            }

#if UNITY_EDITOR
            
            if (_transferLocation == null)
                return;

            GameObject createdGameObject = InstantiateFromPool(moldType.PrefabPoolInfo, transform);
            if(createdGameObject == null) return;

            InitializeEntity(createdGameObject, moldType);

            createdGameObject.name = moldType.name + Postfix_For_Editor;
            createdGameObject.tag = Tag_For_Editor;

            RemoveUnnecessaryComponents(createdGameObject);

            _createdElements.TryGetValue(_transferLocation, out List<GameObject> editorGameObjects);

            if (!_createdElements.ContainsKey(_transferLocation))
            {
                editorGameObjects = new List<GameObject>();
                _createdElements.Add(_transferLocation, editorGameObjects);
                editorGameObjects.Add(createdGameObject);
            }

            result = (T)createdGameObject;
#endif
            
        }

        private static void RemoveUnnecessaryComponents(GameObject instance)
        {
            Component[] components = instance.GetComponentsInChildren<Component>();
            List<Rigidbody> rigidbodies = new List<Rigidbody>();

            foreach (var component in components)
            {
                if (component is Rigidbody rigidbody)
                    rigidbodies.Add(rigidbody);
                
                else if (!requiredComponentsTypes.Contains(component.GetType()))
                    Object.DestroyImmediate(component);
            }

            foreach (var rigidbody in rigidbodies)
                Object.DestroyImmediate(rigidbody);
            
            rigidbodies.Clear();
        }

        private void InitializeEntity(GameObject entityToInitialize, Mold mold)
        {
            
#if UNITY_EDITOR
            
            if(entityToInitialize == null || mold == null || _transferLocation == null)
                return;
            
            switch (mold)
            {
                default:
                    entityToInitialize.AddComponent<EditorEntity>().Initialize(_transferLocation);
                    break;
            }
            
#endif
        }

#if UNITY_EDITOR
        
        public static void AddExistingElement(Location location, GameObject gameObject)
        {
            if (location == null || gameObject == null)
                return;

            _createdElements.TryGetValue(location, out var editorGameObjects);

            if (editorGameObjects == null)
            {
                editorGameObjects = new();
                _createdElements.Add(location, editorGameObjects);
            }

            if (!editorGameObjects.Contains(gameObject))
                editorGameObjects.Add(gameObject);
        }

        public void RefreshLocation(Location location, List<EntitySpawnPreset> elements)
        {
            ClearAllGameObjectElements(location);
            CreateElements(location, elements);
        }

        private static void ClearAllGameObjectElements(Location location)
        {
            if (!_createdElements.TryGetValue(location, out List<GameObject> editorGameObjects)) 
                return;
            
            foreach (var item in editorGameObjects)
                Object.DestroyImmediate(item);
        }

        public void DestroyAllEditorEntities()
        {
            foreach (var location in _createdElements.Keys)
                ClearAllGameObjectElements(location);

            _createdElements.Clear();
            _createdElements = new();
        }
        

        private static bool _showEditorEntities = true;

        [MenuItem(Show_All_Editor_Entities_Menu_Item_Path, false, 2)]
        public static void ToggleAllEditorEntities()
        {
            _showEditorEntities = !_showEditorEntities;

            foreach (var locationEditorEntities in _createdElements.Values)
            {
                foreach (var editorEntity in locationEditorEntities)
                    if (editorEntity != null)
                        editorEntity.SetActive(_showEditorEntities);
            }
        }
        
        [MenuItem(Show_All_Editor_Entities_Menu_Item_Path, true)]
        public static bool ValidateAllEditorEntities()
        {
            Menu.SetChecked(Show_All_Editor_Entities_Menu_Item_Path, _showEditorEntities);
            return true;
        }
        
        public static void TryHideAllEditorEntities()
        {
            if (_showEditorEntities) return;

            foreach (var locationEditorEntities in _createdElements.Values)
            {
                foreach (var editorEntity in locationEditorEntities)
                    if (editorEntity != null)
                        editorEntity.SetActive(false);
            }
        }
        
#endif
    }
}