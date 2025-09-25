using UnityEngine;

[CreateAssetMenu(fileName = "New EnemyCastleData", menuName = "Game/Enemy Castle Data")]
public class EnemyCastleData : ScriptableObject
{
    [Header("Kale Bilgileri")]
    public string castleName = "Düşman Kalesi";
    public Sprite castleSprite; // Haritada görünecek görseli

    [Header("Ordu Gücü")]
    [Tooltip("Bu kaledeki toplam asker sayısı.")]
    public int soldiers = 10;
    [Tooltip("Bu ordunun toplam sağlık puanı.")]
    public long totalHealth = 100;
    [Tooltip("Bu ordunun toplam saldırı puanı.")]
    public long totalAttack = 20;

    [Header("Fetih Ödülü")]
    [Tooltip("Bu kale fethedildiğinde kazanılacak altın miktarı.")]
    public long rewardGold = 1000;
}