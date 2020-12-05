using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public GameObject pauseMenu;
    public GameObject settings;

    public bool escKey;
    public bool paused;

    private GameObject cursor;
    private bool cursorWasOn;

    // Start is called before the first frame update
    void Start()
    {
        Time.timeScale = 1;
        settings.gameObject.SetActive(false);
        pauseMenu.gameObject.SetActive(false);
        paused = false;

        cursor = GameObject.Find("AimingCursor");
        cursorWasOn = false;
    }

    // Update is called once per frame
    void Update()
    {
        escKey = Input.GetKeyDown(KeyCode.Escape);

        if (escKey)
        {
            paused = !paused;
        }

        if (paused)
        {
            OpenPause();
        }
        else
        {
            ClosePause();
        }
    }

    // Pause Menu functions
    void OpenPause()
    {
        pauseMenu.gameObject.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 0;
        paused = true;

        if (cursor.activeSelf)
        {
            cursorWasOn = true;
        }
        cursor.SetActive(false);
    }

    public void ClosePause()
    {
        pauseMenu.gameObject.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Time.timeScale = 1;
        paused = false;

        if (cursorWasOn)
        {
            cursor.SetActive(true);
            cursorWasOn = false;
        }
    }

    public void MainMenu()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene("MainMenu");
    }

    public void OpenSettings(){
        settings.gameObject.SetActive(true);
    }

    public void GoBack(){
        settings.gameObject.SetActive(false);
    }
}
