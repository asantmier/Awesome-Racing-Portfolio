using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using Cinemachine;
using UnityEngine.SceneManagement;

public class RaceController : MonoBehaviour
{
    public bool disableStartupSequence = false;

    public TextMeshProUGUI lapText;

    public GameObject victoryScreen;
    public TextMeshProUGUI finalPositionText;
    public GameObject pressAnyButtonToContinue;
    public Canvas startScreen;
    public Canvas pauseMenu;
    public Canvas gameScreen;
    public TextMeshProUGUI countdownText;
    public Fader fader;

    public GameObject[] racerCars;
    private bool[] racerAboutToFinishLap;
    private int[] racerLap;

    public GameObject playerCar;
    private bool playerAboutToFinishLap;
    private int playerLap;

    private int RACING = 0;
    private int START = 1;
    private int WIN = 2;
    private int PAUSE = 3;
    private int COUNTDOWN = 4;

    private int state;
    private float endWait;
    private float countdown;
    private float countdownDelay;

    private string tppXAxis;
    private string tppYAxis;

    static RaceController mInstance;
    public static RaceController Instance { get => mInstance; }

    private void Awake()
    {
        mInstance = this;
    }


    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Time.timeScale = 1;
        GameObject cinemachine = GameObject.FindGameObjectWithTag("Player Camera");
        if (cinemachine == null)
        {
            Debug.Log("Can't find third person camera in RaceController!");
        }
        tppYAxis = cinemachine.GetComponent<CinemachineFreeLook>().m_YAxis.m_InputAxisName;
        tppXAxis = cinemachine.GetComponent<CinemachineFreeLook>().m_XAxis.m_InputAxisName;
        racerAboutToFinishLap = new bool[racerCars.Length];
        racerLap = new int[racerCars.Length];
        for (int i = 0; i < racerCars.Length; i++) {
            racerAboutToFinishLap[i] = false;
            racerLap[i] = 0;
        }


        endWait = 3;
        countdown = 3;
        countdownDelay = 1;

