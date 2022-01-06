using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnhanceUIInteraction : MonoBehaviour
{
    bool ShowCursor;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            ShowCursor = !ShowCursor;
        }
        if (ShowCursor) { showCursor(); } else { hideCursor(); }

    }

    public void hideCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void showCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
