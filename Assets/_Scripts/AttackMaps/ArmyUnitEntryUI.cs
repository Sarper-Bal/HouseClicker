// Dosya Adı: ArmyUnitEntryUI.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ArmyUnitEntryUI : MonoBehaviour
{
    [SerializeField] private Image soldierIcon;
    [SerializeField] private TextMeshProUGUI soldierCountText;

    public string SoldierName { get; private set; }

    public void Setup(string soldierName, Sprite icon, int count)
    {
        this.SoldierName = soldierName;
        soldierIcon.sprite = icon;
        soldierCountText.text = "x" + count;
    }
}