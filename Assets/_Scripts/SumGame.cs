using System;
using UnityEngine;

public class SumGame : MiniGame
{
    // Şablondan gelen kuralları burada dolduruyoruz.
    public override string GameID => "sum_game";
    public override long CostToPlay => 100; // Örnek bir maliyet, Inspector'dan değiştirilebilir yapabiliriz.
    public override long RewardOnWin => 1000; // Örnek bir ödül.

    public override void Play(Action<MiniGameResult> onGameFinished)
    {
        // 1. Rastgele iki sayı üret (1-10 arası, 11 dahil değil).
        int num1 = UnityEngine.Random.Range(1, 11);
        int num2 = UnityEngine.Random.Range(1, 11);
        int total = num1 + num2;

        // 2. Sonuç için bir veri paketi hazırla.
        MiniGameResult result = new MiniGameResult();
        result.NumbersGenerated = $"Sayılar: {num1}, {num2}";

        // 3. Kazanma/kaybetme durumunu kontrol et.
        if (total <= 10)
        {
            // Kazandı!
            result.IsWin = true;
            result.Payout = RewardOnWin;
            result.ResultMessage = $"{num1} + {num2} = {total}\nKazandın!";
        }
        else
        {
            // Kaybetti!
            result.IsWin = false;
            result.Payout = 0; // Bir şey kazanmadı.
            result.ResultMessage = $"{num1} + {num2} = {total}\nKaybettin!";
        }

        // 4. Sonucu, bu fonksiyonu çağıran script'e geri gönder.
        onGameFinished?.Invoke(result);
    }
}