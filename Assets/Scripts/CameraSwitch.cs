using UnityEngine;

public class CameraSwitch : MonoBehaviour
{
    public Camera topDownCamera;
    public Camera isometricCamera;
    private bool isTopDown;

    void Start()
    {
        isTopDown = true;
        ShowIsometricCamera();   
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
