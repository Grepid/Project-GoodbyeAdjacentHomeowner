using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct InteractionQuery
{
    public RaycastHit Hit;
    public InteractionBase Interactable;
    public bool DidHit;
    public float Distance;

    //Initialisers
    public InteractionQuery(RaycastHit hit, InteractionBase interactable,bool didHit, float distance)
    {
        Hit = hit;
        Interactable = interactable;
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
        InteractionBase interactable = null;
        
        bool didHit = Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit, InteractionRange, castableLayers, QueryTriggerInteraction.Ignore);
        if(didHit) interactable = hit.collider.GetComponent<InteractionBase>();

        InteractionQuery query = new InteractionQuery(hit, interactable,didHit ,hit.distance);
        s_lastHit = query;
    }

}
