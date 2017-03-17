using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// For general purpose behaviour that needs to know game states
/// </summary>
public class TimeManagerDependable : MonoBehaviour
{
    // What will happen when any of the triggering game states evaluated as true
    public enum RequiredAction { Destroy, Deactivate, TurnOffAudiosource, StopAudioSource, TurnOnScript, TurnOffScript }

    // Whatever collection of game states will trigger the desired behaviour/action
    public TimeManager.GameState[] m_TriggeringGameStates;

    public bool m_FindMonobehaviourOnStart;
    public enum TypeOf { VHSEffect }
    public TypeOf m_MonobehaviourTypeToFind;

    // What needs to happen when the game state evaluates as true
    public RequiredAction m_RequiredAction;

    // Optional public references
    public MonoBehaviour m_RelatedScript;

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
        TimeManager.GameState frameGameState = m_TimeManager.m_GameState;

        if (CheckForGameState(frameGameState))
        {
            switch (m_RequiredAction)
            {
                case RequiredAction.Destroy:
                    Destroy(gameObject);
                    break;

                case RequiredAction.Deactivate:
                    gameObject.SetActive(false);
                    break;

                case RequiredAction.TurnOffAudiosource:
                    m_AudioSource.enabled = false;
                    break;

                case RequiredAction.StopAudioSource:
                    m_AudioSource.Stop();
                    break;

                case RequiredAction.TurnOnScript:
                    m_RelatedScript.enabled = true;
                    break;

                case RequiredAction.TurnOffScript:
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
