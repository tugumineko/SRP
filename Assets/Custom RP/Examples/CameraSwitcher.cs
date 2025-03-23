using UnityEngine;

public class CameraSwitcher : MonoBehaviour
{
    public Camera[] cameras;

    private int currentCameraIndex = 0;

    void Start()
    {
        for (int i = 0; i < cameras.Length; i++)
        {
            cameras[i].gameObject.SetActive(i == currentCameraIndex);
        }
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.V))
        {
            cameras[currentCameraIndex].gameObject.SetActive(false);
            currentCameraIndex = (currentCameraIndex + 1) % cameras.Length;
            cameras[currentCameraIndex].gameObject.SetActive(true);
        }
    }
}