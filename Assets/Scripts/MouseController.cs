using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using GeneralEnumerations;

public class MouseController : Singleton<MouseController>
{
    public Action<RaycastHit> OnLeftMouseClick;
    public Action<RaycastHit> OnRightMouseClick;
    public Action<RaycastHit> OnMiddleMouseClick;
    public Action<RaycastHit, bool> SetCursor;
    public Action<RaycastHit> SetSelectedCursor;
    public Action<RaycastHit> SetMultipleCursor;
    public Action NextPhase;
    //public Action<GameObject, int> BuyCard;
    public delegate ErrorMsg buyCard(GameObject card, int coins);
    public static buyCard BuyCard;

    public DeckManager DeckManager;

    private Transform mouseOverRecent;
    private Transform mouseOver;
    
    private const float SCALE_UP = 1.1f;
    private const float SCALE_DOWN = 1.0f;
    private const float MOVE_UP = 0.5f;
    private const float MOVE_DOWN = 0f;
    private const float LT_TIME = 0.2f;

    public void Start()
    {
        DeckManager = GameObject.FindObjectOfType<DeckManager>();
    }

    // Update is called once per frame
    void Update()
    {

        if (GameLoop.isMyTurn)
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
            CheckMouseOverCard();
        } 
    }

    //I decided to make another check for cards because the other method seems
    //to be strongly bound to checking if cursor is over board. 
    //I'd contemplate changing the name of the method because it's misleading
    void CheckMouseOverCard()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        int layer_mask_card_hand = LayerMask.GetMask("Cards");
        if (!EventSystem.current.IsPointerOverGameObject() &&
            Physics.Raycast(ray, out hit, Mathf.Infinity, layer_mask_card_hand))
        {
            SetCursor?.Invoke(hit, true);
        }
        else
        {
            SetCursor?.Invoke(new RaycastHit(), false);
        }
    }

    void CheckMouseOver()
    {
        //======== Mouse Over a Cell ========//
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        RaycastHit hitGrid;
        int layer_mask_grid = LayerMask.GetMask("Grid");
        int layer_mask = LayerMask.GetMask("Cells");
        int layer_mask_card_hand = LayerMask.GetMask("Cards");
        if (!EventSystem.current.IsPointerOverGameObject() && 
            Physics.Raycast(ray, out hit, Mathf.Infinity, layer_mask) &&
            Physics.Raycast(ray, out hitGrid, Mathf.Infinity, layer_mask_grid) &&
            checkIfNotOccupiedPosition(getCellPosition(hitGrid)) &&
            !Physics.Raycast(ray, Mathf.Infinity, layer_mask_card_hand))
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
    
    // ReSharper disable Unity.PerformanceAnalysis
    void CheckMouseClick(int mouseButton)
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        int layer_mask_grid = LayerMask.GetMask("Grid");
        int layer_mask_card = LayerMask.GetMask("Cards");

        //if the cursos is not pointing on the ui element
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            //if it is over board 
            if(Physics.Raycast(ray, out hit, Mathf.Infinity, layer_mask_grid)
                && !Physics.Raycast(ray, Mathf.Infinity, layer_mask_card))
            {
                if (mouseButton == 0)
                {
                    OnLeftMouseClick?.Invoke(hit);
                }
                else if (mouseButton == 1)
                {
                    OnRightMouseClick?.Invoke(hit);
                }
                else if (mouseButton == 2)
                {
                    OnMiddleMouseClick?.Invoke(hit);
                }
                return;
            }

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, layer_mask_card))
            {
                GameObject card = hit.collider.gameObject;
                if (mouseButton == 0)
                {
                    if (card.CompareTag("Card_Hand") && GameLoop.PlayerPhase == Phase.MOVEMENT_PHASE) {
                        SetSelectedCursor?.Invoke(hit);
                    }
                    else if (card.CompareTag("Card_Hand") 
                        && (GameLoop.PlayerPhase == Phase.BUYING_PHASE || GameLoop.PlayerPhase == Phase.REDRAW_PHASE))
                    {
                        SetMultipleCursor?.Invoke(hit);
                    }
                    else if (card.CompareTag("Card_Shop") && GameLoop.PlayerPhase == Phase.BUYING_PHASE)
                    {
                        ErrorMsg msg = (ErrorMsg)(BuyCard?.Invoke(hit.collider.gameObject, DeckManager.getSumOfCoins()));
                        if(msg == ErrorMsg.OK)
                        {
                            DeckManager.clearMultipleChosenCards();
                            NextPhase?.Invoke();
                        }
                    }
                }
                else if(mouseButton == 1)
                {
                    if (card.CompareTag("Card_Hand") && GameLoop.PlayerPhase == Phase.MOVEMENT_PHASE)
                    {
                        SetMultipleCursor?.Invoke(hit);
                    }
                }
                return;
            }
        }
    }
    
    private void showMouseOver(ref Transform obj, float scale, float move)
    {
        LeanTween.scale(obj.gameObject, Vector3.one * scale, LT_TIME).setEase(LeanTweenType.easeOutBack);
        LeanTween.moveY(obj.gameObject, move, LT_TIME).setEase(LeanTweenType.easeOutBack);
    }

    private PawnPosition getCellPosition(RaycastHit hit)
    {
        HexGrid grid = hit.transform.GetComponentInParent<HexGrid>();
        float localX = hit.point.x - hit.transform.position.x;
        float localZ = hit.point.z - hit.transform.position.z;
        int x = (int)HexMetrics.CoordinateToAxial(localX, localZ, grid.HexSize, grid.Orientation).x;
        int z = (int)HexMetrics.CoordinateToAxial(localX, localZ, grid.HexSize, grid.Orientation).y;
        //Debug.Log("Hoover cell cords: " + new Vector2(x, z));
        PawnPosition position = new PawnPosition(grid.BoardPieceLetter, new Vector2(x, z));
        return position;
    }
    public bool checkIfNotOccupiedPosition(PawnPosition position)
    {
        foreach (PawnPosition pawnPosition in BoardSingleton.instance.PawnPositions)
            if (pawnPosition.EqualsTo(position)) return false;
        return true;
    }
}
