using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundDetection : MonoBehaviour
{
    public static SoundDetection instance;


    [SerializeField]
    private RectTransform soundBar, permanentSoundBar;

    public float SoundLevel { get; private set; }
    public float PermanentSoundLevel { get; private set; }

    public float AmbienceLevel { get; private set; }
    private float ambienceMultiplier = 1;
    public float TotalSoundLevel => SoundLevel + PermanentSoundLevel;
    public float unSusPercentPerSecond;
    [SerializeField]
    private float permanentAddThresholdPercent,permanentPercentPerSecondInZone;

    public bool IsTaggedAndCursed => PermanentSoundLevel >= (permanentAddThresholdPercent / 100);

    private float startingWidth;

    private void Awake()
    {
        instance = this;    
        startingWidth = soundBar.rect.width;
        soundBar.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, TotalSoundLevel);
        permanentSoundBar.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, PermanentSoundLevel);
    }

    private void Update()
    {
        SoundLevel = Mathf.Clamp(SoundLevel, 0, 1 - PermanentSoundLevel);
        if (TotalSoundLevel > (permanentAddThresholdPercent / 100) && SoundLevel > 0)
        {
            AddPermanentSoundLevelPercent(permanentPercentPerSecondInZone * Time.deltaTime);
            AddSoundLevelPercent(-permanentPercentPerSecondInZone * Time.deltaTime);
        }
        AddSoundLevelPercent(-unSusPercentPerSecond * Time.deltaTime);

        //Debug
        if (Input.GetKeyDown(KeyCode.X))
        {
            AddSoundLevelPercent(10);
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            AddPermanentSoundLevelPercent(10);
        }
        if (Input.GetKeyDown(KeyCode.Q))
        {
            AddPermanentSoundLevelPercent(-10);
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

    public void AddSoundLevelPercent(float level)
    {
        SoundLevel = Mathf.Clamp(SoundLevel + ((level / 100) * ambienceMultiplier), 0, 1-PermanentSoundLevel);
        UpdateSoundBar();
    }

    private void AddPermanentSoundLevelPercent(float level)
    {
        PermanentSoundLevel = Mathf.Clamp(PermanentSoundLevel + (level / 100), 0, 1);
        UpdateSoundBar();
    }

    private void UpdateSoundBar()
    {
        if (soundBar == null) return;
        soundBar.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Mathf.Lerp(0f, startingWidth, TotalSoundLevel));
        permanentSoundBar.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Mathf.Lerp(0f, startingWidth, PermanentSoundLevel));

    }
}
