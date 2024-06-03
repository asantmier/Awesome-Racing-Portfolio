using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void EasyTrack()
    {
        SceneManager.LoadScene("Racing");
    }

    public void HardTrack()
    {
        SceneManager.LoadScene("Racing 2");
    }
}
