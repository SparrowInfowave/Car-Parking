using Lofelt.NiceVibrations;
using Manager;
using UnityEngine;


public class HapticSoundController : SingletonComponent<HapticSoundController>
{
    public void HapticSoundHeavy()
    {
        if(!CommonGameData.IsHapticVibrationOn)return;
        
        HapticPatterns.PlayEmphasis(1f, 1f);
    }
    public void HapticSoundMedium()
    {
        if(!CommonGameData.IsHapticVibrationOn)return;
        
        HapticPatterns.PlayEmphasis(0.5f, 1f);
    }
    
    public void HapticSoundLow()
    {
        if(!CommonGameData.IsHapticVibrationOn)return;
        
        HapticPatterns.PlayEmphasis(0.2f, 1f);
    }
}
