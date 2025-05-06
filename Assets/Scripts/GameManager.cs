using UnityEngine;
using System.Collections; // IEnumerator için gerekli

public class GameManager : MonoBehaviour
{
    public GameObject enemyPrefab; // Inspector'dan Enemy Prefab'ını sürükle
    public Transform spawnPoint; // Inspector'dan EnemySpawner objesini sürükle
    public float minSpawnDelay = 1.0f; // Minimum spawn aralığı (Inspector'dan ayarlanabilir)
    public float maxSpawnDelay = 3.0f; // Maximum spawn aralığı (Inspector'dan ayarlanabilir)

    private bool isGameOver = false;
    private Coroutine spawnCoroutine; // Spawn işlemini kontrol etmek için

    void Start()
    {
        // Oyunu başlat ve düşman spawn etmeye başla
        StartSpawning();
    }

    void StartSpawning()
    {
        if (spawnCoroutine == null) // Zaten çalışmıyorsa başlat
        {
            isGameOver = false;
            spawnCoroutine = StartCoroutine(SpawnEnemyRoutine());
            Debug.Log("Düşman spawn işlemi başladı.");
        }
    }

    IEnumerator SpawnEnemyRoutine()
    {
        // Oyun bitmediği sürece devam et
        while (!isGameOver)
        {
            // Rastgele bir bekleme süresi belirle
            float waitTime = Random.Range(minSpawnDelay, maxSpawnDelay);
            yield return new WaitForSeconds(waitTime);

            // Oyun hala bitmediyse spawn et (beklerken bitmiş olabilir)
            if (!isGameOver)
            {
                SpawnEnemy();
            }
        }
        Debug.Log("Düşman spawn işlemi durdu.");
    }

    void SpawnEnemy()
    {
        if (enemyPrefab != null && spawnPoint != null)
        {
            Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);
            Debug.Log("Yeni Düşman Spawn Edildi!");
        }
        else
        {
            Debug.LogError("Enemy Prefab veya Spawn Point atanmamış!");
        }
    }

    public void GameOver()
    {
        if (!isGameOver) // Oyunun zaten bitip bitmediğini kontrol et
        {
            isGameOver = true;
            Debug.Log("GameManager: Oyun Bitti!");
            // Spawn işlemini durdur
            if (spawnCoroutine != null)
            {
                StopCoroutine(spawnCoroutine);
                spawnCoroutine = null; // Coroutine referansını temizle
            }

            // İsteğe bağlı: Sahnedeki tüm düşmanları durdur veya yok et
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
            foreach (GameObject enemy in enemies)
            {
                // Düşmanların hareketini durdurmak için script'i devre dışı bırakabilirsin
                EnemyController controller = enemy.GetComponent<EnemyController>();
                if (controller != null) controller.enabled = false;

                // Veya direkt yok et: Destroy(enemy);
            }

            // Game Over UI göster, skoru kaydet vs.
        }
    }
}