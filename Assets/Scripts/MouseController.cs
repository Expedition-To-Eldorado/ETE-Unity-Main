using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MouseController : Singleton<MouseController>
{
    public Action<RaycastHit> OnLeftMouseClick;
    public Action<RaycastHit> OnRightMouseClick;
    public Action<RaycastHit> OnMiddleMouseClick;
    private Transform mouseOverRecent;
    private Transform mouseOver;
    
    private const float SCALE_UP = 1.1f;
    private const float SCALE_DOWN = 1.0f;
    private const float MOVE_UP = 0.5f;
    private const float MOVE_DOWN = 0f;
    private const float LT_TIME = 0.2f;

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
        CheckMouseOver();
    }

    void CheckMouseOver()
    {
        //======== Mouse Over a Cell ========//
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        int layer_mask = LayerMask.GetMask("Cells");
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, layer_mask))
        {
            mouseOver = hit.transform;
            // if mouse is over different cell than before
            if (mouseOverRecent != null && mouseOverRecent != mouseOver) 
            {
                // change back recent cell
                showMouseOver(ref mouseOverRecent, SCALE_DOWN, MOVE_DOWN);
                // scale up actual mouse over cell
                showMouseOver(ref mouseOver, SCALE_UP, MOVE_UP);
            }
            if(mouseOverRecent == null)
            {
                // scale up actual mouse over cell
                showMouseOver(ref mouseOver, SCALE_UP, MOVE_UP);
            }
        }
        else
        {
            if(mouseOverRecent != null)
            {
                //hoovered beside the board so scale down recent mouse over
                showMouseOver(ref mouseOverRecent, SCALE_DOWN, MOVE_DOWN);
            }
            mouseOver = null;
        }
        mouseOverRecent = mouseOver;
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
    
    void showMouseOver(ref Transform obj, float scale, float move)
    {
        LeanTween.scale(obj.gameObject, Vector3.one * scale, LT_TIME).setEase(LeanTweenType.easeOutBack);
        LeanTween.moveY(obj.gameObject, move, LT_TIME).setEase(LeanTweenType.easeOutBack);
    }
}
