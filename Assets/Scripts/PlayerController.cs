using UnityEngine;
// using UnityEngine.SceneManagement; // Eğer RestartGame kullanılmıyorsa bu satır silinebilir

public class PlayerController : MonoBehaviour
{
    public GameObject shurikenPrefab; // Inspector'dan Shuriken Prefab'ını sürükle
    public Transform firePoint; // Shuriken'in çıkacağı nokta (Player'ın önünde boş bir child obje olabilir)
    public GameManager gameManager; // Inspector'dan GameManager objesini sürükle (opsiyonel, başka işlevler için kalabilir)

    void Start()
    {
        // Başlangıçta GameManager'ı bulmaya çalış (eğer Inspector'dan atanmadıysa)
        if (gameManager == null)
        {
            gameManager = FindFirstObjectByType<GameManager>();
            if (gameManager == null)
            {
                Debug.LogWarning("PlayerController, GameManager nesnesini bulamadı.");
            }
        }
    }

    // Bu fonksiyon BoardManager tarafından çağrılacak
    public void ShootShuriken()
    {
        if (shurikenPrefab != null && firePoint != null)
        {
            Instantiate(shurikenPrefab, firePoint.position, firePoint.rotation);
            Debug.Log("Shuriken Fırlatıldı! (Taş Patlatma Tetikledi)"); // Log mesajını güncelledik
        }
        else
        {
            Debug.LogError("Shuriken Prefab veya Fire Point atanmamış! Ateşleme yapılamıyor.");
        }
    }

    // Update fonksiyonunu sildik çünkü artık Space tuşu ile ateş etmiyoruz.

    /* // Eğer RestartGame fonksiyonu kullanılmayacaksa bu da silinebilir
    void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); // Mevcut sahneyi yeniden yükle
    }
    */
}