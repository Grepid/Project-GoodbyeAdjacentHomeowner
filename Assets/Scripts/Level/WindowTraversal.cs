using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindowTraversal : InteractionBase
{
    public GameObject side1, side2;
    protected override void OnInteract()
    {
        
    }
    public void Traverse()
    {
        Player.Controller.TPPlayer(oppositePoint());
    }
    private Vector3 oppositePoint()
    {
        if(Vector3.Distance(Player.Controller.transform.position , side1.transform.position) > Vector3.Distance(Player.Controller.transform.position, side2.transform.position)) return side1.transform.position;
        else return side2.transform.position;
    }
}
