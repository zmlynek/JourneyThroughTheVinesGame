using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.Rendering;
using Unity.VisualScripting;
using static UnityEngine.EventSystems.EventTrigger;
using UnityEngine.TextCore.Text;
using Unity.Collections.LowLevel.Unsafe;

public enum BattleState { Battling, BattleOver, Busy }

//Methods to calculate battle ; used by GameController
public class BattleSystem : MonoBehaviour 
{
    [SerializeField] UIHandler uiHandler;

    //cache of last known UI stats
    private int lastPlayerHealth;
    private int lastCompanionHealth;

    private DamageDetails CharDamageDetails;
    private DamageDetails CompanionDamageDetails;
    private DamageDetails EnemyDamageDetails;

    //set companion + character from unity
    public CompanionController companion;
    public Character character; 

    private Enemy Enemy { get; set; } //the current enemy involved in battle
    BattleState state;

    //set animators from unity
    public Animator playerAnimator; 
    public Animator companionAnimator;
    
    //have to get enemy each battle from current enemy passed
    private Animator enemyAnimator;

    //locks for mutual exclusion
    private bool playerisAttacking = false;
    private bool companionisAttacking = false;
    private bool enemyisAttacking = false;
    private bool decisionMutex = false;

    public event Action<bool> OnBattleOver;

    public void StartBattle(Enemy enemy)
    {
        //Update current enemy and state
        state = BattleState.Battling;
        Enemy = enemy;

        //reset companion and player hp at start of battle for now
        //reset hp within objects
        character.HP = character.Health;
        companion.HP = companion.MaxHealth;

        //reset local hp values
        lastPlayerHealth = character.Health;
        lastCompanionHealth = companion.MaxHealth;
        
        //reset health bars
        uiHandler.playerHealthBar.SetHP(lastPlayerHealth/lastPlayerHealth);
        uiHandler.companionHealthBar.SetHP(lastCompanionHealth/lastCompanionHealth);
        
        //load all information to ui
        UpdateUIStats();

        //default damage details states where everyone is alive
        CharDamageDetails = new DamageDetails()
        {
            Fainted = false,
            Critical = 1f,
            DamageTaken = 0,
        }; 

        CompanionDamageDetails = new DamageDetails()
        {
            Fainted = false,
            Critical = 1f,
            DamageTaken = 0,
        };
        
        EnemyDamageDetails = new DamageDetails()
        {
            Fainted = false,
            Critical = 1f,
            DamageTaken = 0,
        };

        //reset battle bool variables
        playerisAttacking = false;
        companionisAttacking = false;
        enemyisAttacking = false;

        //always set enemy animator as it will be different every battle
        enemyAnimator = Enemy.GetComponentInParent<Animator>();
        
        //Show battle start cutscene?? 
    }

    void UpdateUIStats()
    {
        if (lastPlayerHealth != uiHandler.HP)
        {
            uiHandler.UpdateHP(lastPlayerHealth);
        }
        if (lastCompanionHealth != uiHandler.CompanionHP)
        {
            uiHandler.UpdateCompanionHP(lastCompanionHealth);
        }
    }

    void BattleOver(bool won) //return true if player won
    {
        state = BattleState.BattleOver; //is this line necessary? seems to set state to what it already is 
        RemoveEnemyFromScene();
        OnBattleOver(won);
    }

    void RemoveEnemyFromScene()
    {
        Enemy.gameObject.SetActive(false);
        Enemy.GetComponent<SpriteRenderer>().sortingLayerName = "Default";
    }

    public void HandleUpdate() //Handles each BattleState ; called by GameController during GameState.Battle
    {
        if (state == BattleState.Battling)
        {
            HandleBattle();
        }
        else if (state == BattleState.BattleOver) //checking again here seems redundant, maybe remove state altogether and move to HandleBattle()
        {
            if (EnemyDamageDetails.Fainted) //enemy is dead
                BattleOver(true);
            else if (!EnemyDamageDetails.Fainted) //enemy not dead
                BattleOver(false);
        }
    }

    public void HandleBattle()
    {
        //handle player win lose condition
        if (!CharDamageDetails.Fainted) //check for loss condition
        { 
            //Check for player attack buttons
            if(!playerisAttacking)
                CheckForBattleKeys();

            //Allow enemy to attack if alive
            if (!enemyisAttacking && !EnemyDamageDetails.Fainted)
            StartCoroutine(DecideTargetAndAttack());

            //Allow companion to attack if alive
            if (!companionisAttacking && !CompanionDamageDetails.Fainted)
                StartCoroutine(AttackEnemy());

            //enemy and player HP bars are adjusted accordingly within take damage methods
            //so hp bar update is local, each referencing their own
            //still need to update the HP values
            UpdateUIStats();

            //Check for player win condition
            if (EnemyDamageDetails.Fainted)
                state = BattleState.BattleOver;
        }
        else { state = BattleState.BattleOver; } //enemy win ; if player has no HP enemy will have HP and therfore enemyisDead will be false 
    }

