using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq; // Enumerable.Any() kullanabilmek için eklendi

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance { get; private set; }

    [Header("Savaş Alanı Referansları")]
    [SerializeField] private Transform playerSpawnPoint;
    [SerializeField] private Transform enemySpawnPoint;
    [SerializeField] private GameObject battleSoldierPrefab;

    [Header("Savaş Ayarları")]
    [Tooltip("Her tur arası bekleme süresi (saniye).")]
    [SerializeField] private float turnDelay = 1.0f;

    private Queue<SoldierData> playerArmyQueue = new Queue<SoldierData>();
    private Queue<SoldierData> enemyArmyQueue = new Queue<SoldierData>();

    private BattleSoldier currentPlayerSoldier;
    private BattleSoldier currentEnemySoldier;

    private MapUIController uiController;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    public void SetUIController(MapUIController controller)
    {
        uiController = controller;
    }

    public void StartBattle(List<SoldierData> playerArmy, List<EnemyArmyUnit> enemyArmy)
    {
        if (uiController == null)
        {
            Debug.LogError("MapUIController referansı atanmamış!");
            return;
        }

        PrepareArmies(playerArmy, enemyArmy);

        // --- YENİ GÜVENLİK KONTROLÜ ---
        // Ordulardan herhangi biri boşsa savaşı başlatma.
        if (playerArmyQueue.Count == 0 || enemyArmyQueue.Count == 0)
        {
            Debug.LogError("Savaş başlatılamadı! Ordulardan biri boş.");
            // Oyuncuyu bilgilendirmek için harita ekranına geri dönebilir veya bir mesaj gösterebiliriz.
            // Şimdilik sadece panelleri gizleyip işlemi durduruyoruz.
            uiController.HideAllPanels();
            return;
        }
        // --------------------------------

        uiController.ShowBattlePanel();
        StartCoroutine(BattleLoop());
    }

    private void PrepareArmies(List<SoldierData> playerArmy, List<EnemyArmyUnit> enemyArmy)
    {
        playerArmyQueue.Clear();
        foreach (var soldierData in playerArmy)
        {
            playerArmyQueue.Enqueue(soldierData);
        }

        enemyArmyQueue.Clear();
        foreach (var enemyUnit in enemyArmy)
        {
            for (int i = 0; i < enemyUnit.count; i++)
            {
                enemyArmyQueue.Enqueue(enemyUnit.soldierData);
            }
        }

        // --- YENİ DEBUG MESAJLARI ---
        // Bu mesajlar sayesinde orduların doğru yüklenip yüklenmediğini anlayacağız.
        Debug.Log($"Savaş hazırlanıyor... Oyuncu askeri sayısı: {playerArmyQueue.Count}, Düşman askeri sayısı: {enemyArmyQueue.Count}");
        // -----------------------------
    }

    private IEnumerator BattleLoop()
    {
        SpawnPlayerSoldier();
        SpawnEnemySoldier();

        // Daha güvenli bir döngü yapısı
        while (true)
        {
            // Her turun başında kazanma/kaybetme durumunu kontrol et
            if (currentPlayerSoldier == null || currentEnemySoldier == null)
            {
                break; // Savaş bitti, döngüden çık
            }

            yield return new WaitForSeconds(turnDelay);

            // Oyuncu Düşmana Saldırır
            if (currentPlayerSoldier != null && currentEnemySoldier != null)
            {
                long playerAttack = currentPlayerSoldier.GetAttackPower();
                currentEnemySoldier.TakeDamage(playerAttack);
            }

            // Düşman Oyuncuya Saldırır (eğer hala hayattaysa)
            if (currentPlayerSoldier != null && currentEnemySoldier != null)
            {
                long enemyAttack = currentEnemySoldier.GetAttackPower();
                currentPlayerSoldier.TakeDamage(enemyAttack);
            }
        }

        EndBattle();
    }

    private void SpawnPlayerSoldier()
    {
        if (playerArmyQueue.Count > 0)
        {
            SoldierData soldierToSpawn = playerArmyQueue.Dequeue();
            GameObject soldierGO = Instantiate(battleSoldierPrefab, playerSpawnPoint.position, Quaternion.identity, playerSpawnPoint);
            currentPlayerSoldier = soldierGO.GetComponent<BattleSoldier>();
            currentPlayerSoldier.Setup(soldierToSpawn);
            currentPlayerSoldier.OnSoldierDied += OnPlayerSoldierDied;
        }
        else
        {
            currentPlayerSoldier = null;
        }
    }

    private void SpawnEnemySoldier()
    {
        if (enemyArmyQueue.Count > 0)
        {
            SoldierData soldierToSpawn = enemyArmyQueue.Dequeue();
            GameObject soldierGO = Instantiate(battleSoldierPrefab, enemySpawnPoint.position, Quaternion.identity, enemySpawnPoint);
            currentEnemySoldier = soldierGO.GetComponent<BattleSoldier>();
            currentEnemySoldier.Setup(soldierToSpawn);
            currentEnemySoldier.OnSoldierDied += OnEnemySoldierDied;
        }
        else
        {
            currentEnemySoldier = null;
        }
    }

    private void OnPlayerSoldierDied(BattleSoldier soldier)
    {
        soldier.OnSoldierDied -= OnPlayerSoldierDied;
        SpawnPlayerSoldier();
    }

    private void OnEnemySoldierDied(BattleSoldier soldier)
    {
        soldier.OnSoldierDied -= OnEnemySoldierDied;
        SpawnEnemySoldier();
    }

    private void EndBattle()
    {
        bool playerWon = (currentPlayerSoldier != null && currentEnemySoldier == null);

        if (currentPlayerSoldier != null) Destroy(currentPlayerSoldier.gameObject);
        if (currentEnemySoldier != null) Destroy(currentEnemySoldier.gameObject);

        uiController.ShowResultPanel(playerWon);
    }
}