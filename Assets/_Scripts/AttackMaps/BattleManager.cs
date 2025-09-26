using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

// --- YENİ YARDIMCI CLASS ---
// Savaş alanındaki bir askerin anlık durumunu tutmak için.
public class BattleParticipant
{
    public SoldierData soldierData;
    public long currentHealth;

    public BattleParticipant(SoldierData data)
    {
        soldierData = data;
        currentHealth = data.health;
    }
}
// -------------------------

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance { get; private set; }

    [System.Serializable]
    public class SoldierDisplayUI
    {
        public Image soldierImage;
        public TextMeshProUGUI healthText;
        public TextMeshProUGUI attackText;
        public GameObject displayParent; // Bütün display'i açıp kapamak için
    }

    [Header("UI Referansları")]
    [SerializeField] private SoldierDisplayUI playerDisplay;
    [SerializeField] private SoldierDisplayUI enemyDisplay;

    [Header("Savaş Ayarları")]
    [SerializeField] private float turnDelay = 1.0f;

    private Queue<SoldierData> playerArmyQueue = new Queue<SoldierData>();
    private Queue<SoldierData> enemyArmyQueue = new Queue<SoldierData>();

    private BattleParticipant currentPlayer;
    private BattleParticipant currentEnemy;

    private MapUIController uiController;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); }
        else { Instance = this; }
    }

    public void SetUIController(MapUIController controller)
    {
        uiController = controller;
    }

    public void StartBattle(List<SoldierData> playerArmy, List<EnemyArmyUnit> enemyArmy)
    {
        if (uiController == null) return;

        PrepareArmies(playerArmy, enemyArmy);
        if (playerArmyQueue.Count == 0 || enemyArmyQueue.Count == 0)
        {
            Debug.LogError("Savaş başlatılamadı! Ordulardan biri boş.");
            uiController.HideAllPanels();
            return;
        }

        uiController.ShowBattlePanel();
        StartCoroutine(BattleLoop());
    }

    private void PrepareArmies(List<SoldierData> playerArmy, List<EnemyArmyUnit> enemyArmy)
    {
        playerArmyQueue.Clear();
        foreach (var soldierData in playerArmy) { playerArmyQueue.Enqueue(soldierData); }

        enemyArmyQueue.Clear();
        foreach (var enemyUnit in enemyArmy)
        {
            for (int i = 0; i < enemyUnit.count; i++) { enemyArmyQueue.Enqueue(enemyUnit.soldierData); }
        }
        Debug.Log($"Savaş hazırlanıyor... Oyuncu: {playerArmyQueue.Count}, Düşman: {enemyArmyQueue.Count}");
    }

    private IEnumerator BattleLoop()
    {
        LoadNextPlayerSoldier();
        LoadNextEnemySoldier();

        while (currentPlayer != null && currentEnemy != null)
        {
            yield return new WaitForSeconds(turnDelay);

            // Saldırı Aşaması
            currentEnemy.currentHealth -= currentPlayer.soldierData.attack;
            UpdateDisplay(enemyDisplay, currentEnemy);

            if (currentEnemy.currentHealth <= 0)
            {
                LoadNextEnemySoldier();
                continue; // Düşman öldü, oyuncunun hasar almasını beklemeden turu bitir.
            }

            yield return new WaitForSeconds(0.2f); // Karşılık verme hissi için küçük bir bekleme

            currentPlayer.currentHealth -= currentEnemy.soldierData.attack;
            UpdateDisplay(playerDisplay, currentPlayer);

            if (currentPlayer.currentHealth <= 0)
            {
                LoadNextPlayerSoldier();
            }
        }

        EndBattle();
    }

    private void LoadNextPlayerSoldier()
    {
        if (playerArmyQueue.Count > 0)
        {
            currentPlayer = new BattleParticipant(playerArmyQueue.Dequeue());
            UpdateDisplay(playerDisplay, currentPlayer, true);
        }
        else
        {
            currentPlayer = null;
            playerDisplay.displayParent.SetActive(false); // Oyuncunun askeri bitti, display'i kapat
        }
    }

    private void LoadNextEnemySoldier()
    {
        if (enemyArmyQueue.Count > 0)
        {
            currentEnemy = new BattleParticipant(enemyArmyQueue.Dequeue());
            UpdateDisplay(enemyDisplay, currentEnemy, true);
        }
        else
        {
            currentEnemy = null;
            enemyDisplay.displayParent.SetActive(false); // Düşmanın askeri bitti, display'i kapat
        }
    }

    // Display'i güncelleyen ana fonksiyon
    private void UpdateDisplay(SoldierDisplayUI display, BattleParticipant participant, bool isNew = false)
    {
        if (!display.displayParent.activeSelf) display.displayParent.SetActive(true);

        // Sadece canı güncelle
        display.healthText.text = participant.currentHealth.ToString();

        // Eğer yeni bir asker geldiyse, diğer bilgileri de güncelle
        if (isNew)
        {
            display.soldierImage.sprite = participant.soldierData.shopIcon;
            display.attackText.text = participant.soldierData.attack.ToString();
        }
    }

    private void EndBattle()
    {
        // Kazanan, mevcut askeri kalan taraftır.
        bool playerWon = (currentPlayer != null && currentEnemy == null);
        uiController.ShowResultPanel(playerWon);
    }
}