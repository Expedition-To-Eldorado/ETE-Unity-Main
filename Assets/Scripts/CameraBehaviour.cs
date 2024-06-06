using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GeneralEnumerations;
using System;
using System.Collections.Specialized;

public class CameraBehaviour : MonoBehaviour
{
    [SerializeField] GameObject mainCamera;
    public static Action<ViewTypes> changeView;
    [SerializeField] int viewType = (int)ViewTypes.BoardCards; //default setting
    int lastViewType;
    Vector3 lastPosition;
    [SerializeField] float cameraHeight = 50f;
    [SerializeField] float shopCameraHeight = 26f;
    [SerializeField] float cameraSpeed = 5f;
    [SerializeField] float mouseCameraSpeed = 10f;
    [SerializeField] Vector3[] camPositions = new Vector3[4];
    [SerializeField] Vector3[] camAngles = new Vector3[4];
    public float downCameraBoundY = 100f;
    public float upCameraBoundY = 100f;
    public float leftCameraBoundX = 250f;
    public float rightCameraBoundX = 250f;
    public float turningRate = 100f;
    private float mouseBorder = 10f;

    //I have no idea what this is but is required to rotate camera
    private Quaternion _targetRotation = Quaternion.identity;

    // Start is called before the first frame update
    void Start()
    {
        camPositions[(int)ViewTypes.CardsOnly].x = 0;
        camPositions[(int)ViewTypes.CardsOnly].y = cameraHeight;
        camPositions[(int)ViewTypes.CardsOnly].z = 0;
        camAngles[(int)ViewTypes.CardsOnly].x = 59f;

        camPositions[(int)ViewTypes.BoardCards].x = 0;
        camPositions[(int)ViewTypes.BoardCards].y = cameraHeight;
        camPositions[(int)ViewTypes.BoardCards].z = 0;
        camAngles[(int)ViewTypes.BoardCards].x = 52f;

        camPositions[(int)ViewTypes.BoardOnly].x = 0;
        camPositions[(int)ViewTypes.BoardOnly].y = cameraHeight;
        camPositions[(int)ViewTypes.BoardOnly].z = 0;
        camAngles[(int)ViewTypes.BoardOnly].x = 49.3f;

        camPositions[(int)ViewTypes.Shop].x = -26.7f;
        camPositions[(int)ViewTypes.Shop].y = shopCameraHeight;
        camPositions[(int)ViewTypes.Shop].z = 35.6f;
        camAngles[(int)ViewTypes.Shop].x = 60;
        camAngles[(int)ViewTypes.Shop].y = -90;

        setView(new Vector3(0,0));
    }

    // Update is called once per frame
    void Update()
    {
        handlePlayerInput();
        moveCamera();
    }

    private void moveCamera()
    {
        mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, camPositions[viewType], cameraSpeed * Time.deltaTime);
        mainCamera.transform.rotation = Quaternion.RotateTowards(mainCamera.transform.rotation, _targetRotation, turningRate * Time.deltaTime);
    }

    private void handlePlayerInput()
    {
        //check for changed view mode
        bool viewIsChanged = false;
        if (Input.GetKeyDown(KeyCode.W) && viewType != (int)ViewTypes.Shop)
        {
            if(viewType < (int)ViewTypes.BoardOnly)
            {
                viewType++;
                viewIsChanged = true;
            }
            else
            {
                viewType = (int)ViewTypes.CardsOnly;
                viewIsChanged = true;
            }
        }

        if (Input.GetKeyDown(KeyCode.S) && viewType != (int)ViewTypes.Shop)
        {
            if (viewType > (int)ViewTypes.CardsOnly)
            {
                viewType--;
                viewIsChanged = true;
            }
            else 
            {
                viewType = (int)ViewTypes.BoardOnly;
                viewIsChanged = true;
            }
        }

        if (Input.GetKeyDown(KeyCode.A) && viewType != (int)ViewTypes.Shop)
        {
            lastViewType = viewType;
            lastPosition = mainCamera.transform.position;
            viewType = (int)ViewTypes.Shop;
            viewIsChanged = true;
        }

        if (Input.GetKeyDown(KeyCode.D) && viewType == (int)ViewTypes.Shop)
        {
            viewType = lastViewType;
            setView(lastPosition);
            //viewIsChanged = true;
        }

        Vector3 pos = transform.position;
        if (viewType != (int)ViewTypes.Shop)
        {
            //check if player moved the camera with mouse
            if (Application.isFocused)
            {

                if (Input.mousePosition.y >= Screen.height - mouseBorder)
                {
                    if (mainCamera.transform.position.z < upCameraBoundY)
                    {
                        pos.z += mouseCameraSpeed * Time.deltaTime;
                        viewIsChanged = true;
                    }
                }

                if (Input.mousePosition.y <= mouseBorder)
                {
                    if (mainCamera.transform.position.z > downCameraBoundY)
                    {
                        pos.z -= mouseCameraSpeed * Time.deltaTime;
                        viewIsChanged = true;
                    }
                }

                if (Input.mousePosition.x >= Screen.width - mouseBorder)
                {
                    if (mainCamera.transform.position.x < rightCameraBoundX)
                    {
                        pos.x += mouseCameraSpeed * Time.deltaTime;
                        viewIsChanged = true;
                    }
                }

                if (Input.mousePosition.x <= mouseBorder)
                {
                    if (mainCamera.transform.position.x > leftCameraBoundX)
                    {
                        pos.x -= mouseCameraSpeed * Time.deltaTime;
                        viewIsChanged = true;
                    }
                }
                transform.position = pos;
            }
        }
        

        if (viewIsChanged)
        {
            setView(pos);
        }
    }

    //set coordinates that camera should follow
    private void setView(Vector3 camPosition)
    {
        changeView?.Invoke((ViewTypes)viewType);
        switch (viewType)
        {
            case (int)ViewTypes.CardsOnly:
                SetBlendedEulerAngles(camAngles[viewType]);
                //Debug.Log("view: cards only");
                break;
            case (int)ViewTypes.BoardCards:
                SetBlendedEulerAngles(camAngles[viewType]);
                //Debug.Log("view: board cards");
                break;
            case (int)ViewTypes.BoardOnly:
                SetBlendedEulerAngles(camAngles[viewType]);
                //Debug.Log("view: board only");
                break;
            case (int)ViewTypes.Shop:
                SetBlendedEulerAngles(camAngles[viewType]);
                //Debug.Log("view: shop");
                break;
        }



        for (int i = 0; i < (int)ViewTypes.NumOfViewTypes && i != (int)ViewTypes.Shop; i++)
        {
            camPositions[i].x = camPosition.x;
            camPositions[i].z = camPosition.z;
        }
    }

    //i have no idea what this means
    public void SetBlendedEulerAngles(Vector3 angles)
    {
        _targetRotation = Quaternion.Euler(angles);
    }
}
