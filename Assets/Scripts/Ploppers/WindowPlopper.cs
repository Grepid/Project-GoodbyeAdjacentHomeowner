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
        GameObject parent = transform.gameObject;
        List<GameObject> windows = new List<GameObject>();  
        foreach(Transform t in transform)
        {
            if (t.name.Contains("Window")) windows.Add(t.gameObject);
        }

        foreach(GameObject w in windows)
        {
            MoveableObject mov = w.AddComponent<MoveableObject>();
            AutoMove auto = w.AddComponent<AutoMove>();
            BoxCollider testCol1 = w.AddComponent<BoxCollider>();
            Door d = w.AddComponent<Door>();
            GameObject traversalZone = new GameObject("TraversalZone");
            traversalZone.transform.parent = parent.transform;
            traversalZone.transform.localPosition = w.transform.localPosition;
            traversalZone.transform.rotation = w.transform.rotation;
            BoxCollider travCol = traversalZone.AddComponent<BoxCollider>();
            MeshCollider mc = GetComponent<MeshCollider>();
            mc.enabled = false;
            
            travCol.size = testCol1.size;

            traversalZone.transform.localPosition += (Vector3.up * (travCol.size.y / 2));
            traversalZone.transform.localPosition += Vector3.forward * -0.01f;
            WindowTraversal win = traversalZone.AddComponent<WindowTraversal>();
            GameObject point1 = new GameObject("traversalPoint1");
            GameObject point2 = new GameObject("traversalPoint2");
            point1.transform.parent = traversalZone.transform;
            point2.transform.parent = traversalZone.transform;
            point1.transform.localPosition = Vector3.forward * 0.5f;
            point1.transform.localPosition += Vector3.down * 0.5f;
            point2.transform.localPosition = Vector3.forward * -0.5f;
            point2.transform.localPosition += Vector3.down * 0.5f;
            Vector3 size = travCol.size;
            size.z = 0.01f;
            travCol.size = size;

            win.side1 = point1;
            win.side2 = point2;
            //win.SetCanInteract(true);

            mov.hookMode = new MoveableObject.HookMode[1];
            mov.hookMode[0] = MoveableObject.HookMode.Move;
            mov.intendedMoveLocation = w.transform.localPosition + (Vector3.up * openAmount);

            auto.linkedObjects.Add(mov);
            auto.pullTime = openTime;

            d.moveableCall = auto;
            d.SetCanInteract(true);
            d.suspicionPercentToAdd = susToAddWhenInteracted;
        }

        DestroyImmediate(this);
    }
}
