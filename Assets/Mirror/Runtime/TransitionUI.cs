using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mirror
{
    public class TransitionUI : MonoBehaviour
    {
        public static TransitionUI instance;
        public float TranstionTime = 1;

        Animator anim;

        // Start is called before the first frame update
        void Start()
        {
            
            if (instance == null)
                instance = this;
            else
                Destroy(gameObject);

            anim = GetComponent<Animator>();

            DontDestroyOnLoad(gameObject);
        }

        public void FadeIn()
        {
            anim.Play("in");
        }

        public void FadeOut()
        {
            anim.Play("out");
        }


    }
}
