// Dosya Adı: MapCameraController.cs
using UnityEngine;
using UnityEngine.InputSystem;

public class MapCameraController : MonoBehaviour
{
    private Camera cam;
    private Vector3 lastPanPosition; // Parmağın/mouse'un son pozisyonunu saklamak için

    // --- SINIRLAMA İÇİN GEREKLİ DEĞİŞKENLER ---
    private float minY; // Kameranın inebileceği en alt seviye (başlangıç pozisyonu)

    [Header("Sınır Ayarları")]
    [Tooltip("Kameranın çıkabileceği maksimum Y pozisyonu. Haritadaki en üst kalenin Y pozisyonuna göre ayarlayın.")]
    [SerializeField] private float maxY = 50f; // Varsayılan olarak 50 birim yukarı.
    // ------------------------------------

    private void Awake()
    {
        cam = GetComponent<Camera>();

        // Oyun başladığında kameranın mevcut Y pozisyonunu "zemin" (minimum Y) olarak kaydet.
        minY = transform.position.y;
    }

    private void Update()
    {
        // Hareket etme mantığı burada çalışır.
        HandlePanning();
    }

    private void LateUpdate()
    {
        // Sınırlama mantığı burada çalışır.

        Vector3 currentPosition = transform.position;

        // Kameranın Y pozisyonunu, belirlediğimiz minimum (minY) ve maksimum (maxY)
        // sınırlar arasında kalacak şekilde zorla (Clamp).
        currentPosition.y = Mathf.Clamp(currentPosition.y, minY, maxY);

        // Güncellenmiş ve sınırlanmış pozisyonu kameraya geri ata.
        transform.position = currentPosition;
    }

    /// <summary>
    /// Yeni Input System'i kullanarak sürükleme hareketini algılar ve kamerayı kaydırır.
    /// </summary>
    private void HandlePanning()
    {
        if (Pointer.current == null) return;

        if (Pointer.current.press.wasPressedThisFrame)
        {
            lastPanPosition = cam.ScreenToWorldPoint(Pointer.current.position.ReadValue());
        }
        else if (Pointer.current.press.isPressed)
        {
            Vector3 newPanPosition = cam.ScreenToWorldPoint(Pointer.current.position.ReadValue());
            Vector3 offset = lastPanPosition - newPanPosition;

            // Hem yukarı hem de aşağı yönde serbestçe hareket et.
            // Sınırlama işini LateUpdate halledecek.
            transform.position += new Vector3(0, offset.y, 0);

            lastPanPosition = cam.ScreenToWorldPoint(Pointer.current.position.ReadValue());
        }
    }
}