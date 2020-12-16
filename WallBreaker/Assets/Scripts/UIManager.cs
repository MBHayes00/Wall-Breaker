using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    //Variables
    [SerializeField]
    private GameObject mainMenu;
    [SerializeField]
    private GameObject gameMenu;
    [SerializeField]
    private GameObject gameOverMenu;
    [SerializeField]
    private Text finalScore;
    
    public bool gameOver;

    [HideInInspector]
    public int score;

    // Start is called before the first frame update
    void Start()
    {
        GameObject[] managers = GameObject.FindGameObjectsWithTag("GameController");
        if(managers.Length > 1)
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);

        if(!gameOver)
            ToMainMenu();
        else
        {
            GameOver();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlayGame()
    {
        SceneManager.LoadScene(1);
        gameMenu.SetActive(true);
    }

    public void GameOver()
    {       
        SceneManager.LoadScene(0);
        mainMenu.SetActive(false);
        gameOverMenu.SetActive(true);
    }

    public void ToMainMenu()
    {
        mainMenu.SetActive(true);
        gameOverMenu.SetActive(false);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

}
