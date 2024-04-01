using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using ThemeSelection;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PanelContoller : SingletonComponent<PanelContoller>
{
    [SerializeField] private List<GameObject> tabsContent = new List<GameObject>();
    [SerializeField] private List<GameObject> tabsViewPort = new List<GameObject>();
    [SerializeField] private List<RectTransform> tabsButton = new List<RectTransform>();
    [SerializeField] private RectTransform tabHighlighterRect;
    [SerializeField] private ScrollRect _scrollRect;
    private int currentSelectedTabIndex = 0;
    private bool _canChangeTab = true;

    [SerializeField] public UnityEvent onPanelChange;

    private void Start()
    {
        ChangePanel(0);
    }

    public void Set_Selected_Tab(int index)
    {
        if (index == currentSelectedTabIndex || !_canChangeTab)
            return;

        ChangePanel(index);
    }

    private void ChangePanel(int index)
    { 
        _canChangeTab = false;

        var rectTrans = tabsViewPort[currentSelectedTabIndex].transform.parent.GetComponent<RectTransform>();
        rectTrans.gameObject.SetActive(false);

        //Reset Color
        tabsButton[currentSelectedTabIndex].GetChild(tabsButton[currentSelectedTabIndex].transform.childCount == 1 ? 0 : 1).GetComponent<Image>().color = Color.white;

        //Enable new tab
        var rectTrans1 = tabsViewPort[index].transform.parent.GetComponent<RectTransform>();
        rectTrans1.gameObject.SetActive(true);

        currentSelectedTabIndex = index;

        //Highlight tab
        ResetTabButtonIconColor();
        
        tabHighlighterRect.transform.parent = tabsButton[currentSelectedTabIndex].transform;
        tabHighlighterRect.SetAsFirstSibling();
        tabHighlighterRect.anchoredPosition = new Vector2(0, -10);
        
        _scrollRect.content = tabsContent[currentSelectedTabIndex].GetComponent<RectTransform>();
        _scrollRect.viewport = tabsViewPort[currentSelectedTabIndex].GetComponent<RectTransform>();
        onPanelChange?.Invoke();
        _canChangeTab = true;
    }

    private void ResetTabButtonIconColor()
    {
        for (int i = 0; i < tabsButton.Count; i++)
        {
            tabsButton[i].GetChild(0).GetComponent<Image>().color = i == currentSelectedTabIndex
                ? ThemeSelectionScreenController.Instance.themeButtonIconSelect
                : Color.white;
        }
    }


    public GameObject CurrentContent()
    {
        return tabsContent[currentSelectedTabIndex];
    }

    public int Last_Panel_Index()
    {
        return currentSelectedTabIndex;
    }
}