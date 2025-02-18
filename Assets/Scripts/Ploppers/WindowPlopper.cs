using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindowPlopper : MonoBehaviour
{
    public float openTime = 0.5f;
    public float susToAddWhenInteracted = 10;
    public float openAmount = 0.4f;
    [ContextMenu("Create Window")]
    public void CreateWindow()
    {
        GameObject parent = transform.parent.gameObject;
        MoveableObject mov = gameObject.AddComponent<MoveableObject>();
        AutoMove auto = gameObject.AddComponent<AutoMove>();
        BoxCollider testCol1 = gameObject.AddComponent<BoxCollider>();
        Door d = gameObject.AddComponent<Door>();
        GameObject traversalZone = new GameObject("TraversalZone");
        traversalZone.transform.parent = parent.transform;
        traversalZone.transform.localPosition = transform.localPosition;
        traversalZone.transform.rotation = transform.rotation;
        BoxCollider travCol = traversalZone.AddComponent<BoxCollider>();
        MeshCollider mc = GetComponent<MeshCollider>();
        mc.enabled = false;
        //travCol = testCol1;
        travCol.size = testCol1.size;
        
        traversalZone.transform.localPosition += (Vector3.up * (travCol.size.y / 2));
        traversalZone.transform.localPosition += Vector3.forward * -0.01f;
        WindowTraversal win = traversalZone.AddComponent<WindowTraversal>();
        GameObject point1 = new GameObject("traversalPoint1");
        GameObject point2 = new GameObject("traversalPoint2");
        point1.transform.parent = traversalZone.transform;
        point2.transform.parent = traversalZone.transform;
        point1.transform.localPosition = Vector3.forward * 0.5f;
        point2.transform.localPosition = Vector3.forward * -0.5f;
        Vector3 size = travCol.size;
        size.z = 0.01f;
        travCol.size = size;

        win.side1 = point1;
        win.side2 = point2;
        win.SetCanInteract(true);

        mov.hookMode = new MoveableObject.HookMode[1];
        mov.hookMode[0] = MoveableObject.HookMode.Move;
        mov.intendedMoveLocation = transform.localPosition + (Vector3.up * openAmount);

        auto.linkedObjects.Add(mov);
        auto.pullTime = openTime;

        d.moveableCall = auto;
        d.SetCanInteract(true);
        d.suspicionPercentToAdd = susToAddWhenInteracted;



        DestroyImmediate(this);
    }
}
