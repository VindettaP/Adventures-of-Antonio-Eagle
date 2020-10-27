using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public GameObject pauseMenu;

    public bool escKey;
    public bool paused;

    // Start is called before the first frame update
    void Start()
    {
        pauseMenu.gameObject.SetActive(false);
        paused = false;
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
        paused = true;
    }

    public void ClosePause()
    {
        pauseMenu.gameObject.SetActive(false);
        paused = false;
    }

    public void MainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
