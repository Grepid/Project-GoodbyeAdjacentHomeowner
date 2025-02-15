using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundDetection : MonoBehaviour
{
    public static SoundDetection instance;


    [SerializeField]
    private RectTransform temporarySuspicionBar, permanentSuspicionBar;

    public float TemporarySuspicion { get; private set; }
    public float PermanentSuspicion { get; private set; }

    public float AmbienceLevel { get; private set; }
    private float ambienceMultiplier = 1;
    public float TotalSuspicion => TemporarySuspicion + PermanentSuspicion;
    public float TemporarySuspicionLostPerSecond;
    [SerializeField]
    private float detectionPerecent,permanentPercentPerSecondInZone;

    public bool IsTaggedAndCursed => PermanentSuspicion >= (detectionPerecent / 100);
    public bool IsDetected => TotalSuspicion >= (detectionPerecent/100);

    private float startingWidth;

    private void Awake()
    {
        instance = this;    
        startingWidth = temporarySuspicionBar.rect.width;
        temporarySuspicionBar.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, TotalSuspicion);
        permanentSuspicionBar.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, PermanentSuspicion);
    }

    private void Update()
    {
        TemporarySuspicion = Mathf.Clamp(TemporarySuspicion, 0, 1 - PermanentSuspicion);
        if (TotalSuspicion > (detectionPerecent / 100) && TemporarySuspicion > 0)
        {
            AddPermanentSuspicionPercent(permanentPercentPerSecondInZone * Time.deltaTime);
            //AddSoundLevelPercent(-permanentPercentPerSecondInZone * Time.deltaTime);
        }
        AddTemporarySuspicionPercent(-TemporarySuspicionLostPerSecond * Time.deltaTime);

        //Debug binds
        if (Input.GetKeyDown(KeyCode.Z))
        {
            AddTemporarySuspicionPercent(10);
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            AddTemporarySuspicionPercent(-10);
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            AddPermanentSuspicionPercent(10);
        }
        if (Input.GetKeyDown(KeyCode.Q))
        {
            AddPermanentSuspicionPercent(-10);
        }


        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SetAmbienceLevel(1);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SetAmbienceLevel(2);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            SetAmbienceLevel(3);
        }
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            SetAmbienceLevel(0);
        }
    }
    bool needsUpdate;
    private void LateUpdate()
    {
        if(needsUpdate)UpdateSuspicionBar();
        needsUpdate = false;
    }

    public void SetAmbienceLevel(int level)
    {
        switch (level)
        {
            case 0:
                ambienceMultiplier = 1;
                break;

            case 1:
                ambienceMultiplier = 0.9f;
                break;

            case 2:
                ambienceMultiplier = 0.75f;
                break;

            case 3:
                ambienceMultiplier = 0.5f;
                break;
        }
    }

    public void AddTemporarySuspicionPercent(float level)
    {
        TemporarySuspicion = Mathf.Clamp(TemporarySuspicion + ((level / 100) * ambienceMultiplier), 0, 1-PermanentSuspicion);
        //UpdateSoundBar();
        needsUpdate = true;
    }

    public void AddPermanentSuspicionPercent(float level)
    {
        PermanentSuspicion = Mathf.Clamp(PermanentSuspicion + (level / 100), 0, 1);
        AddTemporarySuspicionPercent(-level);
        //UpdateSoundBar();
        needsUpdate = true;
    }

    private void UpdateSuspicionBar()
    {
        if (temporarySuspicionBar == null) return;
        temporarySuspicionBar.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Mathf.Lerp(0f, startingWidth, TotalSuspicion));
        permanentSuspicionBar.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Mathf.Lerp(0f, startingWidth, PermanentSuspicion));

    }
}
