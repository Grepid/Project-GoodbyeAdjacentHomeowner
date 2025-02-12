using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct InteractionQuery
{
    public RaycastHit Hit;
    public bool DidHit;
    public float Distance;

    //Initialisers
    public InteractionQuery(RaycastHit hit, bool didHit, float distance)
    {
        Hit = hit;
        DidHit = didHit;
        Distance = distance;
    }
}

public class InteractionSystem : MonoBehaviour
{
    public static InteractionQuery s_lastHit { get; private set; }

    [SerializeField]
    private LayerMask castableLayers;

    [SerializeField]
    private Camera playerCamera;

    public float InteractionRange;

    private void Awake()
    {
        Player.SetInteractSys(this);
    }

    private void Update()
    {
        AssignLastHit();
    }
    private void AssignLastHit()
    {
        RaycastHit hit = new RaycastHit();
        
        bool didHit = Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit, InteractionRange, castableLayers, QueryTriggerInteraction.Ignore);

        InteractionQuery query = new InteractionQuery(hit, didHit ,hit.distance);
    }

}
