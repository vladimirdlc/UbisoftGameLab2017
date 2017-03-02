using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamaraSelectiveShaderAttachment : MonoBehaviour {

    /// <summary>
    /// This script will have take the camera attached to its gameobject and set an alternative
    /// render type for said shader based on  the string tag of it's subshaders.
    /// WARNING DID NOT WORK PRECISELY LIKE I THOUGHT IT WOULD
    /// </summary>
    public Shader shader;
    public string renderTypeTag;

    Camera camera;

	// Use this for initialization
	void Start () {
        camera = GetComponent<Camera>();
        camera.SetReplacementShader(shader, renderTypeTag);
	}
}
