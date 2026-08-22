using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ActiveRagdoll{
public class FerrisWheelScript : MonoBehaviour
{
    public Animator myAnimator;
    public float speed;
    // Start is called before the first frame update
    void Start()
    {
        myAnimator.speed = 0;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartAnim(){
        myAnimator.speed = speed;
    }

    public void PauseAnim(){
        myAnimator.speed = 0;
    }
}
}