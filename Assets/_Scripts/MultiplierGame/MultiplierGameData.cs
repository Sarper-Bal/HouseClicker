using UnityEngine;
using System.Collections.Generic; // List kullanabilmek için

[CreateAssetMenu(fileName = "MultiplierGameData", menuName = "Mini Games/Multiplier Game Data")]
public class MultiplierGameData : ScriptableObject
{
    [Header("Genel Bilgiler")]
    [Tooltip("Oyunun adı (MiniGameManager için ID olarak kullanılacak).")]
    public string gameName = "Çarpan Kutuları";

    [Tooltip("Oyuna girmek için yatırılacak sabit bahis miktarı.")]
    public long betAmount = 100;

    [Header("Çarpan Havuzu")]
    [Tooltip("Kutulardan çıkabilecek pozitif çarpanların listesi. Örneğin 1.5 (x1.5), 2 (x2) gibi.")]
    public List<float> positiveMultipliers = new List<float> { 1.5f, 2f, 2.5f, 3f };

    [Tooltip("Kutulardan çıkabilecek negatif çarpanların (bölenlerin) listesi. Örneğin -2 (-x2), -4 (-x4) gibi. Para bu sayıya bölünecektir.")]
    public List<float> negativeMultipliers = new List<float> { -2f, -4f };
}