using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class CharacterSelector : NetworkBehaviour
{
    public GameObject groundController;
    public GameObject overseerController;

    private void Start()
    {
        if (NetworkCustom.isServer)
        {
            groundController.SetActive(true);
        }
        else
        {
            overseerController.SetActive(true);
        }
    }
}
