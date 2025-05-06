using UnityEngine;
using System.Collections;

public class EnemyController : MonoBehaviour
{
    public float moveSpeed = 2f; // Düşmanın hareket hızı

    // --- YENİ EKLENEN CAN DEĞİŞKENLERİ ---
    public int maxHealth = 25; // Düşmanın maksimum canı (Inspector'dan ayarlanabilir)
    private int currentHealth; // Düşmanın mevcut canı
    // --- ---

    private Transform playerTransform; // Oyuncunun pozisyonunu takip etmek için

    void Start()
    {
        // --- CANI BAŞLANGIÇTA AYARLA ---
        currentHealth = maxHealth;
        // --- ---

        // Oyuncuyu etiketiyle ("Player") bul
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            playerTransform = playerObject.transform;
        }
        else
        {
            Debug.LogError("Sahnedeki Player nesnesinin etiketi 'Player' olarak ayarlanmamış!");
        }
    }

    void Update()
    {
        if (playerTransform != null && playerTransform.gameObject.activeSelf)
        {
            Vector2 direction = (playerTransform.position - transform.position).normalized;
            transform.position = Vector2.MoveTowards(transform.position, playerTransform.position, moveSpeed * Time.deltaTime);
        }

        if (transform.position.x < -2f)
        {
            Time.timeScale = 0f;
            // Burada Game Over ekranı açılabilir
            StartCoroutine(RestartSceneAfterDelay(2f));
        }
    }

    // Shuriken ile çarpışma kontrolü (Shuriken'in Collider'ı Trigger olmalı)
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Shuriken"))
        {
            // --- ARTIK DOĞRUDAN ÖLMEYECEK, HASAR ALACAK ---
            TakeDamage(1); // Her shuriken 1 hasar versin
            // --- ---
            Destroy(other.gameObject); // Çarpan shuriken'i yok et
        }
    }

    // Player ile çarpışma kontrolü
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // Player kendi script'inde ölecek
        }
    }

    // --- YENİ HASAR ALMA FONKSİYONU ---
    public void TakeDamage(int damageAmount)
    {
        currentHealth -= damageAmount; // Canı azalt
        Debug.Log(gameObject.name + " Kalan Can: " + currentHealth); // Konsola kalan canı yazdır (test için)

        // İsteğe bağlı: Hasar alma efekti (renk değiştirme vb.) burada eklenebilir

        if (currentHealth <= 0) // Can 0 veya altına düştüyse
        {
            Die(); // Ölme fonksiyonunu çağır
        }
    }
    // --- ---

    void Die()
    {
        Debug.Log("Düşman Yok Edildi! (" + gameObject.name + ")");
        // Burada yok olma efekti, ses vs. eklenebilir
        Destroy(gameObject); // Düşman nesnesini yok et
    }

    // Coroutine ekle:
    IEnumerator RestartSceneAfterDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }
}