//
// Custom editor class for Stream
//
using UnityEngine;
using UnityEditor;

namespace Kvant
{
    [CustomEditor(typeof(Stream)), CanEditMultipleObjects]
    public class StreamEditor : Editor
    {
        SerializedProperty _maxParticles;
        SerializedProperty _footprintTex;
        SerializedProperty _footPosition;
        SerializedProperty _cavePosition;
        SerializedProperty _debug;
        static GUIContent _textCenter = new GUIContent("Center");

        void OnEnable()
        {
            _maxParticles = serializedObject.FindProperty("_maxParticles");
            _footprintTex = serializedObject.FindProperty("_footprintTex");
            _footPosition = serializedObject.FindProperty("_footPosition");
            _cavePosition = serializedObject.FindProperty("_cavePosition");
            _debug = serializedObject.FindProperty("_debug");
        }

        public override void OnInspectorGUI()
        {
            var targetStream = target as Stream;

            serializedObject.Update();

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(_maxParticles);
            if (!_maxParticles.hasMultipleDifferentValues)
            {
                EditorGUILayout.LabelField(" ", "Allocated: " + targetStream.maxParticles, EditorStyles.miniLabel);
                EditorGUILayout.LabelField(" ", targetStream.BufferWidth + "x" + targetStream.BufferHeight, EditorStyles.miniLabel);
            }
            if (EditorGUI.EndChangeCheck())
                targetStream.NotifyConfigChange();

            EditorGUILayout.PropertyField(_footprintTex);

            EditorGUILayout.LabelField("Emitter", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_footPosition, _textCenter);
            EditorGUILayout.PropertyField(_cavePosition, _textCenter);

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(_debug);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
