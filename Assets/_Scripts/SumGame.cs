using System;
using UnityEngine;

public class SumGame : MiniGame
{
    // --- YENİ EKLENEN KISIM ---
    // Bu alana Unity editöründen oyun ayarlarını içeren veri dosyasını sürükleyeceğiz.
    [SerializeField] private SumGameData gameData;
    // -------------------------

    // Artık bu değerler sabit değil, doğrudan 'gameData' dosyasından okunuyor.
    public override string GameID => "sum_game";
    public override long CostToPlay => gameData.costToPlay;
    public override long RewardOnWin => gameData.rewardOnWin;

    public override void Play(Action<MiniGameResult> onGameFinished)
    {
        if (gameData == null)
        {
            Debug.LogError("SumGame için GameData atanmamış!");
            return;
        }

        // Değerleri artık hard-coded değil, 'gameData' dosyasından alıyoruz.
        int num1 = UnityEngine.Random.Range(gameData.minNumber, gameData.maxNumber + 1);
        int num2 = UnityEngine.Random.Range(gameData.minNumber, gameData.maxNumber + 1);
        int total = num1 + num2;

        MiniGameResult result = new MiniGameResult();
        result.NumbersGenerated = $"Sayılar: {num1}, {num2}";

        // Kazanma koşulunu 'gameData' dosyasından kontrol ediyoruz.
        if (total <= gameData.targetSum)
        {
            result.IsWin = true;
            result.Payout = RewardOnWin;
            result.ResultMessage = $"{num1} + {num2} = {total}\nKazandın!";
        }
        else
        {
            result.IsWin = false;
            result.Payout = 0;
            result.ResultMessage = $"{num1} + {num2} = {total}\nKaybettin!";
        }

        onGameFinished?.Invoke(result);
    }
}