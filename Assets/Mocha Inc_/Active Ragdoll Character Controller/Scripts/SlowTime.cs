using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ActiveRagdoll{
public class SlowTime : MonoBehaviour
{
    float fixedDeltaTime;
    // Start is called before the first frame update
    void Awake()
    {
        this.fixedDeltaTime = Time.fixedDeltaTime;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown("u") && Time.timeScale >=.2f){
            Time.timeScale = Time.timeScale/1.25f;
            Time.fixedDeltaTime = this.fixedDeltaTime * Time.timeScale;
        }
        else if(Input.GetKeyDown("i") && Time.timeScale <1f){
            Time.timeScale = Time.timeScale*1.25f;
            Time.fixedDeltaTime = this.fixedDeltaTime * Time.timeScale;
        }
    }
}
}