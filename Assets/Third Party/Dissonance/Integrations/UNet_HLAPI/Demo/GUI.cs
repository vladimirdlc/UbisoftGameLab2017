using UnityEngine;
using System.Collections;

namespace Dissonance.Integrations.UNet_HLAPI.Demo
{
    public class GUI : MonoBehaviour
    {
        private void OnGUI()
        {
            GUILayout.Label("Players:");

            var comms = FindObjectOfType<DissonanceComms>();
            if (comms.isActiveAndEnabled)
            {
                foreach (var player in comms.Players)
                {
                    if (!player.IsConnected)
                        continue;

                    if (player.Name == comms.LocalPlayerName)
                        GUILayout.Label("> '" + player.Name + "' (Local)");
                    else
                        GUILayout.Label("> '" + player.Name + "' Speaking:" + player.IsSpeaking);
                }
            }
        }
    }
}
