// Dosya Adı: SoundManager.cs
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    // Bu, script'e her yerden kolayca erişmemizi sağlayan Singleton yapısıdır.
    public static SoundManager Instance { get; private set; }

    [Header("Ses Kaynakları")]
    [Tooltip("Kısa ses efektleri (tıklama, vuruş vb.) için kullanılacak hoparlör.")]
    [SerializeField] private AudioSource sfxSource;
    [Tooltip("Arka plan müziği için kullanılacak hoparlör (şimdilik boş kalabilir).")]
    [SerializeField] private AudioSource musicSource;

    [Header("UI Sesleri")]
    public AudioClip buttonClickSound;

    private void Awake()
    {
        // Eğer sahnede zaten bir SoundManager varsa, bu yenisini yok et.
        // Bu, sahneler arası geçişte birden fazla yönetici oluşmasını engeller.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            Instance = this;
        }

        // Bu objenin, sahneler arasında geçiş yaparken yok olmamasını sağla.
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Verilen ses klibini, efekt hoparlöründen çalar.
    /// </summary>
    /// <param name="clip">Çalınacak ses dosyası (AudioClip).</param>
    public void PlaySound(AudioClip clip)
    {
        if (clip == null) return;

        // PlayOneShot, mevcut sesi kesmeden yeni bir ses çalmamızı sağlar.
        // Butonlara art arda basıldığında seslerin düzgün çıkması için mükemmeldir.
        sfxSource.PlayOneShot(clip);
    }
}