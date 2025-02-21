using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class InteractionSystem : MonoBehaviour
{
    public static RaycastHit s_lastHit { get; private set; }

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
        s_lastHit = hit;
    }

}
