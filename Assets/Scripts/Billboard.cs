using UnityEngine;

public class Billboard : MonoBehaviour
{
    public enum BillboardType
    {
        LookAtCamera,
        CameraForward,
        Euler
    }

    public BillboardType billboardType;

    void LateUpdate()
    {
        switch (billboardType)
        {
            case BillboardType.LookAtCamera:
                transform.LookAt(Camera.main.transform.position, Vector3.up);
                break;
            case BillboardType.CameraForward:
                transform.forward = Camera.main.transform.forward;
                break;
            case BillboardType.Euler:
                transform.rotation = Quaternion.Euler(Camera.main.transform.rotation.eulerAngles.x, 0f, 0f);
                break;
            default:
                break;
        }
    }
}
