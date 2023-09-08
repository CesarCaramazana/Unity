using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseLook : MonoBehaviour
{
    private float mouseX, mouseY;

    [SerializeField]
    private float sensitivityX = 10f;
    [SerializeField]
    private float sensitivityY = 10f;

    public GameObject CinemachineCameraTarget;

    [SerializeField]
    private float xClamp = 85.0f;


    private GameObject _mainCamera;
    private InputManager inputManager;


    // Start is called before the first frame update
    void Start()
    {
        inputManager = InputManager.Instance; //Create the input manager object
        Cursor.lockState = CursorLockMode.Locked;

    }

    private void Awake()
    {
        // get a reference to our main camera
        if (_mainCamera == null)
        {
            _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        }
    }

    // Update is called once per frame
    void LateUpdate()
    {
        Vector2 mouse = inputManager.GetMouseDelta();

        //Debug.Log("Mouse delta: " + mouse);

        mouseX = mouse.x * sensitivityX;// * Time.deltaTime;
        mouseY += mouse.y * sensitivityY;// * Time.deltaTime;

        mouseY = Mathf.Clamp(mouseY, -xClamp, xClamp);
        CinemachineCameraTarget.transform.localRotation = Quaternion.Euler(mouseY, 0.0f, 0.0f);

        transform.Rotate(Vector3.up * mouseX);
  
    }
}
