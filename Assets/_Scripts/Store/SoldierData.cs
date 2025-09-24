using UnityEngine;

[CreateAssetMenu(fileName = "New SoldierData", menuName = "Game/Soldier Data")]
public class SoldierData : ScriptableObject
{
    [Header("Asker Bilgileri")]
    public string soldierName = "Piyade";
    public Sprite shopIcon;

    [Header("Satın Alma Ayarları")]
    [Tooltip("Bu askerin mağazadaki altın maliyeti.")]
    public long cost = 500;

    [Header("İstatistikler")]
    [Tooltip("Bu askerin orduya kattığı sağlık puanı.")]
    public int health = 10;
    [Tooltip("Bu askerin orduya kattığı saldırı puanı.")]
    public int attack = 2;
}