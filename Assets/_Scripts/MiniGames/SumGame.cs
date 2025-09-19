using System;
using UnityEngine;

public class SumGame : MiniGame
{
    [SerializeField] private SumGameData gameData;

    // --- YENİ EKLENEN KISIM ---
    // MiniGameUIController'ın bu script'teki verilere temiz bir şekilde erişmesini sağlar.
    public SumGameData GameData => gameData;
    // -------------------------

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

        int num1 = UnityEngine.Random.Range(gameData.minNumber, gameData.maxNumber + 1);
        int num2 = UnityEngine.Random.Range(gameData.minNumber, gameData.maxNumber + 1);
        int total = num1 + num2;

        MiniGameResult result = new MiniGameResult();
        result.NumbersGenerated = $"Sayılar: {num1}, {num2}";

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