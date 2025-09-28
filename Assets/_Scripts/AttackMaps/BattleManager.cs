using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public class BattleManager : MonoBehaviour
{
    // --- HATA DÜZELTMESİ: BattleParticipant sınıfı, orijinal yapıda olduğu gibi BattleManager'ın İÇİNE geri taşındı. ---
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
    // -----------------------------------------------------------------------------------------------------------------

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
    [SerializeField] private RectTransform battlePanelTransform;

    [Header("Ordu Durum Panelleri")]
    [SerializeField] private ArmyListUI playerArmyListUI;
    [SerializeField] private ArmyListUI enemyArmyListUI;

    [Header("Animasyon Referansları")]
    [SerializeField] private BattleAnimator playerAnimator;
    [SerializeField] private BattleAnimator enemyAnimator;

    [Header("Savaş Ayarları")]
    [SerializeField] private float turnDelay = 1.0f;

    private Queue<SoldierData> playerArmyQueue = new Queue<SoldierData>();
    private Queue<SoldierData> enemyArmyQueue = new Queue<SoldierData>();

    private Dictionary<string, int> playerArmyComposition = new Dictionary<string, int>();
    private Dictionary<string, int> enemyArmyComposition = new Dictionary<string, int>();
    private Dictionary<string, Sprite> soldierIconDict = new Dictionary<string, Sprite>();

    private BattleParticipant currentPlayer;
    private BattleParticipant currentEnemy;

    private MapUIController uiController;
    private List<SoldierData> allSoldierTypes;
    private Castle castleBeingFought;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); }
        else { Instance = this; }
    }

    public void SetUIController(MapUIController controller)
    {
        uiController = controller;
    }

    public void StartBattle(List<SoldierData> playerArmy, List<EnemyArmyUnit> enemyArmy, List<SoldierData> allTypes, Castle targetCastle)
    {
        if (uiController == null) return;
        allSoldierTypes = allTypes;
        castleBeingFought = targetCastle;

        PrepareArmies(playerArmy, enemyArmy);

        if (playerArmyQueue.Count == 0 || enemyArmyQueue.Count == 0)
        {
            Debug.LogError("Savaş başlatılamadı! Ordulardan biri boş.");
            uiController.HideAllPanels();
            return;
        }

        uiController.ShowBattlePanel();
        UpdateArmyListDisplays();
        StopAllCoroutines();
        StartCoroutine(BattleLoop());
    }

    private void PrepareArmies(List<SoldierData> playerArmy, List<EnemyArmyUnit> enemyArmy)
    {
        playerArmyQueue.Clear();
        enemyArmyQueue.Clear();
        playerArmyComposition.Clear();
        enemyArmyComposition.Clear();
        soldierIconDict.Clear();

        foreach (var soldierData in playerArmy)
        {
            playerArmyQueue.Enqueue(soldierData);
            if (!playerArmyComposition.ContainsKey(soldierData.soldierName))
            {
                playerArmyComposition[soldierData.soldierName] = 0;
                if (!soldierIconDict.ContainsKey(soldierData.soldierName))
                    soldierIconDict.Add(soldierData.soldierName, soldierData.shopIcon);
            }
            playerArmyComposition[soldierData.soldierName]++;
        }

        foreach (var enemyUnit in enemyArmy)
        {
            for (int i = 0; i < enemyUnit.count; i++)
            {
                enemyArmyQueue.Enqueue(enemyUnit.soldierData);
            }
            if (!enemyArmyComposition.ContainsKey(enemyUnit.soldierData.soldierName))
            {
                enemyArmyComposition[enemyUnit.soldierData.soldierName] = 0;
                if (!soldierIconDict.ContainsKey(enemyUnit.soldierData.soldierName))
                    soldierIconDict.Add(enemyUnit.soldierData.soldierName, enemyUnit.soldierData.shopIcon);
            }
            enemyArmyComposition[enemyUnit.soldierData.soldierName] += enemyUnit.count;
        }
    }

    private IEnumerator BattleLoop()
    {
        LoadNextPlayerSoldier();
        LoadNextEnemySoldier();

        Sequence entranceSequence = DOTween.Sequence();
        if (currentPlayer != null) entranceSequence.Append(playerAnimator.PlaySpawn());
        if (currentEnemy != null) entranceSequence.Join(enemyAnimator.PlaySpawn());
        yield return entranceSequence.WaitForCompletion();

        while (currentPlayer != null && currentEnemy != null)
        {
            yield return new WaitForSeconds(turnDelay);

            Sequence attackSequence = DOTween.Sequence();
            attackSequence.Append(playerAnimator.PlayAttack());
            attackSequence.Join(enemyAnimator.PlayAttack());
            if (battlePanelTransform != null)
                attackSequence.Insert(0.2f, battlePanelTransform.DOShakeAnchorPos(0.2f, new Vector2(15, 0), 20, 90));
            yield return attackSequence.WaitForCompletion();

            long playerAttackPower = currentPlayer.soldierData.attack;
            long enemyAttackPower = currentEnemy.soldierData.attack;
            currentPlayer.currentHealth -= enemyAttackPower;
            currentEnemy.currentHealth -= playerAttackPower;

            Sequence damageSequence = DOTween.Sequence();
            damageSequence.Append(playerAnimator.PlayTakeDamage());
            damageSequence.Join(enemyAnimator.PlayTakeDamage());
            UpdateDisplay(playerDisplay, currentPlayer);
            UpdateDisplay(enemyDisplay, currentEnemy);
            yield return damageSequence.WaitForCompletion();

            bool playerDied = currentPlayer.currentHealth <= 0;
            bool enemyDied = currentEnemy.currentHealth <= 0;

            if (playerDied) playerArmyComposition[currentPlayer.soldierData.soldierName]--;
            if (enemyDied) enemyArmyComposition[currentEnemy.soldierData.soldierName]--;

            Sequence deathSequence = DOTween.Sequence();
            if (playerDied) deathSequence.Append(playerAnimator.PlayDeath());
            if (enemyDied) deathSequence.Join(enemyAnimator.PlayDeath());
            yield return deathSequence.WaitForCompletion();

            if (playerDied || enemyDied) UpdateArmyListDisplays();

            Sequence newSoldierSequence = DOTween.Sequence();
            if (playerDied)
            {
                LoadNextPlayerSoldier();
                if (currentPlayer != null) newSoldierSequence.Append(playerAnimator.PlaySpawn());
            }
            if (enemyDied)
            {
                LoadNextEnemySoldier();
                if (currentEnemy != null) newSoldierSequence.Join(enemyAnimator.PlaySpawn());
            }
            yield return newSoldierSequence.WaitForCompletion();
        }
        EndBattle();
    }

    private void UpdateArmyListDisplays()
    {
        if (playerArmyListUI != null) playerArmyListUI.UpdateList(playerArmyComposition, soldierIconDict);
        if (enemyArmyListUI != null) enemyArmyListUI.UpdateList(enemyArmyComposition, soldierIconDict);
    }

    private void EndBattle()
    {
        bool playerWon = (currentPlayer != null && currentEnemy == null);
        if (playerWon)
        {
            if (WorldMapManager.Instance != null && castleBeingFought != null)
                WorldMapManager.Instance.ConquerCastle(castleBeingFought);
        }
        UpdateArmyRecordsAfterBattle(playerWon);
        uiController.ShowResultPanel(playerWon);
    }

    private void LoadNextPlayerSoldier()
    {
        if (playerArmyQueue.Count > 0)
        {
            currentPlayer = new BattleParticipant(playerArmyQueue.Dequeue());
            if (playerAnimator != null) playerAnimator.ResetAnimator();
            UpdateDisplay(playerDisplay, currentPlayer, true);
        }
        else
        {
            currentPlayer = null;
            if (playerDisplay.displayParent != null) playerDisplay.displayParent.SetActive(false);
        }
    }

    private void LoadNextEnemySoldier()
    {
        if (enemyArmyQueue.Count > 0)
        {
            currentEnemy = new BattleParticipant(enemyArmyQueue.Dequeue());
            if (enemyAnimator != null) enemyAnimator.ResetAnimator();
            UpdateDisplay(enemyDisplay, currentEnemy, true);
        }
        else
        {
            currentEnemy = null;
            if (enemyDisplay.displayParent != null) enemyDisplay.displayParent.SetActive(false);
        }
    }

    private void UpdateDisplay(SoldierDisplayUI display, BattleParticipant participant, bool isNew = false)
    {
        if (display == null || participant == null) return;
        if (display.displayParent != null && !display.displayParent.activeSelf) display.displayParent.SetActive(true);
        if (display.healthText != null)
            display.healthText.text = participant.currentHealth > 0 ? participant.currentHealth.ToString() : "0";
        if (isNew)
        {
            if (display.soldierImage != null)
                display.soldierImage.sprite = participant.soldierData.shopIcon;
            if (display.attackText != null)
                display.attackText.text = participant.soldierData.attack.ToString();
        }
    }

    private void UpdateArmyRecordsAfterBattle(bool playerWon)
    {
        if (allSoldierTypes == null) return;
        if (playerWon)
        {
            var survivingSoldiers = new Dictionary<string, int>();
            if (currentPlayer != null) { playerArmyQueue.Enqueue(currentPlayer.soldierData); }

            foreach (var soldier in playerArmyQueue)
            {
                if (!survivingSoldiers.ContainsKey(soldier.soldierName)) { survivingSoldiers[soldier.soldierName] = 0; }
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
            foreach (var type in allSoldierTypes) { PlayerPrefs.SetInt("SoldierCount_" + type.soldierName, 0); }
        }
        PlayerPrefs.Save();
        if (SoldierManager.Instance != null) { SoldierManager.Instance.RefreshDataFromPrefs(); }
    }
}