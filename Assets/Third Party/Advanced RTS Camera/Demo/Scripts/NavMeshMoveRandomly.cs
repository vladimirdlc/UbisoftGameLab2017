using UnityEngine;
using System.Collections;

public class NavMeshMoveRandomly : MonoBehaviour {

    UnityEngine.AI.NavMeshAgent navMesh;

	// Use this for initialization
    void Start()
    {
        navMesh = gameObject.GetComponent<UnityEngine.AI.NavMeshAgent>();
    }
	
	// Update is called once per frame
    void Update()
    {

    }
}
