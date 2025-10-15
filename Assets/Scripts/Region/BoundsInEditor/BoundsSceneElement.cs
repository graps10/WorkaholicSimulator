using Core.Enums;
using Core.Utilities;
using Entities.EditorSceneElements;
using UnityEngine;

namespace Region.BoundsInEditor
{
    public abstract class BoundsSceneElement : EditorSceneElement
    {
        private const string Created_Bounds_GameObject_Name = "BoundsMesh";
        
        [SerializeField] protected Location myLocation;

        [SerializeField] protected MeshFilter myMeshFilter;

        private MeshRenderer _myMeshRenderer;

        public bool IsVisibleBounds
        {
            get
            {
                if (_myMeshRenderer != null) 
                    return _myMeshRenderer.enabled;
            
                Debug.LogWarning("MyMeshRenderer is null so return false");
                return false;
            }
        }

#if UNITY_EDITOR

        protected override void Start()
        {
            base.Start();
            SwitchVisible(false);
        }

        public void SwitchVisible(bool value)
        {
            if (_myMeshRenderer == null)
                _myMeshRenderer = GetComponent<MeshRenderer>();

            if (_myMeshRenderer == null)
            {
                Debug.LogWarning(gameObject.name + " have no MeshRenderer");
                return;
            }

            _myMeshRenderer.enabled = value;
        }
#endif

        public abstract void CreateMeshBounds();

        public static T Create<T>(Location location, Material material = null, Transform parent = null) where T : BoundsSceneElement
        {
            var createdBoundsSceneElement = new GameObject(Created_Bounds_GameObject_Name, 
                typeof(MeshFilter), typeof(MeshRenderer), typeof(T)).GetComponent<T>();

            createdBoundsSceneElement.transform.SetParent(parent);
            createdBoundsSceneElement.transform.localPosition = Vector3.zero;
            createdBoundsSceneElement.myLocation = location;
            createdBoundsSceneElement.myMeshFilter = createdBoundsSceneElement.GetComponent<MeshFilter>();

            if(material != null)
                createdBoundsSceneElement.GetComponent<Renderer>().material = material;

            createdBoundsSceneElement.gameObject.layer = UnityLayers.Bounds.GetIndex();

            return createdBoundsSceneElement;
        }
        
        public MeshFilter GetMeshFilter() => myMeshFilter;
        public MeshRenderer GetMeshRenderer() => _myMeshRenderer;
    }
}
