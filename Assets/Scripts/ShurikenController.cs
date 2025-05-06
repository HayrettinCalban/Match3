using UnityEngine;

public class ShurikenController : MonoBehaviour
{
    public float speed = 10f; // Shuriken hızı
    public float lifetime = 3f; // Ekranda kalma süresi (hedefi bulamazsa kendini yok etsin)
    private GameObject targetEnemy; // Hedef düşman

    void Start()
    {
        // En yakındaki düşmanı hedef al (veya başka bir hedefleme mantığı kurabilirsin)
        FindClosestEnemy();

        // Belirli bir süre sonra shuriken'i yok et (ekranda kaybolmazsa)
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        if (targetEnemy != null && targetEnemy.activeSelf) // Hedef hala varsa ve aktifse
        {
            // Hedefe doğru yönel ve ilerle
            Vector2 direction = (targetEnemy.transform.position - transform.position).normalized;
            transform.position += (Vector3)direction * speed * Time.deltaTime; // Rigidbody ile hareket daha iyi olabilir

            // İsteğe bağlı: Shuriken'in hedefe doğru dönmesini sağla
            // float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            // transform.rotation = Quaternion.Euler(0, 0, angle);
        }
        else
        {
            // Hedef yoksa veya yok olduysa düz ilerle (veya en yakın yeni hedefi bul)
            transform.position += transform.right * speed * Time.deltaTime; // Fırlatıldığı yönde gider (Eğer başlangıç rotasyonu ayarlıysa)
            // Ya da yeni hedef ara: FindClosestEnemy();
        }
    }

    void FindClosestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        float closestDistance = Mathf.Infinity;
        GameObject nearestEnemy = null;

        foreach (GameObject enemy in enemies)
        {
            float distanceToEnemy = Vector2.Distance(transform.position, enemy.transform.position);
            if (distanceToEnemy < closestDistance)
            {
                closestDistance = distanceToEnemy;
                nearestEnemy = enemy;
            }
        }

        if (nearestEnemy != null)
        {
            targetEnemy = nearestEnemy;
            Debug.Log("Hedef Düşman Bulundu: " + targetEnemy.name);
        }
        else
        {
            Debug.Log("Yakında aktif düşman bulunamadı.");
        }
    }

    // Çarpışma için Enemy script'indeki OnTriggerEnter2D kullanılacak.
    // Bu yüzden Shuriken'in Collider'ı Trigger olmalı.
}