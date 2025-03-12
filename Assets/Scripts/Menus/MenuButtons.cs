using System;
using UnityEditor;
using UnityEngine;

public class MenuButtons : MonoBehaviour
{
    //this class holds menu button functions to allow GameController to be notified through events to control game states
    public event Action OnEnterMenu;
    public event Action OnExitMenu;

    public void EnterMenu()
    {
        OnEnterMenu();
    }

    public void ExitMenu()
    {
        OnExitMenu();
    }
}
