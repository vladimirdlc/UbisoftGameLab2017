using System.Linq;
using Dissonance.Config;
using UnityEditor;
using UnityEngine;

namespace Dissonance
{
    [CustomEditor(typeof (VoiceBroadcastTrigger))]
    public class VoiceBroadcastTriggerEditor : Editor
    {
        private Texture2D _logo;
        private ChatRoomSettings _roomSettings;

        private readonly TokenControl _tokenEditor = new TokenControl("This broadcast trigger will only send voice if the local player has at least one of these access tokens");

        public void Awake()
        {
            _logo = Resources.Load<Texture2D>("dissonance_logo");
            _roomSettings = ChatRoomSettings.Load();
        }

        public override void OnInspectorGUI()
        {
            GUILayout.Label(_logo);

            var transmitter = (VoiceBroadcastTrigger) target;

            ChannelTypeGui(transmitter);

            EditorGUILayout.Space();
            PositionalAudioGui(transmitter);

            EditorGUILayout.Space();
            PriorityGui(transmitter);

            EditorGUILayout.Space();
            ActivationModeGui(transmitter);

            EditorGUILayout.Space();
            _tokenEditor.DrawInspectorGui(transmitter);

            EditorGUILayout.Space();
            TriggerActivationGui(transmitter);

            //Repaint the scene so that gizmos update
            if (GUI.changed)
                EditorUtility.SetDirty(target);
        }

        private void ChannelTypeGui(VoiceBroadcastTrigger transmitter)
        {
            transmitter.ChannelType = (CommTriggerTarget)EditorGUILayout.EnumPopup("Channel Type", transmitter.ChannelType);

            if (transmitter.ChannelType == CommTriggerTarget.Player)
            {
                transmitter.PlayerId = EditorGUILayout.TextField("Recipient Player Name", transmitter.PlayerId);
                EditorGUILayout.HelpBox("Player mode sends voice data to the specified player.", MessageType.None);
            }

            if (transmitter.ChannelType == CommTriggerTarget.Room)
            {
                var roomNames = _roomSettings.Names;

                var haveRooms = roomNames.Count > 0;
                if (haveRooms)
                {
                    EditorGUILayout.BeginHorizontal();

                    var selectedIndex = string.IsNullOrEmpty(transmitter.RoomName) ? 0 : roomNames.IndexOf(transmitter.RoomName);
                    selectedIndex = EditorGUILayout.Popup("Chat Room", selectedIndex, roomNames.ToArray());

                    if (GUILayout.Button("Config Rooms"))
                        ChatRoomSettingsEditor.GoToSettings();

                    EditorGUILayout.EndHorizontal();

                    if (selectedIndex >= 0)
                        transmitter.RoomName = roomNames[selectedIndex];
                    else
                        EditorGUILayout.HelpBox(string.Format("Room '{0}' is no longer defined in the chat room configuration! \nRe-create the '{0}' room, or select a different room.", transmitter.RoomName), MessageType.Warning);
                }
                else
                {
                    if (GUILayout.Button("Create New Rooms"))
                        ChatRoomSettingsEditor.GoToSettings();
                }

                EditorGUILayout.HelpBox("Room mode sends voice data to all players in the specified room.", MessageType.None);

                if (!haveRooms)
                    EditorGUILayout.HelpBox("No rooms are defined. Click 'Create New Rooms' to configure chat rooms.", MessageType.Warning);
            }

            if (transmitter.ChannelType == CommTriggerTarget.Self)
            {
                EditorGUILayout.HelpBox(
                    "Self mode sends voice data to the DissonancePlayer attached to this entity.",
                    MessageType.None);

                var player = transmitter.GetComponent<IDissonancePlayer>();
                if (player == null)
                {
                    EditorGUILayout.HelpBox(
                        "This entity has no Dissonance player component!",
                        MessageType.Error);
                }
                else if (Application.isPlaying && player.Type == NetworkPlayerType.Local)
                {
                    EditorGUILayout.HelpBox(
                        "This is the local player.\n" +
                        "Are you sure you mean to broadcast to the local player?",
                        MessageType.Warning);
                }
            }
        }

        private static void PositionalAudioGui(VoiceBroadcastTrigger transmitter)
        {
            transmitter.BroadcastPosition = EditorGUILayout.Toggle("Use Positional Data", transmitter.BroadcastPosition);
            EditorGUILayout.HelpBox(
                "Send audio on this channel with positional data to allow 3D playback if set up on the receiving end. There is no performance cost to enabling this.\n\n" +
                "Please see the Dissonance documentation for instructions on how to set your project up for playback of 3D voice comms.",
                MessageType.Info);
        }

        private static void PriorityGui(VoiceBroadcastTrigger transmitter)
        {
            transmitter.Priority = (ChannelPriority)EditorGUILayout.EnumPopup("Priority", transmitter.Priority);
            EditorGUILayout.HelpBox("Priority for the voice sent from this room. Voices will mute all lower priority voices on the receiver while they are speaking.\n\n" +
                                    "'None' means that this room specifies no particular priority and the priority of this player will be used instead", MessageType.Info);
        }

        private static void ActivationModeGui(VoiceBroadcastTrigger transmitter)
        {
            transmitter.Mode = (CommActivationMode)EditorGUILayout.EnumPopup("Activation Mode", transmitter.Mode);

            if (transmitter.Mode == CommActivationMode.None)
            {
                EditorGUILayout.HelpBox(
                    "While in this mode no voice will ever be transmitted",
                    MessageType.Info);
            }

            if (transmitter.Mode == CommActivationMode.PushToTalk)
            {
                transmitter.InputName = EditorGUILayout.TextField("Input Axis Name", transmitter.InputName);

                EditorGUILayout.HelpBox(
                    "Define an input axis in Unity's input manager if you have not already.",
                    MessageType.Info);
            }
        }

        private static void TriggerActivationGui(VoiceBroadcastTrigger transmitter)
        {
            using (var toggle = new EditorGUILayout.ToggleGroupScope("Trigger Activation", transmitter.UseTrigger))
            {
                transmitter.UseTrigger = toggle.enabled;

                if (transmitter.UseTrigger)
                {
                    if (!transmitter.gameObject.GetComponents<Collider>().Any(c => c.isTrigger))
                        EditorGUILayout.HelpBox("Cannot find any collider triggers attached to this entity.", MessageType.Warning);
                    if (!transmitter.gameObject.GetComponents<Rigidbody>().Any())
                        EditorGUILayout.HelpBox("Cannot find a RigidBody attached to this entity (required for triggers to work).", MessageType.Warning);
                }

                EditorGUILayout.HelpBox(
                    "Use trigger activation to only broadcast when the player is inside a trigger volume.",
                    MessageType.Info);
            }
        }
    }
}
