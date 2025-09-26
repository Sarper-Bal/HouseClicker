using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class MapUIController : MonoBehaviour
{
    [Header("Panel Referansları")]
    [SerializeField] private GameObject playerCastlePanel;
    [SerializeField] private GameObject enemyCastlePanel;
    [SerializeField] private GameObject battlePanel; // --- YENİ EKLENDİ ---
    [SerializeField] private GameObject battleResultPanel; // --- YENİ EKLENDİ ---

    [Header("Oyuncu Kalesi Paneli UI")]
    [SerializeField] private Transform soldierListContent;
    [SerializeField] private GameObject soldierListItemPrefab;
    [SerializeField] private Button closePlayerPanelButton;

    [Header("Düşman Kalesi Paneli UI")]
    [SerializeField] private TextMeshProUGUI enemyCastleNameText;
    [SerializeField] private Transform enemySoldierListContent;
    [SerializeField] private Button closeEnemyPanelButton;
    [SerializeField] private Button attackButton;

    // --- YENİ EKLENEN KISIM ---
    [Header("Savaş Sonuç Paneli UI")]
    [SerializeField] private TextMeshProUGUI resultText;
    [SerializeField] private Button closeResultPanelButton;
    // -------------------------

    [Header("Genel Butonlar")]
    [SerializeField] private Button backToMainSceneButton;

    private void Start()
    {
        closePlayerPanelButton.onClick.AddListener(HideAllPanels);
        closeEnemyPanelButton.onClick.AddListener(HideAllPanels);
        backToMainSceneButton.onClick.AddListener(() => SceneLoader.Instance.LoadMainScene());

        // --- YENİ EKLENEN BUTON OLAYLARI ---
        attackButton.onClick.AddListener(OnAttackButtonClicked);
        closeResultPanelButton.onClick.AddListener(HideAllPanels);
        BattleManager.Instance.SetUIController(this); // BattleManager'a bu script'in referansını ver
        // ------------------------------------

        HideAllPanels();
    }

    // --- YENİ EKLENEN FONKSİYON ---
    private void OnAttackButtonClicked()
    {
        HideAllPanels();
        WorldMapManager.Instance.InitiateBattle();
    }
    // -------------------------

    public void ShowPlayerCastlePanel()
    {
        HideAllPanels();
        playerCastlePanel.SetActive(true);
        PopulateSoldierList();
    }

    public void ShowEnemyCastlePanel(EnemyCastleData castleData, bool isAttackable)
    {
        HideAllPanels();
        enemyCastlePanel.SetActive(true);
        attackButton.interactable = isAttackable;
        if (castleData != null)
        {
            enemyCastleNameText.text = castleData.castleName;
            PopulateEnemySoldierList(castleData);
        }
    }

    // --- YENİ EKLENEN PANEL KONTROL FONKSİYONLARI ---
    public void ShowBattlePanel()
    {
        HideAllPanels();
        battlePanel.SetActive(true);
    }

    public void ShowResultPanel(bool playerWon)
    {
        HideAllPanels();
        battleResultPanel.SetActive(true);
        resultText.text = playerWon ? "KAZANDIN!" : "KAYBETTİN!";
    }
    // -----------------------------------------------

    private void PopulateSoldierList()
    {
        foreach (Transform child in soldierListContent) { Destroy(child.gameObject); }
        if (SoldierManager.Instance == null || SoldierManager.Instance.allSoldierTypes == null) return;
        foreach (SoldierData soldierType in SoldierManager.Instance.allSoldierTypes)
        {
            int count = SoldierManager.Instance.GetSoldierCount(soldierType.soldierName);
            if (count > 0)
            {
                GameObject itemGO = Instantiate(soldierListItemPrefab, soldierListContent);
                itemGO.GetComponent<SoldierListItemUI>()?.Setup(soldierType.shopIcon, count);
            }
        }
    }

    private void PopulateEnemySoldierList(EnemyCastleData castleData)
    {
        foreach (Transform child in enemySoldierListContent) { Destroy(child.gameObject); }
        if (castleData.armyComposition == null) return;
        foreach (EnemyArmyUnit unit in castleData.armyComposition)
        {
            if (unit.count > 0 && unit.soldierData != null)
            {
                GameObject itemGO = Instantiate(soldierListItemPrefab, enemySoldierListContent);
                itemGO.GetComponent<SoldierListItemUI>()?.Setup(unit.soldierData.shopIcon, unit.count);
            }
        }
    }

    public void HideAllPanels()
    {
        playerCastlePanel.SetActive(false);
        enemyCastlePanel.SetActive(false);
        battlePanel.SetActive(false); // --- YENİ EKLENDİ ---
        battleResultPanel.SetActive(false); // --- YENİ EKLENDİ ---
    }
}