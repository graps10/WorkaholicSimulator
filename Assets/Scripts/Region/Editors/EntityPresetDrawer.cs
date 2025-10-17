using Entities;
using UnityEditor;
using UnityEngine;

namespace Region.Editors
{
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(EntitySpawnPreset))]
    public class EntityPresetDrawer : PropertyDrawer
    {
        private const string FoldoutKey = "EntityPresetFoldout";
        private const string ButtonFoldoutKey = "ButtonFoldout";
        
        private static Color warningColor = new Color(1f, 0.3f, 0.3f);
        
        private bool _isFoldout = true;
        private bool _isButtonFoldout = true;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Color defaultGUIColor = GUI.color;

            EditorGUI.BeginProperty(position, label, property);

            string foldoutKey = $"{FoldoutKey}_{property.propertyPath}";
            _isFoldout = EditorPrefs.GetBool(foldoutKey, true);

            string buttonFoldoutKey = $"{ButtonFoldoutKey}_{property.propertyPath}";
            _isButtonFoldout = EditorPrefs.GetBool(buttonFoldoutKey, true);

            var transformsProp = property.FindPropertyRelative("transforms");
            var moldProp = property.FindPropertyRelative("mold");
            var activatedObjectProp = property.FindPropertyRelative("activatedObject");
            var onPressProp = property.FindPropertyRelative("OnPress");
            var onReleaseProp = property.FindPropertyRelative("OnRelease");

            string elementName = "Element";
            if (transformsProp.arraySize > 0)
            {
                var transform = transformsProp.GetArrayElementAtIndex(0).objectReferenceValue;
                if (transform != null)
                    elementName = transform.name;

                if (transformsProp.arraySize > 1)
                    elementName += $"   and {transformsProp.arraySize - 1} others";
            }

            bool isTransformEmpty = transformsProp.arraySize == 0 || ContainsNull(transformsProp);
            bool isMoldEmpty = moldProp.objectReferenceValue == null;

            if (isTransformEmpty || isMoldEmpty) GUI.color = warningColor;

            Rect foldoutRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            _isFoldout = EditorGUI.Foldout(foldoutRect, _isFoldout, elementName, true);
            EditorPrefs.SetBool(foldoutKey, _isFoldout);

            GUI.color = defaultGUIColor;

            if (_isFoldout)
            {
                EditorGUI.indentLevel++;

                float lineHeight = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                Rect currentRect = new Rect(position.x, position.y + lineHeight, position.width, EditorGUIUtility.singleLineHeight);

                if (isTransformEmpty) GUI.color = warningColor;

                EditorGUI.PropertyField(currentRect, transformsProp, true);
                currentRect.y += EditorGUI.GetPropertyHeight(transformsProp);

                GUI.color = defaultGUIColor;

                currentRect.y += EditorGUIUtility.standardVerticalSpacing;

                if (isMoldEmpty) GUI.color = warningColor;

                EditorGUI.PropertyField(currentRect, moldProp, new GUIContent("Mold"));
                currentRect.y += lineHeight;

                GUI.color = defaultGUIColor;
                EditorGUI.indentLevel--;
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = EditorGUIUtility.singleLineHeight;

            string foldoutKey = $"{FoldoutKey}_{property.propertyPath}";
            bool isFoldout = EditorPrefs.GetBool(foldoutKey, true);

            string buttonFoldoutKey = $"{ButtonFoldoutKey}_{property.propertyPath}";
            _isButtonFoldout = EditorPrefs.GetBool(buttonFoldoutKey, true);

            if (isFoldout)
            {
                var transformsProp = property.FindPropertyRelative("transforms");
                height += EditorGUI.GetPropertyHeight(transformsProp);
                height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                height += EditorGUIUtility.singleLineHeight * 0.5f;
            }

            return height;
        }

        private static bool ContainsNull(SerializedProperty array)
        {
            for (int i = 0; i < array.arraySize; i++)
                if (array.GetArrayElementAtIndex(i).objectReferenceValue == null)
                    return true;
            
            return false;
        }
    }
#endif
}