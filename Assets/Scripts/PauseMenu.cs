using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour
{
    private GameObject canvas;

    private bool paused = false;

    // Start is called before the first frame update
    void Start()
    {
        canvas = GetComponentInChildren<Canvas>(true).gameObject;
        canvas.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        Gamepad g = Gamepad.current;
        if (Keyboard.current.escapeKey.wasPressedThisFrame || (g != null && (
            g.selectButton.wasPressedThisFrame)))
        {
            paused = !paused;
            canvas.SetActive(paused);
            if (paused)
            {
                Time.timeScale = 0;
                Cursor.lockState = CursorLockMode.None;
            }
            else
            {
                Time.timeScale = 1;
                Cursor.lockState = CursorLockMode.Locked;
            }
        }
    }

    public void AdjustVolume(float f)
    {
        AudioListener.volume = f;
    }

    public void Quit()
    {
        Application.Quit();
    }
}
