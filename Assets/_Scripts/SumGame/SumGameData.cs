using UnityEngine;

[CreateAssetMenu(fileName = "SumGameData", menuName = "Mini Games/Sum Game Data")]
public class SumGameData : ScriptableObject
{
    [Header("Liste Görünümü")]
    [Tooltip("Oyunun seçim menüsünde görünecek adı.")]
    public string gameName = "Sayı Toplama";

    [Tooltip("Oyunun seçim menüsünde görünecek ikonu/resmi.")]
    public Sprite gameIcon;

    [Header("Oyun Kuralları")]
    [Tooltip("Oyunu oynamanın maliyeti.")]
    public long costToPlay = 100;

    [Tooltip("Oyunu kazandığında verilecek ödül.")]
    public long rewardOnWin = 1000;

    [Header("Sayı Ayarları")]
    [Tooltip("Üretilecek rastgele sayılar için minimum değer (bu değer dahil).")]
    public int minNumber = 1;

    [Tooltip("Üretilecek rastgele sayılar için maksimum değer (bu değer dahil).")]
    public int maxNumber = 10;

    [Tooltip("Sayıların toplamı bu değere eşit veya küçükse oyuncu kazanır.")]
    public int targetSum = 10;
}