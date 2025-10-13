using System.Collections.Generic;
using Entities.Molds;
using TMPro;
using UnityEngine;
using UnityEngine.ProBuilder;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Entities.Constructors.Editor
{
    [ExecuteInEditMode] // Attribute for exicuting in editor mode
    public class EditorEntityConstructor : ObjectConstructor<GameObject, Mold>
    {
        private const string POSTFIX_FOR_EDITOR = "_EDITOR-Entity";

        private static List<System.Type> _requiredComponentsTypes = new()
        {
            typeof(Transform),
            typeof(RectTransform),
            typeof(MeshRenderer),
            typeof(Canvas),
            typeof(CanvasRenderer),
            typeof(MeshFilter),
            typeof(SkinnedMeshRenderer),
            typeof(ProBuilderMesh),
            typeof(TextMeshPro),
            typeof(TextMeshProUGUI)
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

        public override void LoadImmediately<T>(Mold entityMold, Transform transform, out T result)
        {
            throw new System.NotImplementedException();
        }
    }
}