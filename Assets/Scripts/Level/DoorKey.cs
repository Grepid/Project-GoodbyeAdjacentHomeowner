using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Interactables/DoorKey")]
public class DoorKey : ScriptableObject
{
    public bool Obtained;

    public void PickedUp()
    {
        Obtained = true;
    }
}
