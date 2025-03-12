using System;
using System.Collections;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.TextCore.Text;

//This class  controls the companioons movement and states including dialogue and battle
public class CompanionController : MonoBehaviour
{
    [SerializeField]PlayerController playerController;
    [SerializeField] HealthBar healthBar;
    public Transform movePoint;
    public Transform startingPos;
    public float moveSpeed = 5f;
    public LayerMask player;
    public LayerMask collision;
    public LayerMask enemies;

    public int MaxHealth { get; set; }
    public int Defense { get; set; }
    public int Attack { get; set; }
    public float CritChance { get; set; }
    public float Stregnth { get; set; }
    public int HP { get; set; }

    public void Start()
    {
        movePoint.parent = null;
        MaxHealth = 200;
        HP = MaxHealth;
        Attack = 100;
        Defense = 150;
        Stregnth = 0.3f;
    }

    private bool isWalkable(Vector3 targetPos)
    {
        if (Physics2D.OverlapCircle(targetPos, 0.1f, player | collision | enemies ) != null)
        {
            return false;
        }
        return true;
    }

    Vector3 FindEnemyPos(Enemy enemy)
    {
        Transform enemyPos = enemy.transform;
        Vector3 targetTileR = new Vector3(enemyPos.position.x + 1, enemyPos.position.y);
        Vector3 targetTileL = new Vector3(enemyPos.position.x - 1, enemyPos.position.y);
        Vector3 targetTileU = new Vector3(enemyPos.position.x, enemyPos.position.y + 1);
        Vector3 targetTileD = new Vector3(enemyPos.position.x, enemyPos.position.y - 1);
        if (isWalkable(targetTileR)) 
            return targetTileR;
        else if (isWalkable(targetTileL)) 
            return targetTileL;
        else if (isWalkable(targetTileU))
            return targetTileU;
        else if (isWalkable(targetTileD))
            return targetTileD;
        else 
            return transform.position; 
    }
    public IEnumerator WalkToPos(Vector3 targetPos) //currently does not work for some reason
    {
        float originalX = transform.position.x;
        float originalY = transform.position.y;
        float xTiles = targetPos.x - originalX;
        float yTiles = targetPos.y - originalY;

        //movePoint + 1 tile along path to targetPos
        //for x-axis
        for (int i = 0; i < xTiles; i++)
        {
            var nextTile = new Vector3(movePoint.position.x + (originalX / Mathf.Abs(originalX)), originalY, transform.position.z);
            if (isWalkable(nextTile))
            {
                movePoint.position = nextTile;
                yield return new WaitForSeconds(0.2f);
            }
        }
        //for y-axis
        for (int i = 0; i < yTiles; i++)
        {
            var nextTile = new Vector3(originalX, movePoint.position.y + (originalY / Mathf.Abs(originalY)), transform.position.z);
            if (isWalkable(nextTile))
            {
                movePoint.position = new Vector3(originalX, movePoint.position.y + (originalY / Mathf.Abs(originalY)), transform.position.z);
                yield return new WaitForSeconds(0.2f);
            }
        }
    }

    public DamageDetails TakeDamage(Enemy attacker)
    {

        bool isDead = false;
        float critical = 1f;
        if (UnityEngine.Random.value <= attacker.CritChance)
        {
            critical = 1.5f;
        }

        //damage taken formula : dmg = (abilityPower(%) * attackerATK) * (attackerATK / defenderDEF)
        int damageTaken = Mathf.FloorToInt( (float)attacker.AbilityBase.Power * attacker.Attack * ( (float)attacker.Attack / Defense) );
        damageTaken = Mathf.FloorToInt(damageTaken * critical);
        HP -= damageTaken;
        if (HP <= 0)
        {
            HP = 0;
            isDead = true;
        }

         var damageDetails = new DamageDetails()
        {
            Fainted = isDead,
            Critical = critical,
            DamageTaken = damageTaken,
        };

        float HPnormalized;
        if (HP > 0)
        {
            HPnormalized = (float)HP / MaxHealth;
            StartCoroutine(healthBar.SetHPSmooth(HPnormalized));
        }
        else
            healthBar.SetHP(0);

        return damageDetails; //return damge info
    }

    public IEnumerator BattleMode()
    {
        //Move to side of player instead of behind
        yield return new WaitForSeconds(1f);
        //Execute attacks every 3 seconds
        yield return new WaitForSeconds(3f);
    }

    public IEnumerator OnStartGame()
    {
        StartCoroutine(WalkToPos(startingPos.position));
        yield return new WaitForSeconds(2f);
    }

    public void HandleUpdateFreeRoam()
    {
        transform.position = Vector3.MoveTowards(transform.position, movePoint.position, moveSpeed * Time.deltaTime);
        //logic should be follow player if can move AND no dialogue
        if (Vector3.Distance(transform.position, movePoint.position) <= .05f) 
        {
            if (isWalkable(playerController.LastTileVisited)) 
                movePoint.position = playerController.LastTileVisited;
        }
    }

    public void HandleUpdateBattle(Enemy enemy) //fix follow player on right ; maybe change to move to 1 tile range from enemy position
    {
        transform.position = Vector3.MoveTowards(transform.position, movePoint.position, moveSpeed * Time.deltaTime);
        //logic should be follow player if can move AND no dialogue
        if (movePoint.position != FindEnemyPos(enemy))
            movePoint.position = FindEnemyPos(enemy);
    }
}
