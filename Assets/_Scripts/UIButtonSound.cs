// Dosya Adı: UIButtonSound.cs
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))] // Bu script'in sadece butonların üzerine eklenebilmesini sağlar.
public class UIButtonSound : MonoBehaviour
{
    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    private void Start()
    {
        // Butonun tıklama olayına (onClick) bir dinleyici ekle.
        // Artık bu butona her tıklandığında, PlayClickSound metodu çalışacak.
        button.onClick.AddListener(PlayClickSound);
    }

    private void OnDestroy()
    {
        // Obje yok edilirken dinleyiciyi kaldırmak iyi bir pratiktir.
        button.onClick.RemoveListener(PlayClickSound);
    }

    private void PlayClickSound()
    {
        // SoundManager'a git ve genel tıklama sesini çalmasını söyle.
        if (SoundManager.Instance != null && SoundManager.Instance.buttonClickSound != null)
        {
            SoundManager.Instance.PlaySound(SoundManager.Instance.buttonClickSound);
        }
    }
}