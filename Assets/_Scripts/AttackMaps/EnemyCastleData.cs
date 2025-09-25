using UnityEngine;
using System.Collections.Generic; // List kullanabilmek için eklendi

// --- YENİ EKLENEN YARDIMCI CLASS ---
// Bu class, hangi askerden (SoldierData) kaç tane (count) olacağını belirtir.
// [System.Serializable] sayesinde Unity editöründe bu listeyi düzenleyebiliriz.
[System.Serializable]
public class EnemyArmyUnit
{
    public SoldierData soldierData;
    public int count;
}
// ---------------------------------

[CreateAssetMenu(fileName = "New Enemy Castle Data", menuName = "Game/Enemy Castle Data")]
public class EnemyCastleData : ScriptableObject
{
    [Header("Kale Bilgileri")]
    public string castleName;
    public Sprite castleSprite;

    // --- DEĞİŞTİRİLEN KISIM ---
    // Eski sabit değerler kaldırıldı.
    // public int soldiers;
    // public long totalHealth;
    // public long totalAttack;

    // Yeni, esnek ordu kompozisyonu listesi eklendi.
    [Header("Ordu Kompozisyonu")]
    public List<EnemyArmyUnit> armyComposition;
    // -------------------------
}