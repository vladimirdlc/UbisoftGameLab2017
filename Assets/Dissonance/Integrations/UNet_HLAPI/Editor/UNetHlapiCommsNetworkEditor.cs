using UnityEditor;
using UnityEngine;

namespace Dissonance.Integrations.UNet_HLAPI.Editor
{
    [CustomEditor(typeof(HlapiCommsNetwork))]
    [CanEditMultipleObjects]
    public class UNetCommsNetworkEditor
        : UnityEditor.Editor
    {
        private Texture2D _logo;

        private bool _advanced;

        private int _typeCode;
        private SerializedProperty _typeCodeProperty;

        private int _reliableSequencedChannel;
        private SerializedProperty _reliableSequencedChannelProperty;

        private int _unreliableChannel;
        private SerializedProperty _unreliableChannelChannelProperty;

        public void Awake()
        {
            _logo = Resources.Load<Texture2D>("dissonance_logo");
        }

        public void OnEnable()
        {
            _typeCodeProperty = serializedObject.FindProperty("TypeCode");
            _typeCode = _typeCodeProperty.intValue;

            _reliableSequencedChannelProperty = serializedObject.FindProperty("ReliableSequencedChannel");
            _reliableSequencedChannel = _reliableSequencedChannelProperty.intValue;

            _unreliableChannelChannelProperty = serializedObject.FindProperty("UnreliableChannel");
            _unreliableChannel = _unreliableChannelChannelProperty.intValue;
        }

        public override void OnInspectorGUI()
        {
            GUILayout.Label(_logo);

            if (GUILayout.Button("Open Documentation"))
                Help.BrowseURL("https://dissonance.readthedocs.io/en/latest/Basics/Quick%20Start%20-%20UNet%20HLAPI/");

            //Set the two QoS channels
            EditorGUILayout.HelpBox("Dissonance requires 2 HLAPI QoS channels.", MessageType.Info);
            _reliableSequencedChannel = EditorGUILayout.DelayedIntField("Reliable Channel", _reliableSequencedChannel);
            _unreliableChannel = EditorGUILayout.DelayedIntField("Unreliable Channel", _unreliableChannel);
            if (_unreliableChannel < 0 || _unreliableChannel >= byte.MaxValue || _reliableSequencedChannel < 0 || _reliableSequencedChannel >= byte.MaxValue)
                EditorGUILayout.HelpBox("Channel IDs must be between 0 and 255", MessageType.Error);
            else if (_unreliableChannel == _reliableSequencedChannel)
                EditorGUILayout.HelpBox("Channel IDs must be unique", MessageType.Error);
            else
            {
                _reliableSequencedChannelProperty.intValue = _reliableSequencedChannel;
                _unreliableChannelChannelProperty.intValue = _unreliableChannel;
            }

            _advanced = EditorGUILayout.Foldout(_advanced, "Advanced Configuration");
            if (_advanced)
            {
                //Set type code
                EditorGUILayout.HelpBox("Dissonance requires a HLAPI type code. If you are not sending raw HLAPI network packets you should use the default value.", MessageType.Info);
                _typeCode = EditorGUILayout.DelayedIntField("Type Code", _typeCode);
                if (_typeCode >= ushort.MaxValue || _typeCode < 1000)
                    EditorGUILayout.HelpBox("Event code must be between 1000 and 65535", MessageType.Error);
                else
                    _typeCodeProperty.intValue = _typeCode;
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}