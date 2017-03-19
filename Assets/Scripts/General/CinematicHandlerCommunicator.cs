using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CinematicHandlerCommunicator : MonoBehaviour {

	void onCinematicFinish()
    {
        GameState.Instance.GetComponent<CinematicHandler>().onCinematicFinish();
    }
}
