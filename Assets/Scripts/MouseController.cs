using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseController : Singleton<MouseController>
{
    public Action<RaycastHit> OnLeftMouseClick;
    public Action<RaycastHit> OnRightMouseClick;
    public Action<RaycastHit> OnMiddleMouseClick;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            CheckMouseClick(0);
        }
        if (Input.GetMouseButtonDown(1))
        {
            CheckMouseClick(1);
        }
        if (Input.GetMouseButtonDown(2))
        {
            CheckMouseClick(2);
        }
    }

    void CheckMouseClick(int mouseButton)
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        int layer_mask = LayerMask.GetMask("Grid");

        if(Physics.Raycast(ray, out hit, Mathf.Infinity, layer_mask))
        {
            if(mouseButton == 0)
            {
                OnLeftMouseClick?.Invoke(hit);
            }
            else if(mouseButton == 1)
            {
                OnRightMouseClick?.Invoke(hit);
            }
            else if (mouseButton == 2)
            {
                OnMiddleMouseClick?.Invoke(hit);
            }
        }
    }
}
