using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ActiveRagdoll
{
    public class showSkinnedMeshScript : MonoBehaviour
{
    public MeshRenderer[] baseLimbRenderers;

    public SkinnedMeshRenderer skinnedMeshRenderer;

    public bool isUsingSkinnedMesh = true;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown("x")){
            if(isUsingSkinnedMesh){
                isUsingSkinnedMesh = false;
                skinnedMeshRenderer.enabled = false;
                foreach(MeshRenderer meshRenderer in baseLimbRenderers){
                    meshRenderer.enabled = true;
                }
            }
            else{
                isUsingSkinnedMesh = true;
                skinnedMeshRenderer.enabled = true;
                foreach(MeshRenderer meshRenderer in baseLimbRenderers){
                    meshRenderer.enabled = false;
                }
            }
        }
    }
}
    
}
