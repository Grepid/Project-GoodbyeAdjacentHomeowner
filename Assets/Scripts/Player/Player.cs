using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Player
{
    public static PlayerController Controller { get; private set; }
    public static InteractionSystem InteractSys { get; private set; }
    public static PlayerData Data { get; private set; }

    #region Init
    public static void SetController(PlayerController playerController)
    {
        if (Controller != null) return; 
        Controller = playerController;
    }
    public static void SetInteractSys(InteractionSystem interactionSystem)
    {
        if (InteractSys != null) return;
        InteractSys = interactionSystem;
    }

    public static void SetData(PlayerData data)
    {
        if (Data != null) return;
        Data = data;
    }
    #endregion


    #region Player Controller

    public static void SetCursor(bool value)
    {
        Controller.SetCursor(value);
    }

    public static void SetControlling(bool value)
    {
        //
    }

    #endregion

    #region Interact System

    #endregion

    #region Data

    #endregion
}
