using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using System.Collections.Generic;

//This class is used to execute methods for buttons on the character selection menu
public class CharacterSelector : MonoBehaviour
{
    public SpriteRenderer sr;
    public List<Sprite> characters = new List<Sprite>();
    //list of classes ? 
    private int currentCharacter = 0;
    public GameObject characterChoice;

    public void NextOption()
    {
        currentCharacter++;
        if (currentCharacter == characters.Count)
        {
            currentCharacter = 0;
        }

        sr.sprite = characters[currentCharacter];
    }

    public void BackOption()
    {
        currentCharacter--;
        if (currentCharacter < 0)
        {
            currentCharacter = characters.Count - 1;
        }

        sr.sprite = characters[currentCharacter];
    }

    public void PlayGame()
    {
        //PrefabUtility.SaveAsPrefabAsset(characterChoice, "Assets/Prefabs/SelectedSkin.prefab"); //Change Player Prefab to contain correct sprite

        //New Logic: 
        //Have 4 prefabs, one for each of the classes, determine which was chosen and use that prefab in the next scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}
