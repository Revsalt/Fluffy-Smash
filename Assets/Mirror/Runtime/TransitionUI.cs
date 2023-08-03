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

        bool fade_state = false;

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
            if (fade_state) return;

            fade_state = true;
            anim.Play("in");
        }

        public void FadeOut()
        {
            if (!fade_state) return;

            fade_state = false;
            anim.Play("out");
        }


    }
}
