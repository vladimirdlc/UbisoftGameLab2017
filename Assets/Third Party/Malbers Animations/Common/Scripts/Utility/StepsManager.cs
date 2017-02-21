using UnityEngine;

namespace MalbersAnimations
{
    public class StepsManager : MonoBehaviour
    {
        [System.Serializable]
        public class GroundAudio
        {
            public string tag;
            public AudioClip[] clips;
        }

        public GameObject Tracks;
        public AudioClip[] clips;


        public void EnterStep(StepTrigger foot)
        {
            RaycastHit footRay;
            if (foot.StepAudio && clips.Length > 0)
            {
                foot.StepAudio.clip = clips[Random.Range(0, clips.Length)];
                foot.StepAudio.Play();

                //Put a track and particles
                if (Tracks && !foot.Hastrack)
                {
                    if (Physics.Raycast(foot.transform.position, -transform.up, out footRay, 1, GetComponent<Animal>().GroundLayer))
                    {
                        GameObject foottrack = Instantiate(Tracks);
                        foottrack.transform.position = new Vector3(foot.transform.position.x, footRay.point.y + 0.005f, foot.transform.position.z);
                        foottrack.transform.rotation = Quaternion.FromToRotation(-foot.transform.forward, footRay.normal)* foot.transform.rotation;
                    }
                }
            }
        }
    }
}
