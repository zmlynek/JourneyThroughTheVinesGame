using UnityEngine;

//This class is what is referenced to do calculations for the correct weapon ??
public class Weapon 
{
    [SerializeField] WeaponBase wBase;

    public WeaponBase WeaponBase
    {
        get { return wBase; }
    }
}
