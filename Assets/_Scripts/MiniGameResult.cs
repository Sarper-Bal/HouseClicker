// Bu bir script değil, sadece bir veri yapısıdır.
// Bir mini oyunun sonucunu paket halinde taşımamızı sağlar.
public struct MiniGameResult
{
    public bool IsWin; // Oyunu kazandı mı?
    public long Payout; // Kazanılan ödül miktarı (kaybederse 0 olabilir).
    public string ResultMessage; // Ekranda gösterilecek mesaj ("Kazandın!", "10+5=15 Kaybettin!" gibi).
    public string NumbersGenerated; // Üretilen sayıları gösteren metin ("Sayılar: 10, 5" gibi).
}