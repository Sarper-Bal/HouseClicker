// Dosya Adı: UIButtonSound.cs
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UIButtonSound : MonoBehaviour
{
    // --- YENİ EKLENEN KISIM ---
    [Header("Özel Ses Ayarı")]
    [Tooltip("Eğer bu butona özel bir ses atamak isterseniz, ses klibini buraya sürükleyin. Boş bırakırsanız SoundManager'daki varsayılan sesi çalar.")]
    [SerializeField] private AudioClip overrideSound;
    // -------------------------

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    private void Start()
    {
        button.onClick.AddListener(PlayClickSound);
    }

    private void OnDestroy()
    {
        button.onClick.RemoveListener(PlayClickSound);
    }

    private void PlayClickSound()
    {
        // Eğer SoundManager bulunamadıysa hiçbir şey yapma.
        if (SoundManager.Instance == null) return;

        // --- GÜNCELLENMİŞ MANTIK ---
        // 1. Özel bir ses (overrideSound) atanmış mı?
        if (overrideSound != null)
        {
            // Evet, atanmış. O zaman bu özel sesi çal.
            SoundManager.Instance.PlaySound(overrideSound);
        }
        else
        {
            // Hayır, atanmamış. O zaman SoundManager'daki varsayılan sesi çal.
            SoundManager.Instance.PlaySound(SoundManager.Instance.buttonClickSound);
        }
        // -------------------------
    }
}