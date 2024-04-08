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
    public Action<RaycastHit> MouseOverEnter;
    public Action<RaycastHit> MouseOverExit;
    private Transform mouseOverRecent;
    private Transform mouseOver;

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
                LeanTween.scale(mouseOverRecent.gameObject, Vector3.one, 0.2f).setEase(LeanTweenType.easeOutBack);
                LeanTween.moveY(mouseOverRecent.gameObject, 0f, 0.2f).setEase(LeanTweenType.easeOutBack);
                // scale up actual mouse over cell
                LeanTween.scale(mouseOver.gameObject, Vector3.one * 1.1f, 0.2f).setEase(LeanTweenType.easeOutBack);
                LeanTween.moveY(mouseOver.gameObject, 0.5f, 0.2f).setEase(LeanTweenType.easeOutBack);

            }
            if(mouseOverRecent == null)
            {
                // scale up actual mouse over cell
                LeanTween.scale(mouseOver.gameObject, Vector3.one * 1.1f, 0.2f).setEase(LeanTweenType.easeOutBack);
                LeanTween.moveY(mouseOver.gameObject, 0.5f, 0.2f).setEase(LeanTweenType.easeOutBack);
            }
            //MouseOverEnter?.Invoke(hit);
        }
        else
        {
            if(mouseOverRecent != null)
            {
                LeanTween.scale(mouseOver.gameObject, Vector3.one, 0.2f).setEase(LeanTweenType.easeOutBack);
                LeanTween.moveY(mouseOver.gameObject, 0f, 0.2f).setEase(LeanTweenType.easeOutBack);
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
}
