using System.Collections.Generic;
using TMPro;
using Unity.Collections.LowLevel.Unsafe;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using static UnityEngine.Rendering.DebugUI;

public enum GameState { GameStart, FreeRoam, Battle, MapMenu, Dialogue, Cutscene}

//This class is the most important as it handles each state of the game, allowing each class to have control at the proper time, and organizing the order in which events happen 
//Added to invisible game object that is called GameController
public class GameController : MonoBehaviour
{
    [SerializeField] BattleSystem battleSystem; 
    [SerializeField] PlayerController playerController;
    [SerializeField] CompanionController companion;
    [SerializeField] Camera worldCam;
    [SerializeField] Transform checkpoint;

    [SerializeField] UIHandler uiHandler;
    [SerializeField] GameObject gameUI; //to set visible or not
    public MenuButtons menuUI; //to check for event
    public MenuButtons backUI; //to check for event

    //prefabs to control which character was chosen from character selection
    public GameObject selectedCharacter;
    public GameObject player;

    private Sprite playerSprite; //correct sprite chosen from character selection
    //need reference to correct animator based on character selection

    GameState state = GameState.GameStart; //Initial game state

    [SerializeField] List<Enemy> enemyList = new List<Enemy>();
    private Enemy CurrentEnemy { get; set; }

    public void Start()
    {    
        //uiDocument = gameUI.GetComponent<UIDocument>();
        playerSprite = selectedCharacter.GetComponent<SpriteRenderer>().sprite;
        player.GetComponent<SpriteRenderer>().sprite = playerSprite;

        //Save sprite chosen from selected character ;
        //logic should change to chooses a prefab of the correct class, 4 prefabs with each class respectively will be made. 
        //PrefabUtility.SaveAsPrefabAsset(player, "Assets/Prefabs/Player.prefab"); //does not work because conflict with animator

        playerController.OnEncounter += (Collider2D enemyCollider) =>
        {
            if (enemyCollider.TryGetComponent<Enemy>(out var enemy))
            {
                CurrentEnemy = enemy;
                StartBattle(enemy);
            }
        };

        battleSystem.OnBattleOver += EndBattle;

        playerController.OnCutscene += (Collider2D cutsceneCollider) =>
        {
            var cutscene = cutsceneCollider.GetComponentInParent<Cutscene>();
            if (cutscene != null)
            {
                state = GameState.Cutscene;
                StartCoroutine(cutscene.StartCutscene());
            }
        }; ;

        DialogueManager.Instance.OnShowDialogue += () =>
        {
            state = GameState.Dialogue;
        };

        DialogueManager.Instance.OnCloseDialogue += () =>
        {
            if (state == GameState.Dialogue)
                //LoadUIStats();
                state = GameState.FreeRoam;
        };

        menuUI.OnEnterMenu += () =>
        {
            state = GameState.MapMenu;
        };
        backUI.OnExitMenu += () =>
        {
            state = GameState.FreeRoam;
        };
    }

    void LoadUIStats()
    {
        //set UI values to player values
        uiHandler.UpdateHP(playerController.PlayerChar.Health);
        uiHandler.UpdateDEF(playerController.PlayerChar.Defense);
        uiHandler.UpdateATK(playerController.PlayerChar.Attack);
        uiHandler.UpdateCompanionHP(companion.MaxHealth);
        uiHandler.UpdateClassAbilityCooldown(playerController.PlayerChar.Abilities[0].Cooldown);
        uiHandler.UpdateHealAbilityCooldown(playerController.PlayerChar.Abilities[1].Cooldown);
    }

    void CompanionStartGame() //Starting 'cutscene' ; game opens with dialogue not entered by player
    {
        companion.OnStartGame(); // Did not work at all, might be due to time while loading scene
        companion.GetComponent<Interactable>()?.Interact();
    }

    void StartBattle(Enemy enemy)
    {
        state = GameState.Battle;

        battleSystem.StartBattle(enemy);
    }
    void EndBattle(bool won) //called from battle system ; returns the boolean of who won (true if player won)
    {       
        //reset to last checkpoint if lost
        if (!won)
        {
            ResetToLastCheckPoint();
        }

        //Return to freeRoam after end of battle execution
        state = GameState.FreeRoam;
    }

    void ResetToLastCheckPoint()
    {
        //set player and movepoint positions
        playerController.movePoint.position = checkpoint.position;
        player.transform.position = checkpoint.position;

        //also set companion and companion movepoint positions
        var tileToRightOfCheckPoint = new Vector3(checkpoint.position.x, checkpoint.position.y - 1);
        companion.transform.position = tileToRightOfCheckPoint;
        companion.movePoint.position = tileToRightOfCheckPoint;
    }

    void EnterCutscene()
    {
        state = GameState.Cutscene;
        //Show cutscene
    }

    private void Update() //Handles game states (try to remove as much as possible)
    {
        if (state == GameState.FreeRoam)
        {
            //these statements should only execute once
            if(!gameUI.activeSelf)
                gameUI.SetActive(true);
            if (CurrentEnemy != null)
                CurrentEnemy = null;

            //these statements should execute while in free roam
            playerController.HandleUpdateFreeRoam();
            companion.HandleUpdateFreeRoam();
            StartCoroutine(uiHandler.HandleAbilityTimerUpdate());
            foreach (var enemy in enemyList)
            {
                enemy.HandleUpdate();
            }
        }
        else if (state == GameState.GameStart) //On start load UI info and start companion dialogue
        {
            LoadUIStats();
            CompanionStartGame();
        }
        else if (state == GameState.Battle) //Handle all updates during battle  (maybe move to battle system)
        {
            playerController.HandleUpdateBattle();
            companion.HandleUpdateBattle(CurrentEnemy);
            battleSystem.HandleUpdate();
            StartCoroutine(uiHandler.HandleAbilityTimerUpdate());
            CurrentEnemy.HandleUpdateBattle();
        }
        else if (state == GameState.MapMenu) //Handle menu updates then store upgrade/item info
        {
            //update skills / stats with new values from skill tree
            //mapMenu.HandleUpdate();
        }
        else if (state == GameState.Dialogue) //Let the dialogue play line by line while disallowing the player to move
        {
            gameUI.SetActive(false);
            DialogueManager.Instance.HandleUpdate();
        }
    }
}


