using UnityEngine;

// Bu satır sayesinde Unity'de sağ tık -> Create menüsünde "Game/Level Data" seçeneği çıkar.
[CreateAssetMenu(fileName = "New Level Data", menuName = "Game/Level Data")]
public class LevelData : ScriptableObject
{
    [Header("Level Configuration")]
    public int levelIndex; // Sadece bilgilendirme için, bu seviyenin kaçıncı seviye olduğu.

    [Header("Progression")]
    public long upgradeCost; // Bu seviyeye GEÇMEK için gereken maliyet.

    [Header("Income")]
    public long goldPerClick; // Bu seviyedeyken tıklama başına kazanılacak altın.
    public long goldPerSecond; // Bu seviyedeyken saniye başına kazanılacak pasif altın.

    [Header("Visuals")]
    public Sprite houseSprite; // Bu seviyede evin nasıl görüneceği.

    // --- YENİ EKLENEN KISIM ---
    [Header("Cosmetics")]
    [Tooltip("Bu seviyeye ulaşıldığında gösterilecek olan kozmetik objelerin ID'leri.")]
    public string[] cosmeticObjectsToShow;
    // -------------------------
}