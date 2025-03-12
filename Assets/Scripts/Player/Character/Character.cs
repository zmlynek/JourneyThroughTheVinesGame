using System.Collections.Generic;
using UnityEngine;

//This class is the implementation for character base 
public class Character : MonoBehaviour
{
    public string classType;
    [SerializeField] WeaponBase weapon;
    [SerializeField] HealthBar healthBar;
    public int HP { get; set; }

    public WeaponBase Weapon
    {
        get { return weapon; }
    }

    public int level;
    public int Level { get { return level; } }

    [SerializeField] CharacterBase cBase;
    public CharacterBase CharacterBase { get { return cBase; } }

    [SerializeField] List<AbilityBase> abilities = new List<AbilityBase>();
    public List<AbilityBase> Abilities { get { return abilities; } }

    //Level scaling formula = stat + (stat * x% * level)
    //current formulas are across all classes using the base to determine class' base stats
    //across all classes: aiming for roughly 8k health max, 8k max defense, 6k max attack around lvl 50
    //with stats reflecting the class, i.e swordsman has high health so its closer to 8k by max level
    public int Health
    {
        get { return Mathf.FloorToInt(cBase.Health + (cBase.Health * 0.56f * (level-1) )); } //56% increase per level
    }
    public int Defense
    {
        get { return Mathf.FloorToInt(cBase.Defense + (cBase.Defense * 0.48f * (level-1) )); } //48% increase per level
    }
    public int Attack
    {
        get { return Mathf.FloorToInt(cBase.Attack + (cBase.Attack * 0.26f * (level - 1) )); ; } //26% increase per level
    }

    //Start Method to set class type accordingly after character selection? 
    public void Start()
    {
        HP = Health;
    }

    public void SetPlayerHPBar(float hpNormalized)
    {
        StartCoroutine(healthBar.SetHPSmooth(hpNormalized));
    }

    public DamageDetails TakeDamage(Enemy attacker)
    {
        bool isDead = false;
        float critical = 1f;
        if (UnityEngine.Random.value <= attacker.CritChance)
        {
            critical = 2f;
        }

        //damage taken formula : dmg = (abilityPower(%) * attackerATK) * (attackerATK / defenderDEF)
        int damageTaken = Mathf.FloorToInt((attacker.AbilityBase.Power * attacker.Attack) * ((float)attacker.Attack / Defense));
        damageTaken = Mathf.FloorToInt(damageTaken * critical);
        HP -= damageTaken;
        if (HP <= 0)
        {
            HP = 0;
            isDead = true;
        }

        var damageDetails = new DamageDetails() //character damage details
        {
            Fainted = isDead,
            Critical = critical,
            DamageTaken = damageTaken,
        };

        float HPnormalized;
        if (HP > 0) 
        {
            HPnormalized = (float)HP / Health;
            StartCoroutine(healthBar.SetHPSmooth(HPnormalized));        }
        else
            healthBar.SetHP(0);

        return damageDetails; //return damge info
    }
}
