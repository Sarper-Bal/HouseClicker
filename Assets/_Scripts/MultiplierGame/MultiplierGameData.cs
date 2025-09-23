using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "MultiplierGameData", menuName = "Mini Games/Multiplier Game Data")]
public class MultiplierGameData : ScriptableObject
{
    [Header("Genel Bilgiler")]
    public string gameName = "Çarpan Kutuları";

    [Header("Dinamik Bahis Ayarları")]
    [Tooltip("Oyunun 1. seviyedeki temel bahis maliyeti.")]
    public long baseBetAmount = 100;

    [Tooltip("Her seviye atlandığında bahis maliyetine eklenecek miktar.")]
    public long amountPerLevel = 50;

    [Header("Çarpan Havuzu")]
    [Tooltip("Kutulardan çıkabilecek pozitif çarpanların listesi. Örneğin 1.5 (x1.5), 2 (x2) gibi.")]
    public List<float> positiveMultipliers = new List<float> { 1.5f, 2f, 2.5f, 3f };

    [Tooltip("Kutulardan çıkabilecek negatif çarpanların (bölenlerin) listesi. Örneğin -2 (-x2), -4 (-x4) gibi.")]
    public List<float> negativeMultipliers = new List<float> { -2f, -4f };
}