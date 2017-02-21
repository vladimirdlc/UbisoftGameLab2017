using UnityEngine;
using System.Collections;


namespace MalbersAnimations
{
    public class StepTrigger : MonoBehaviour
    {
        StepsManager _StepsManager;
        [HideInInspector]
        public AudioSource StepAudio;
        [HideInInspector]
        public string TextureName;
        bool hastrack; // If has a track don't put another
        bool waitrack; 

        public bool Hastrack
        {
            get { return hastrack; }
            set { hastrack = value; }
        }

        void Awake()
        {
            _StepsManager = GetComponentInParent<StepsManager>();
            StepAudio = gameObject.AddComponent<AudioSource>();
        }


        void OnTriggerEnter(Collider other)
        {
            if (!waitrack && _StepsManager)
            {
                //Wait half a secod before making another step
                 StartCoroutine(WaitForStep(0.5f));
                _StepsManager.EnterStep(this);
                hastrack = true;
            }
        }

        void OnTriggerExit(Collider other)
        {
            hastrack = false;
        }


        IEnumerator WaitForStep(float seconds)
        {
            waitrack =  true;
            yield return new WaitForSeconds(seconds);
            waitrack = false;
        }
    }
}