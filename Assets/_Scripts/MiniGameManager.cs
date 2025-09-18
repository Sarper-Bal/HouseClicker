using System;
using System.Collections.Generic;
using UnityEngine;

public class MiniGameManager : MonoBehaviour
{
    public static MiniGameManager Instance { get; private set; }

    [Header("Ayarlar")]
    [Tooltip("Bir mini oyunu oynadıktan sonra tekrar oynamak için gereken bekleme süresi (saniye).")]
    [SerializeField] private float cooldownDuration = 30f;

    // Her oyunun en son ne zaman oynandığını saklamak için bir sözlük.
    private Dictionary<string, DateTime> lastPlayedTimes = new Dictionary<string, DateTime>();

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    // Bir oyunun oynanabilir olup olmadığını kontrol eder.
    public bool CanPlay(string gameId)
    {
        if (lastPlayedTimes.ContainsKey(gameId))
        {
            // Son oynanma zamanından bu yana cooldown süresi geçti mi?
            TimeSpan timeSinceLastPlay = DateTime.Now - lastPlayedTimes[gameId];
            return timeSinceLastPlay.TotalSeconds >= cooldownDuration;
        }

        // Eğer daha önce hiç oynanmadıysa, oynanabilir.
        return true;
    }

    // Bir oyunun oynandığı zamanı kaydeder.
    public void RecordPlayTime(string gameId)
    {
        lastPlayedTimes[gameId] = DateTime.Now;
    }

    // Kalan bekleme süresini saniye olarak verir.
    public float GetRemainingCooldown(string gameId)
    {
        if (lastPlayedTimes.ContainsKey(gameId))
        {
            TimeSpan timeSinceLastPlay = DateTime.Now - lastPlayedTimes[gameId];
            float remainingTime = cooldownDuration - (float)timeSinceLastPlay.TotalSeconds;
            return Mathf.Max(0, remainingTime); // Negatif değer dönmesini engelle.
        }
        return 0;
    }
}