        pauseMenu.enabled = false;
        startScreen.enabled = true;
        gameScreen.enabled = false;
        victoryScreen.SetActive(false);
        pressAnyButtonToContinue.SetActive(false);
        if (disableStartupSequence)
        {
            countdownText.enabled = false;
            GoToRacing();   
        } else
        {
            GoToStart();
        }
    }

    private void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null)
        {
            Debug.Log("No keyboard detected!");
            return;
        }

        if (keyboard.pageUpKey.wasPressedThisFrame)
        {
            Win();
        }

        lapText.text = (playerLap + 1) + "/3";

        if (state == RACING)
        {
            // Delay the countdown for a second while the GO text is on screen
            if (countdownDelay > 0)
            {
                countdownDelay -= Time.deltaTime;
                if (countdownDelay < 0)
                {
                    countdownText.enabled = false;
                }
            } else
            {
                if (keyboard.escapeKey.wasPressedThisFrame)
                {
                    GoToPause();
                }
            }
        } else if (state == WIN)
        {
            if (endWait <= 0)
            {
                pressAnyButtonToContinue.SetActive(true);

                if (keyboard.anyKey.wasPressedThisFrame)
                {
                    GoToMainMenu();
                }
            } else
            {
                endWait -= Time.deltaTime;
            }
        } else if (state == START)
        {
            if (keyboard.anyKey.wasPressedThisFrame)
            {
                state = COUNTDOWN;
                gameScreen.enabled = true;
                startScreen.enabled = false;
                countdownText.enabled = true;
            }
        } else if (state == COUNTDOWN)
        {
            countdown -= Time.deltaTime;
            countdownText.text = ((int)Mathf.Ceil(countdown)).ToString();
            if (countdown <= 0)
            {
                countdownText.text = "GO!";
                EnableControl();
                state = RACING;
            }
        } else if (state == PAUSE)
        {
            if (keyboard.escapeKey.wasPressedThisFrame)
            {
                Unpause();
            }
        }
    }

    public void FinishLap(GameObject racer)
    {
        if (racer == playerCar)
        {
            if (playerAboutToFinishLap)
            {
                playerLap += 1;
                playerAboutToFinishLap = false;
                if (playerLap >= 3)
                {
                    Win();
                }
            }
        } else
        {
            int id;
            for (id = 0; id < racerCars.Length; id++)
            {
                if (racerCars[id] == racer)
                {
                    break;
                }
            }
            if (racerAboutToFinishLap[id])
            {
                racerLap[id] += 1;
                racerAboutToFinishLap[id] = false;
                Debug.Log("Racer #" + id + " has completed " + racerLap[id] + " laps!");
                // The racers keep going regardless of how many laps they've done
            }
        }
    }

    public void AboutToFinishLap(GameObject racer)
    {
        if (racer == playerCar)
        {
            playerAboutToFinishLap = true;
        }
        else
        {
            int id;
            for (id = 0; id < racerCars.Length; id++)
            {
                if (racerCars[id] == racer)
                {
                    break;
                }
            }
            racerAboutToFinishLap[id] = true;
        }
    }

    private void Win()
    {
        // Check to make sure the game hasn't finished
        if (state == RACING)
        {
            state = WIN;
            victoryScreen.SetActive(true);
            int aiWins = 0;
            foreach (int laps in racerLap)
            {
                if (laps >= 3)
                {
                    aiWins++;
                }
            }
            switch(aiWins)
            {
                case 0:
                    finalPositionText.text = "1st";
                    break;
                case 1:
                    finalPositionText.text = "2nd";
                    break;
                case 2:
                    finalPositionText.text = "3rd";
                    break;
                case 3:
                    finalPositionText.text = "4th";
                    break;
                case 4:
                    finalPositionText.text = "5th";
                    break;
            }
            
            DisableControl();
        }
    }

    private void GoToStart()
    {
        state = START;

        pauseMenu.enabled = false;
        startScreen.enabled = true;
        gameScreen.enabled = false;

        DisableControl();
    }

    private void GoToPause()
    {
        state = PAUSE;

        startScreen.enabled = false;
        gameScreen.enabled = false;
        pauseMenu.enabled = true;

        DisableControl();
        Time.timeScale = 0;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

    }

    private void GoToRacing()
    {
        state = RACING;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        startScreen.enabled = false;
        gameScreen.enabled = true;
        pauseMenu.enabled = false;

        EnableControl();
    }

    public void DisableControl()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.Log("Cant find player from RaceController!");
        }
        player.GetComponent<PrometeoCarController>().enabled = false;
        player.GetComponent<PlayerCarControl>().enabled = false;

        GameObject cinemachine = GameObject.FindGameObjectWithTag("Player Camera");
        if (cinemachine == null)
        {
            Debug.Log("Can't find third person camera in RaceController!");
        }
        cinemachine.GetComponent<CinemachineFreeLook>().m_YAxis.m_InputAxisName = "";
        cinemachine.GetComponent<CinemachineFreeLook>().m_XAxis.m_InputAxisName = "";

        foreach (GameObject racer in racerCars)
        {
            racer.GetComponent<CarAI>().running = false;
        }
    }

    public void EnableControl()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.Log("Cant find player from RaceController!");
        }
        player.GetComponent<PrometeoCarController>().enabled = true;
        player.GetComponent<PlayerCarControl>().enabled = true;

        GameObject cinemachine = GameObject.FindGameObjectWithTag("Player Camera");
        if (cinemachine == null)
        {
            Debug.Log("Can't find third person camera in RaceController!");
        }
        cinemachine.GetComponent<CinemachineFreeLook>().m_YAxis.m_InputAxisName = tppYAxis;
        cinemachine.GetComponent<CinemachineFreeLook>().m_XAxis.m_InputAxisName = tppXAxis;

        foreach (GameObject racer in racerCars)
        {
            racer.GetComponent<CarAI>().running = true;
        }
    }

    public void Unpause()
    {
        Time.timeScale = 1;
        GoToRacing();
    }

    public void GoToMainMenu()
    {
        Debug.Log("Going to main menu");
        mInstance = null;
        SceneManager.LoadScene("Main Menu");
    }
}
