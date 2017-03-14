using UnityEditor;
using UnityEngine;

namespace Dissonance.Integrations.UNet_LLAPI.Editor
{
    [CustomEditor(typeof(UNetCommsNetwork))]
    [CanEditMultipleObjects]
    public class UNetCommsNetworkEditor
        : UnityEditor.Editor
    {
        private Texture2D _logo;

        private int _maxConnections;
        private SerializedProperty _maxConnectionsProperty;

        private int _port;
        private SerializedProperty _portProperty;

        public void Awake()
        {
            _logo = Resources.Load<Texture2D>("dissonance_logo");
        }

        public void OnEnable()
        {
            _maxConnectionsProperty = serializedObject.FindProperty("_maxConnections");
            _maxConnections = _maxConnectionsProperty.intValue;

            _portProperty = serializedObject.FindProperty("_port");
            _port = _portProperty.intValue;
        }

        public override void OnInspectorGUI()
        {
            GUILayout.Label(_logo);

            _port = EditorGUILayout.DelayedIntField("Port", _port);
            if (_port >= ushort.MaxValue)
                EditorGUILayout.HelpBox("Port must be between 0 and 65535", MessageType.Error);
            else
                _portProperty.intValue = _port;

            _maxConnections = EditorGUILayout.DelayedIntField("Max Connections", _maxConnections);
            if (_maxConnections < 0)
                EditorGUILayout.HelpBox("Max connections must be > 0", MessageType.Error);
            else
                _maxConnectionsProperty.intValue = _maxConnections;

            serializedObject.ApplyModifiedProperties();
        }
    }
}