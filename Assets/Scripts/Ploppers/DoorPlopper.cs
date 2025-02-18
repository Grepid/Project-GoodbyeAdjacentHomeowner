using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorPlopper : MonoBehaviour
{
    public float openTime = 0.5f;
    public float susToAddWhenInteracted = 10;

    [ContextMenu("Create Door")]
    public void CreateDoor()
    {
        GameObject door = transform.GetChild(0).gameObject;
        if (door == null)
        {
            Debug.LogWarning("Trying to Plop on a door with no child object (usually the door itself)");
            return;
        }
        Transform handle = door.transform.GetChild(0);
        if (handle == null)
        {
            Debug.LogWarning("Trying to Plop on a door with no Handle");
            return;
        }
        handle.GetComponent<Collider>().enabled = false;
        MoveableObject mov = door.AddComponent<MoveableObject>();
        AutoMove auto = door.AddComponent<AutoMove>();
        Door d = door.AddComponent<Door>();
        mov.hookMode = new MoveableObject.HookMode[1];
        mov.hookMode[0] = MoveableObject.HookMode.Spin;
        mov.SpinAmount.y = -90f;

        auto.linkedObjects.Add( mov );
        auto.pullTime = openTime;

        d.moveableCall = auto;
        d.SetCanInteract(true);
        d.suspicionPercentToAdd = susToAddWhenInteracted;

        door.layer = 6;

        DestroyImmediate(this);
    }
    
}
