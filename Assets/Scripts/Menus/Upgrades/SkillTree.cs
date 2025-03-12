using System.Collections.Generic;
using UnityEngine;

public class SkillTree : MonoBehaviour 
{
    //will contain the skill tree as a dictionary, and methods to be used on buttons in the skill tree
    public Dictionary<string, int> skillTree { get; private set; }
    
    //need reference to all level images
    public int StaffMasteryLevel { get; private set; } //+20 dmg ; MAX LVL: 5
    public int SwordMasteryLevel { get; private set; } //+15 dmg ; MAX LVL: 5
    public int DaggerMasteryLevel { get; private set; } //+20 dmg ; MAX LVL: 5
    public int BowMasteryLevel { get; private set; } //+15 dmg ; MAX LVL: 5

    public int HealBuffLevel { get; private set; } //+5% healing ; MAX LVL: 6
    public int CompanionPowerLevel { get; private set; } //27.5% compounding increase to ATK ; MAX LVL: 10

    public int AttackBonusLevel { get; private set; } //+1% ATK ; MAX LVL: 10
    public int DefenseBonusLevel { get; private set; } //+2% DEF ; MAX LVL: 10

    public int WeaponDropChanceLevel { get; private set; } //+1% drop chance ; MAX LVL: 10
    public int WeaponRarityQualityLevel { get; private set; } //+1% better drop rate ; MAX LVL: 5
    public int HeroOfTheForestLevel { get ; private set; } //+25 ATK and +25 DEF ; MAX LVL: 5

    public int OverdrivePowerLevel { get; private set; } //+10% for effects ; MAX LVL: 3
    public int OverdriveCooldownLevel { get; private set; } //-10s cooldown ; MAX LVL: 3
    public int EnragePowerLevel { get; private set; } //+15% for effects ; MAX LVL: 3
    public int EnrageCooldownLevel { get; private set; } //-10s cooldown ; MAX LVL: 3
    public int SneakDurationLevel { get; private set; } //+1s sneak time ; MAX LVL: 3
    public int SneakCooldownLevel { get; private set; } //-8s cooldown ; MAX LVL: 3
    public int ArrowBarrageArrowsLevel {  get; private set; } //+1 arrow in arrow barrage ; MAX LVL: 3
    public int ArrowBarrageCooldownLevel { get; private set; }//-10s cooldown ; MAX LVL: 3

    public void Start()
    {
        //initialize everything to 1 and initialize dictionary
        skillTree = new Dictionary<string, int>();
        StaffMasteryLevel = 1; skillTree.Add("Staff Mastery", StaffMasteryLevel);
        SwordMasteryLevel = 1; skillTree.Add("Sword Mastery", SwordMasteryLevel);
        DaggerMasteryLevel = 1; skillTree.Add("Dagger Mastery", DaggerMasteryLevel);
        BowMasteryLevel = 1; skillTree.Add("Bow Mastery", BowMasteryLevel);

        HealBuffLevel = 1; skillTree.Add("Heal Buff", HealBuffLevel);
        CompanionPowerLevel = 1; skillTree.Add("Companion Power", CompanionPowerLevel);

        AttackBonusLevel = 1; skillTree.Add("Attack Bonus", AttackBonusLevel);
        DefenseBonusLevel = 1; skillTree.Add("Defense Bonus", DefenseBonusLevel);

        WeaponDropChanceLevel = 1; skillTree.Add("Weapon Drop Chance", WeaponDropChanceLevel);
        WeaponRarityQualityLevel = 1; skillTree.Add("Weapon Rarity Quality", WeaponRarityQualityLevel);
        HeroOfTheForestLevel = 1; skillTree.Add("Hero of the Forest", HeroOfTheForestLevel);

        OverdrivePowerLevel = 1; skillTree.Add("Overdrive Power", OverdrivePowerLevel);
        OverdriveCooldownLevel = 1; skillTree.Add("Overdrive Cooldown", OverdriveCooldownLevel);
        EnragePowerLevel = 1; skillTree.Add("Enrage Power", EnragePowerLevel);
        EnrageCooldownLevel = 1; skillTree.Add("Enrage Cooldown", EnrageCooldownLevel);
        SneakDurationLevel = 1; skillTree.Add("Sneak Duration", SneakDurationLevel); 
        SneakCooldownLevel = 1; skillTree.Add("Sneak Cooldown", SneakCooldownLevel);
        ArrowBarrageArrowsLevel = 1; skillTree.Add("Arrow Barrage Extra Arrows", ArrowBarrageArrowsLevel);
        ArrowBarrageCooldownLevel = 1; skillTree.Add("Arrow Barrage Cooldown", ArrowBarrageCooldownLevel);

        Debug.Log(skillTree["Staff Mastery"]);
    }

}
