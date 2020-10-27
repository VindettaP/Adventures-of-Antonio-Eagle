using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuHandler : MonoBehaviour
{
    // buttons
    public Button newGame;
    public Button loadGame;
    public Button options;
    public Button instructions;
    public Button credits;
    public Button exit;

    // pop ups
    public Button creditsPopup;
    public Button instructionsPopup;

    // Start is called before the first frame update
    void Start()
    {
        creditsPopup.gameObject.SetActive(false);
        instructionsPopup.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Disable all menu buttons (useful when displaying messages)
    void DisableButtons()
    {
        newGame.interactable = false;
        loadGame.interactable = false;
        options.interactable = false;
        instructions.interactable = false;
        credits.interactable = false;
        exit.interactable = false;
    }
    // Enables all buttons
    void EnableButtons()
    {
        newGame.interactable = true;
        loadGame.interactable = true;
        options.interactable = true;
        instructions.interactable = true;
        credits.interactable = true;
        exit.interactable = true;
    }

    // Functions for handling menu loading
    public void NewGame()
    {
        SceneManager.LoadScene("GameWorld");
    }

    public void ContinueGame()
    {
        SceneManager.LoadScene("ContinueMenu");
    }

    public void Options()
    {
        SceneManager.LoadScene("OptionsMenu");
    }

    public void ExitGame()
    {
        Application.Quit();
        Debug.Log("Exiting Game in .exe");
    }


    // Functions for handling UI popups
    public void CreditsButton()
    {
        creditsPopup.gameObject.SetActive(true);
        DisableButtons();
    }

    public void CreditsPopup()
    {
        creditsPopup.gameObject.SetActive(false);
        EnableButtons();
    }

    public void InstructionsButton()
    {
        instructionsPopup.gameObject.SetActive(true);
        DisableButtons();
    }

    public void InstructionsPopup()
    {
        instructionsPopup.gameObject.SetActive(false);
        EnableButtons();
    }
}
