using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using TMPro;

public class SettingsMenu : MonoBehaviour
{
    Resolution[] resolutions;
    public TMP_Dropdown resDropdown;
    void Start(){
        resolutions = Screen.resolutions;
        resDropdown.ClearOptions();
        List<string> options = new List<string>();
        int curr = 0;
        for(int i = 0; i < resolutions.Length; i++){
            string res = resolutions[i].width + " x " + resolutions[i].height;
            options.Add(res);
            if(resolutions[i].width == Screen.currentResolution.width && resolutions[i].height == Screen.currentResolution.height){
                curr = i;
            }
        }
        resDropdown.AddOptions(options);
        resDropdown.value = curr;
        resDropdown.RefreshShownValue();
    }

    public void SetRes(int resIndex){
        Resolution resolution = resolutions[resIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
    }
    public AudioMixer myMixer;
    public void SetVolume(float vol){
        myMixer.SetFloat("vol", vol);
    }

    public void SetResolution(){

    }

    public void setFullscreen(bool check){
        Screen.fullScreen = check;
    }
}
