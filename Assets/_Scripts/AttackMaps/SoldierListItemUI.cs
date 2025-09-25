using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SoldierListItemUI : MonoBehaviour
{
    [SerializeField] private Image soldierIcon;
    [SerializeField] private TextMeshProUGUI soldierCountText;

    /// <summary>
    /// Bu liste elemanını bir askerin ikonu ve sayısıyla doldurur.
    /// </summary>
    public void Setup(Sprite icon, int count)
    {
        if (soldierIcon != null)
        {
            soldierIcon.sprite = icon;
        }
        if (soldierCountText != null)
        {
            soldierCountText.text = count.ToString();
        }
    }
}