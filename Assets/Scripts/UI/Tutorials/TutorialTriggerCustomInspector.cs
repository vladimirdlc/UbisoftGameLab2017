using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TutorialTrigger))]
public class TutorialTriggerCustomInspector : Editor
{

    int tutorialIdex;
    TutorialTrigger myTarget;

    private static string[] tutorialOptionsGroundPlayer =
    {
        "Move Prompt", "Look Prompt"
    };

    private static string[] tutorialOptionsOverseerPlayer =
    {
        "Move Prompt", "Look Prompt"
    };

    public override void OnInspectorGUI()
    {
        myTarget = (TutorialTrigger)target;

        myTarget.m_PlayerType = (TutorialCanvas.PlayerType)EditorGUILayout.EnumPopup("Player for the tutorial:", myTarget.m_PlayerType);

        myTarget.m_TutorialIndex = EditorGUILayout.Popup("Triggered tutorial:", myTarget.m_TutorialIndex,
            myTarget.m_PlayerType == TutorialCanvas.PlayerType.Dog ? tutorialOptionsGroundPlayer : tutorialOptionsOverseerPlayer);

        // Save the changes back to the object
        EditorUtility.SetDirty(target);
    }
}
