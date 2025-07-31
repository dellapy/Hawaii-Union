using UnityEngine;

public class CameraSwitch : MonoBehaviour
{
    public Camera topDownCamera;
    public Camera isometricCamera;
    private bool isTopDown = false;

    void Start()
    {
        ShowIsometricCamera();   
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            SwitchPerspective();
        }
    }

    public void SwitchPerspective()
    {
        if (!isTopDown)
        {
            ShowTopDownCamera();
            isTopDown = true;
        }
        else
        {
            ShowIsometricCamera();
            isTopDown = false;
        }
    }

    private void ShowTopDownCamera()
    {
        topDownCamera.enabled = true;
        isometricCamera.enabled = false;
    }

    private void ShowIsometricCamera()
    {
        topDownCamera.enabled = false;
        isometricCamera.enabled = true;
    }
}
