using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

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

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance { get; private set; }

    [System.Serializable]
    public class SoldierDisplayUI
    {
        public Image soldierImage;
        public TextMeshProUGUI healthText;
        public TextMeshProUGUI attackText;
        public GameObject displayParent;
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
    private List<SoldierData> allSoldierTypes;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); }
        else { Instance = this; }
    }

    public void SetUIController(MapUIController controller)
    {
        uiController = controller;
    }

    public void StartBattle(List<SoldierData> playerArmy, List<EnemyArmyUnit> enemyArmy, List<SoldierData> allTypes)
    {
        if (uiController == null) return;

        allSoldierTypes = allTypes;

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

            currentEnemy.currentHealth -= currentPlayer.soldierData.attack;
            UpdateDisplay(enemyDisplay, currentEnemy);

            if (currentEnemy.currentHealth <= 0)
            {
                LoadNextEnemySoldier();
                continue;
            }

            yield return new WaitForSeconds(0.2f);

            currentPlayer.currentHealth -= currentEnemy.soldierData.attack;
            UpdateDisplay(playerDisplay, currentPlayer);

            if (currentPlayer.currentHealth <= 0)
            {
                LoadNextPlayerSoldier();
            }
        }

        EndBattle();
    }

    private void EndBattle()
    {
        bool playerWon = (currentPlayer != null && currentEnemy == null);

        UpdateArmyRecordsAfterBattle(playerWon);

        uiController.ShowResultPanel(playerWon);
    }

    private void UpdateArmyRecordsAfterBattle(bool playerWon)
    {
        if (allSoldierTypes == null)
        {
            Debug.LogError("allSoldierTypes listesi null, kayıtlar güncellenemedi!");
            return;
        }

        if (playerWon)
        {
            var survivingSoldiers = new Dictionary<string, int>();

            if (currentPlayer != null)
            {
                playerArmyQueue.Enqueue(currentPlayer.soldierData);
            }

            foreach (var soldier in playerArmyQueue)
            {
                if (!survivingSoldiers.ContainsKey(soldier.soldierName))
                {
                    survivingSoldiers[soldier.soldierName] = 0;
                }
                survivingSoldiers[soldier.soldierName]++;
            }

            foreach (var type in allSoldierTypes)
            {
                int newCount = survivingSoldiers.ContainsKey(type.soldierName) ? survivingSoldiers[type.soldierName] : 0;
                PlayerPrefs.SetInt("SoldierCount_" + type.soldierName, newCount);
            }
        }
        else
        {
            foreach (var type in allSoldierTypes)
            {
                PlayerPrefs.SetInt("SoldierCount_" + type.soldierName, 0);
            }
        }

        PlayerPrefs.Save();
        Debug.Log("Asker kayıtları savaş sonucuna göre güncellendi.");

        // --- TEK VE ÖNEMLİ DEĞİŞİKLİK BURADA ---
        // PlayerPrefs güncellendikten sonra, hafızadaki SoldierManager'a da
        // verilerini bu yeni PlayerPrefs'ten tazelemesini söylüyoruz.
        if (SoldierManager.Instance != null)
        {
            SoldierManager.Instance.RefreshDataFromPrefs();
        }
        // ------------------------------------
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
            playerDisplay.displayParent.SetActive(false);
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
            enemyDisplay.displayParent.SetActive(false);
        }
    }

    private void UpdateDisplay(SoldierDisplayUI display, BattleParticipant participant, bool isNew = false)
    {
        if (!display.displayParent.activeSelf) display.displayParent.SetActive(true);
        display.healthText.text = participant.currentHealth.ToString();
        if (isNew)
        {
            display.soldierImage.sprite = participant.soldierData.shopIcon;
            display.attackText.text = participant.soldierData.attack.ToString();
        }
    }
}