    void CheckForBattleKeys() //determine how to battle based on player input
    {
        if (!CharDamageDetails.Fainted && !playerisAttacking) //if player is alive and not already attacking
        {
            if (Input.GetKeyDown(KeyCode.R))//Ability heal 
            {
                StartCoroutine(UseHealAbility());
            }
            else if (Input.GetKeyDown(KeyCode.Q)) //Class ability
            {
                StartCoroutine(UseClassAbility());
            }
            else if (Input.GetKeyDown(KeyCode.E)) //Main attack
            {
                //add if statement to check if within range and check for which class is currently selected using distance vector
                StartCoroutine(MainAttack());
            }
        }
    }

    IEnumerator DecideTargetAndAttack() //enemy decides who to attack
    {
        if (!decisionMutex)
        {
            decisionMutex = true; //enter critical section

            if (!CharDamageDetails.Fainted && !CompanionDamageDetails.Fainted) //both player and companion are alive
            {
                var toAttack = UnityEngine.Random.value * 2; //range 0 to 2, if < 1 attack player, if > 1 attack companion
                if (toAttack < 1)
                {
                    yield return AttackPlayer();
                    yield return new WaitForSeconds(1.5f);
                }
                else if (toAttack >= 1)
                {
                    yield return AttackCompanion();
                    yield return new WaitForSeconds(1.5f);
                }
            }
            else if (!CharDamageDetails.Fainted) //only player is alive
            {
                yield return AttackPlayer();
                yield return new WaitForSeconds(1.5f);
            }
            else if (!CompanionDamageDetails.Fainted) //only companion is alive
            {
                yield return AttackCompanion();
                yield return new WaitForSeconds(1.5f);
            }

            decisionMutex = false; //exit critcal section
        }
    }

    IEnumerator AttackPlayer() //Enemy attack player
    {
        if (character != null)
        {
            //enter critical section
            enemyisAttacking = true;

            yield return new WaitForSeconds(1.5f);

            CharDamageDetails = character.TakeDamage(Enemy); //update character damagedetails information
            lastPlayerHealth = character.HP - CharDamageDetails.DamageTaken;

            //exit critical section
            enemyisAttacking = false;
        }
        else Debug.Log("BattleSystem AttackPlayer; Character is not found");
    }

    IEnumerator AttackCompanion() //Enemy attack companion
    {
        if (companion != null)
        {
            //enter critical section
            enemyisAttacking = true;

            yield return new WaitForSeconds(1.5f);

            CompanionDamageDetails = companion.TakeDamage(Enemy); //update companion damagedetails information
            lastCompanionHealth = companion.HP - CompanionDamageDetails.DamageTaken;

            //exit critical section
            enemyisAttacking = false;
        }

    }

    IEnumerator AttackEnemy() //companion attack enemy
    {
        if (Enemy.currentHealth > 0 && !companionisAttacking)
        {
            if (Enemy != null)
            { 
                //enter critical section
                companionisAttacking = true;
            
                yield return new WaitForSeconds(3f);
                EnemyDamageDetails = Enemy.TakeDamage(companion); //update enemy damagedetails information
                Debug.Log("Companion attacking, dealt " + EnemyDamageDetails.DamageTaken + " damage.");

                //exit critical section
                companionisAttacking = false;
            }
        }
    }

    IEnumerator MainAttack() //player main attack against enemy
    {
        //add logic to determine if attack is ranged or not ; different logic for ranged
        playerisAttacking = true; playerAnimator.SetBool("isAttacking", playerisAttacking); //update animator to play battle animation

        //enter critical Section ; make enemy take damage *depending on what ability* (to be added)
        if (!EnemyDamageDetails.Fainted)
        {
            EnemyDamageDetails = Enemy.TakeDamage(character); //update enemy damagedetails information //giving error and attacker passed is null
        }            

        yield return new WaitForSeconds(0.5f);
        
        //exit critical section
        playerisAttacking = false; playerAnimator.SetBool("isAttacking", playerisAttacking); //update animator to end battle animation

        //ranged logic:
    }

    IEnumerator UseClassAbility() //execute class ability based on what class the player is
    {
        Debug.Log("Using Class ability");

        //class ability decision + logic
        if (uiHandler.classAbilityReady)
        {
            //which class is using it
        }

        uiHandler.SetClassAbilityCooldownMax(character.Abilities[0].Cooldown);
        yield return new WaitForSeconds(0.5f);
    }

    IEnumerator UseHealAbility() //heal player *+ companion*
    {
        Debug.Log("Using Heal ability");

        if (uiHandler.healAbilityReady)
        {
            var amtToHeal = Mathf.FloorToInt(character.Health * 0.3f);
            if (amtToHeal + character.HP > character.Health)
                character.HP = character.Health;
            else
                character.HP += amtToHeal;
            UpdateUIStats();
            character.SetPlayerHPBar((float)character.HP / character.HP);

            uiHandler.SetHealAbilityCooldownMax(character.Abilities[1].Cooldown);
            yield return new WaitForSeconds(0.5f);
        }
    }

    public IEnumerator UseHealAbilityFreeRoam()
    {
        yield return UseHealAbility();
    }
}
