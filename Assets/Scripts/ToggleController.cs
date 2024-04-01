using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class ToggleController : MonoBehaviour
{
    [SerializeField] private RectTransform toggleIconRect;
    [SerializeField] private Image toggleBgImage;
    [SerializeField] private Color defaultBgColor, onBgColor;
    private Toggle _toggle;

    private float leftPosIcon, rightPosIcon;

    private void Start()
    {
        rightPosIcon = Mathf.Abs(toggleIconRect.anchoredPosition.x);
        leftPosIcon = -rightPosIcon;
        
        _toggle = GetComponent<Toggle>();

        Move_Toggle_Anime();

        _toggle.onValueChanged.AddListener(delegate {
            Set_Toggle();
        });
    }
    private void Set_Toggle()
    {
        Move_Toggle_Anime();
    }
    
    private void Move_Toggle_Anime()
    {
        if (!_toggle.isOn)
        {
            toggleIconRect.DOAnchorPos(new Vector2(leftPosIcon, 0), 0.2f).SetEase(Ease.Linear);
            toggleBgImage.DOColor(defaultBgColor,0.2f).SetEase(Ease.Linear);
        }
        else
        {
            toggleIconRect.DOAnchorPos(new Vector2(rightPosIcon, 0), 0.2f).SetEase(Ease.Linear);
            toggleBgImage.DOColor(onBgColor,0.2f).SetEase(Ease.Linear);
        }
    }
}
