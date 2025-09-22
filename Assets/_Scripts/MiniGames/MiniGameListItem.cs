using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System; // Action için gerekli

public class MiniGameListItem : MonoBehaviour
{
    [SerializeField] private Image gameIconImage;
    [SerializeField] private TextMeshProUGUI gameNameText;
    [SerializeField] private TextMeshProUGUI gameInfoText;
    [SerializeField] private Button itemButton;

    // Bu elemanı dolduracak olan bilgileri dışarıdan alır ve UI'ı günceller.
    public void Setup(Sprite icon, string name, string info)
    {
        gameIconImage.sprite = icon;
        gameNameText.text = name;
        gameInfoText.text = info;
    }

    // Bu elemana tıklandığında ne olacağını dışarıdan belirlememizi sağlar.
    public void AddClickListener(Action listener)
    {
        itemButton.onClick.AddListener(() => listener());
    }
}