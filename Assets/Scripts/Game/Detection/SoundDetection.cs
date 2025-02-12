using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundDetection : MonoBehaviour
{
    [SerializeField]
    private RectTransform soundBar, permanentSoundBar;

    public float SoundLevel { get; private set; }
    public float PermanentSoundLevel { get; private set; }

    public float AmbienceLevel { get; private set; }
    private float ambienceMultiplier = 1;
    public float TotalSoundLevel => SoundLevel + PermanentSoundLevel;

    private float startingWidth;

    private void Awake()
    {
        startingWidth = soundBar.rect.width;
        soundBar.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, TotalSoundLevel);
        permanentSoundBar.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, PermanentSoundLevel);
    }

    private void Update()
    {
        
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

    private void AddSoundLevel(float level)
    {
        SoundLevel += level * ambienceMultiplier;
        UpdateSoundBar();
    }

    private void UpdateSoundBar()
    {
        if (soundBar == null) return;
        soundBar.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Mathf.Lerp(0f, startingWidth, TotalSoundLevel));
        permanentSoundBar.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Mathf.Lerp(0f, startingWidth, PermanentSoundLevel));

    }
}
