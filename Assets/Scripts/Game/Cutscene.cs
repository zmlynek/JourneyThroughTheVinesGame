using System.Collections;
using UnityEngine;

//Class to hold relevant details for each cutscene i.e. duration, dialogue, camera position, etc. and all relevant methods/references 
public class Cutscene
{
    [SerializeField] Dialogue dialogue; 
    public Transform cameraPos;
    public Transform companionPos;

    public IEnumerator StartCutscene()
    {
        //Set camera position and show dialogue with time in between
        yield return new WaitForSeconds(2f);
    }

}
