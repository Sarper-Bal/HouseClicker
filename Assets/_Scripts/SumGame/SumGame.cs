using System;
using UnityEngine;

public class SumGame : MiniGame
{
    [SerializeField] private SumGameData gameData;

    public SumGameData GameData => gameData;

    // MiniGameManager'ın bekleme süresini takip edebilmesi için oyunun adını ID olarak kullanıyoruz.
    public override string GameID => gameData != null ? gameData.gameName : "sum_game_default";
    public override long CostToPlay => gameData != null ? gameData.costToPlay : 0;
    public override long RewardOnWin => gameData != null ? gameData.rewardOnWin : 0;

    // Bu metodun içi artık boş çünkü tüm oyun mantığı kendi kontrolcüsünde (SumGameController).
    // Sadece MiniGame soyut sınıfının yapısını korumak için burada bulunuyor.
    public override void Play(Action<MiniGameResult> onGameFinished)
    {
        // Oyun mantığı SumGameController.cs içerisine taşındı.
    }
}