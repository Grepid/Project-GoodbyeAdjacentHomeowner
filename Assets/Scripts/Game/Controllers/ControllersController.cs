using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllersController : MonoBehaviour
{
    private void Awake()
    {
        foreach(Transform t in transform)
        {
            ControllerBase controller = t.GetComponent<ControllerBase>();
            if (controller == null) continue;
            controller.Initialise();
        }
    }
}
