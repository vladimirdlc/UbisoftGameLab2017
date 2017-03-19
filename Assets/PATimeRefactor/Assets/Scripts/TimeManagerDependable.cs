using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// For general purpose behaviour that needs to know game states
/// </summary>
public class TimeManagerDependable : MonoBehaviour
{
    // What will happen when any of the triggering game states evaluated as true
    // DO NOT ADD ENUMS AT THE BEGGINING ONLY RENAME OR ADD ENUMS AT THE END, IT WILL BREAK THE EXISTING PREFABS
    // THAT USE THIS
    public enum RequiredAction
    {
        DestroyGameObject, ActivateGameObject, DeactivateGameObject, TurnOffAudiosource, StopAudioSource,
        TurnOnMonobehaviour, TurnOffMonobehaviour
    }

    // Whatever collection of game states will trigger the desired behaviour/action
    public TimeManager.GameState[] m_TriggeringGameStates;

    public bool m_FindMonobehaviourOnStart;
    public enum TypeOf { VHSEffect }
    public TypeOf m_MonobehaviourTypeToFind;

    // What needs to happen when the game state evaluates as true
    public RequiredAction m_RequiredAction;

    // Optional public references
    public MonoBehaviour m_RelatedScript;
    public GameObject m_RelatedGameobject;

    // Necessary references
    private TimeManager m_TimeManager;

    // Optional references
    private AudioSource m_AudioSource;

    // Use this for initialization
    void Start()
    {
        m_TimeManager = GameObject.FindGameObjectWithTag("Time Manager").GetComponent<TimeManager>();

        if (m_RequiredAction == RequiredAction.TurnOffAudiosource)
            m_AudioSource = GetComponent<AudioSource>();

        if (m_FindMonobehaviourOnStart)
        {
            switch (m_MonobehaviourTypeToFind)
            {
                case TypeOf.VHSEffect:
                    m_RelatedScript = GetComponent<VHSPostProcessEffect>();
                    break;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Don't perform superfluous actions
        if (m_RequiredAction == RequiredAction.ActivateGameObject && m_RelatedGameobject.activeSelf)
            return;
        if (m_RequiredAction == RequiredAction.DeactivateGameObject && !m_RelatedGameobject.activeSelf)
            return;


            TimeManager.GameState frameGameState = m_TimeManager.m_GameState;

        if (CheckForGameState(frameGameState))
        {
            switch (m_RequiredAction)
            {
                case RequiredAction.DestroyGameObject:
                    Destroy(gameObject);
                    break;

                case RequiredAction.DeactivateGameObject:
                        m_RelatedGameobject.SetActive(false);
                    break;

                case RequiredAction.ActivateGameObject:
                    m_RelatedGameobject.SetActive(true);
                    break;

                case RequiredAction.TurnOffAudiosource:
                    m_AudioSource.enabled = false;
                    break;

                case RequiredAction.StopAudioSource:
                    m_AudioSource.Stop();
                    break;

                case RequiredAction.TurnOnMonobehaviour:
                    m_RelatedScript.enabled = true;
                    break;

                case RequiredAction.TurnOffMonobehaviour:
                    m_RelatedScript.enabled = false;
                    break;
            }
        }
    }

    /// <summary>
    /// Looping through game states and returning true if any of them exist
    /// </summary>
    /// <param name="searchedGameState"></param>
    /// <returns></returns>
    private bool CheckForGameState(TimeManager.GameState searchedGameState)
    {
        bool result = false;

        foreach (TimeManager.GameState comparedGameState in m_TriggeringGameStates)
        {
            if (comparedGameState == searchedGameState)
            {
                result = true;
                break;
            }
        }

        return result;
    }
}
