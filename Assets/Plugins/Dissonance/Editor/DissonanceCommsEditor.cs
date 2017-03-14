#if !NCRUNCH

using System.Linq;
using Dissonance.Audio.Playback;
using Dissonance.Networking;
using UnityEditor;
using UnityEngine;

namespace Dissonance
{
    [CustomEditor(typeof (DissonanceComms))]
    public class DissonanceCommsEditor : Editor
    {
        private Texture2D _logo;

        private readonly TokenControl _tokenEditor = new TokenControl("These access tokens are used by broadcast/receipt triggers to determine if they should function");

        public void Awake()
        {
            _logo = Resources.Load<Texture2D>("dissonance_logo");
        }

        public override void OnInspectorGUI()
        {
            var comm = (DissonanceComms) target;

            GUILayout.Label(_logo);

            CommsNetworkGui();
            PlaybackPrefabGui(comm);

            comm.IsMuted = EditorGUILayout.Toggle("Mute", comm.IsMuted);

            EditorGUILayout.Space();

            if (Application.isPlaying)
            {
                StatusGui(comm);

                EditorGUILayout.Space();
            }

            _tokenEditor.DrawInspectorGui(comm);

            if (GUILayout.Button("Voice Settings"))
                VoiceSettingsEditor.GoToSettings();

            if (GUILayout.Button("Configure Rooms"))
                ChatRoomSettingsEditor.GoToSettings();
            
            if (GUILayout.Button("Diagnostic Settings"))
                DebugSettingsEditor.GoToSettings();
        }

        private static void PlaybackPrefabGui(DissonanceComms comm)
        {
            using (new EditorGUI.DisabledScope(Application.isPlaying))
            {
                var prefab = EditorGUILayout.ObjectField("Playback Prefab", comm.PlaybackPrefab, typeof(VoicePlayback), false);
                if (!Application.isPlaying)
                {
                    if (prefab != null && PrefabUtility.GetPrefabType(prefab) == PrefabType.Prefab)
                        comm.PlaybackPrefab = (VoicePlayback)prefab;
                    else
                        comm.PlaybackPrefab = null;
                }
            }
        }

        private void CommsNetworkGui()
        {
            var net = ((DissonanceComms)target).gameObject.GetComponent<ICommsNetwork>();
            if (net == null)
            {
                EditorGUILayout.HelpBox(
                    "Please attach a voice comm network component appropriate to your networking system to the entity.",
                    MessageType.Error);
            }
        }

        private static void StatusGui(DissonanceComms comm)
        {
            EditorGUILayout.LabelField("Local Player ID", comm.LocalPlayerName);
            EditorGUILayout.LabelField("Peers (" + comm.Players.Count + ")", comm.Players.Count == 0 ? "none": "");

            for (int i = 0; i < comm.Players.Count; i++)
            {
                var p = comm.Players[i];
                var speaking = p.IsSpeaking ? "(speaking)" : "";
                var disconnected = !p.IsConnected ? "(disconnected)" : "";
                EditorGUILayout.LabelField(string.Format(" - {0} {1} {2}", i, speaking, disconnected), p.Name);

                //If there is a player we'll set the comms object to dirty which causes the editor to be redrawn.
                //This makes the (speaking) indicator update live for players.
                EditorUtility.SetDirty(comm);
            }
        }
    }
}

#endif