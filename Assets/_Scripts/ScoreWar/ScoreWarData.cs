using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ScoreWarData", menuName = "Mini Games/Score War Data")]
public class ScoreWarData : ScriptableObject
{
    [Header("Genel Bilgiler")]
    public string gameName = "Puan Savaşı";

    //[Tooltip("Oyunu kazandığında verilecek sabit ödül miktarı.")]
    //public long rewardAmount = 500;

    [Tooltip("Beraberlik durumunda giriş ücreti iade edilsin mi?")]
    public bool refundOnDraw = true;

    [Header("Dinamik Bahis Ayarları")]
    [Tooltip("Oyunun 1. seviyedeki temel bahis maliyeti.")]
    public long baseBetAmount = 100;

    [Tooltip("Her seviye atlandığında bahis maliyetine eklenecek miktar.")]
    public long amountPerLevel = 50;

    [Header("Puan Ayarları")]
    [Tooltip("Oyuncu ve rakibin başlangıç puanı.")]
    public int startScore = 100;

    [Header("Değer Havuzu")]
    [Tooltip("Kutulardan çıkabilecek toplama ve çıkarma değerleri listesi.")]
    public List<int> additiveModifiers = new List<int> { 10, 20, 30, -5, -15, -25 };

    [Tooltip("Kutulardan çıkabilecek çarpma ve bölme değerleri listesi. Örn: 2 (x2), -2 (/2).")]
    public List<float> multiplicativeModifiers = new List<float> { 2f, -2f };
}