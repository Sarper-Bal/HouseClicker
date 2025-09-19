using System;
using UnityEngine;

// Bu, bir GameObject'e doğrudan eklenemeyen bir şablon sınıftır.
// Diğer mini oyun script'leri bunu temel alarak yazılacak.
public abstract class MiniGame : MonoBehaviour
{
    // Her oyunun benzersiz bir kimliği olmalı (cooldown takibi için).
    public abstract string GameID { get; }

    // Her oyunun bir oynama maliyeti olmalı.
    public abstract long CostToPlay { get; }

    // Her oyunun bir ödülü olmalı.
    public abstract long RewardOnWin { get; }

    // Her mini oyunun ana oynanış mantığını çalıştıran fonksiyon.
    // 'Action<MiniGameResult>' sayesinde, oyun bittiğinde sonucu dinleyen script'e geri bildirir.
    public abstract void Play(Action<MiniGameResult> onGameFinished);
}