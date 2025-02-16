using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class SoundDetection : MonoBehaviour
{
    public static SoundDetection instance;


    [SerializeField]
    private RectTransform temporarySuspicionBar, permanentSuspicionBar;

    public float TemporarySuspicion { get; private set; }
    public float PermanentSuspicion { get; private set; }

    private List<EnvironmentalMask> activeMasks = new List<EnvironmentalMask>();
    public List<EnvironmentalMask> OrderedMasks
    {
        get
        {
            if(activeMasks.Count == 0) return new List<EnvironmentalMask>();    
            return activeMasks.OrderByDescending(m => m.maskLevel).ToList();
        }
    }
    public List<EnvironmentalMask> ActiveMasks => activeMasks;
    public void AddMask(EnvironmentalMask mask)
    {
        activeMasks.Add(mask);
        UpdateMask();
    }
    public void RemoveMask(EnvironmentalMask mask)
    {
        if (!activeMasks.Contains(mask)) return;
        activeMasks.Remove(mask);
        UpdateMask();
    }
    private void UpdateMask()
    {
        
        if(activeMasks.Count == 0)
        {
            SetAmbienceLevel(0);
            return;
        }
        int highest = OrderedMasks[0].maskLevel;
        SetAmbienceLevel(highest);
        
    }
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
        if (Keyboard.current.zKey.wasPressedThisFrame)
        {
            AddTemporarySuspicionPercent(10);
        }
        if (Keyboard.current.cKey.wasPressedThisFrame)
        {
            AddTemporarySuspicionPercent(-10);
        }
        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            AddPermanentSuspicionPercent(10);
        }
        if (Keyboard.current.qKey.wasPressedThisFrame)
        {
            AddPermanentSuspicionPercent(-10);
        }


        if (Keyboard.current.digit1Key.wasPressedThisFrame)
        {
            SetAmbienceLevel(1);
        }
        if (Keyboard.current.digit2Key.wasPressedThisFrame)
        {
            SetAmbienceLevel(2);
        }
        if (Keyboard.current.digit3Key.wasPressedThisFrame)
        {
            SetAmbienceLevel(3);
        }
        if (Keyboard.current.digit0Key.wasPressedThisFrame)
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
        AmbienceLevel = level;
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
