using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class BoardManager : MonoBehaviour
{
    [Header("Board Ayarları")]
    public int width = 8;
    public int height = 8;
    public float spacing = 0.1f;

    [Header("Prefabs")]
    public GameObject[] tilePrefabs;

    [Header("Oyun Mekanikleri")]
    public float swapDuration = 0.2f;
    public float destroyDelay = 0.1f;
    public float fallDuration = 0.3f;

    [Header("Shooting Mekanikleri")] // <-- YENİ HEADER
    public PlayerController playerController; // Inspector'dan Player objesini sürükleyin
    public int tilesToPopForShot = 10; // Kaç taş patlatınca ateş edilecek?
    private int poppedTileCount = 0; // Patlatılan taş sayacı

    private Tile[,] allTiles;
    private Tile selectedTile = null;
    private bool isProcessingMove = false;

    public bool IsProcessingMove()
    {
        return isProcessingMove;
    }

    void Start()
    {
        // PlayerController referansını kontrol et (Inspector'dan atanmadıysa bulmayı dene)
        if (playerController == null)
        {
            playerController = FindFirstObjectByType<PlayerController>();
            if (playerController == null)
            {
                Debug.LogError("BoardManager PlayerController bulamadı! Shuriken atma mekaniği çalışmayacak.");
            }
        }

        if (tilePrefabs == null || tilePrefabs.Length == 0)
        {
            Debug.LogError("BoardManager'da 'Tile Prefabs' dizisi atanmamış veya boş! Oyun başlayamaz.");
            return;
        }
        allTiles = new Tile[width, height];
        SetupBoard();
    }

    // --- Taş Yok Etme ve Sayaç Güncelleme Yardımcı Metodu ---
    private void DestroyTileAndCount(Tile tile)
    {
        if (tile != null && allTiles[tile.xIndex, tile.yIndex] == tile) // Hâlâ tahtada ve doğru tile mı?
        {
            allTiles[tile.xIndex, tile.yIndex] = null; // Tahtadan kaldır
            Destroy(tile.gameObject, destroyDelay);   // Objesini yok et (gecikmeli)

            // Sayacı artır ve ateş etme koşulunu kontrol et
            poppedTileCount++;
            // Debug.Log($"Patlayan Taş Sayısı: {poppedTileCount}"); // Test için log
            CheckForShoot();
        }
    }

    // --- Ateş Etme Koşulunu Kontrol Eden Yardımcı Metot ---
    private void CheckForShoot()
    {
        // PlayerController varsa ve yeterli taş patlatıldıysa
        if (playerController != null && poppedTileCount >= tilesToPopForShot)
        {
            Debug.Log($"{poppedTileCount}/{tilesToPopForShot} taş patladı. Shuriken ateşleniyor!");
            playerController.ShootShuriken(); // Oyuncuya ateş etmesini söyle
            poppedTileCount -= tilesToPopForShot; // Sayacı sıfırla veya eksilt (taşma payını korumak için)
            // poppedTileCount = 0; // Eğer tam sıfırlanması isteniyorsa
        }
    }
    // --- ---

    void SetupBoard()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                CreateTile(x, y, true); // Başlangıçta eşleşme kontrolü yaparak oluştur
            }
        }
        // Başlangıçta oluşabilecek eşleşmeleri temizle (Bu kısım sayaç artırmamalı)
        StartCoroutine(CheckAndClearAllMatches(true)); // İlk temizlik için flag ekle
    }

    void CreateTile(int x, int y, bool checkInitial = true)
    {
        if (allTiles[x, y] != null) return;

        int randomIndex = Random.Range(0, tilePrefabs.Length);
        GameObject selectedPrefab = tilePrefabs[randomIndex];

        if (checkInitial)
        {
            Tile potentialTileComponent = selectedPrefab.GetComponent<Tile>();
            if (potentialTileComponent == null) { Debug.LogError($"..."); return; } // Hata kontrolü
            int potentialType = potentialTileComponent.tileType;

            int loopGuard = 0;
            while (CheckInitialMatch(x, y, potentialType) && loopGuard < tilePrefabs.Length * 2)
            {
                randomIndex = Random.Range(0, tilePrefabs.Length);
                selectedPrefab = tilePrefabs[randomIndex];
                potentialTileComponent = selectedPrefab.GetComponent<Tile>();
                if (potentialTileComponent == null) { Debug.LogError($"..."); return; } // Hata kontrolü
                potentialType = potentialTileComponent.tileType;
                loopGuard++;
            }
            if (loopGuard >= tilePrefabs.Length * 2) Debug.LogWarning($"...");
        }


        Vector2 position = GetWorldPosition(x, y);
        GameObject newTileObject = Instantiate(selectedPrefab, position, Quaternion.identity, transform);
        newTileObject.name = $"Tile_{selectedPrefab.name} ({x},{y})";

        Tile newTile = newTileObject.GetComponent<Tile>();
        if (newTile == null) { Debug.LogError($"..."); Destroy(newTileObject); return; } // Hata kontrolü

        newTile.Initialize(x, y, this);
        allTiles[x, y] = newTile;
    }


    bool CheckInitialMatch(int x, int y, int type)
    {
        // Sol 2'li kontrol
        if (x >= 2 && allTiles[x - 1, y] != null && allTiles[x - 2, y] != null)
        {
            if (allTiles[x - 1, y].tileType == type && allTiles[x - 2, y].tileType == type) return true;
        }
        // Alt 2'li kontrol
        if (y >= 2 && allTiles[x, y - 1] != null && allTiles[x, y - 2] != null)
        {
            if (allTiles[x, y - 1].tileType == type && allTiles[x, y - 2].tileType == type) return true;
        }
        return false;
    }


    Vector2 GetWorldPosition(int x, int y)
    {
        float tileWidth = 1.0f;
        float tileHeight = 1.0f;
        // Prefab varsa boyutunu al, yoksa varsayılanı kullan
        if (tilePrefabs.Length > 0 && tilePrefabs[0] != null)
        {
            // SpriteRenderer'dan boyut almayı dene
            SpriteRenderer sr = tilePrefabs[0].GetComponent<SpriteRenderer>();
            if (sr != null && sr.sprite != null)
            {
                tileWidth = sr.bounds.size.x;
                tileHeight = sr.bounds.size.y;
            }
            // Olmazsa Transform scale kullan
            else
            {
                tileWidth = tilePrefabs[0].transform.localScale.x;
                tileHeight = tilePrefabs[0].transform.localScale.y;
            }
        }

        float startX = -(width / 2.0f) * (tileWidth + spacing) + (tileWidth / 2.0f);
        float startY = -(height / 2.0f) * (tileHeight + spacing) + (tileHeight / 2.0f);

        return new Vector2(startX + x * (tileWidth + spacing),
                           startY + y * (tileHeight + spacing));
    }

    public void TileClicked(Tile clickedTile)
    {
        if (isProcessingMove) return; // İşlem devam ediyorsa tıklamayı engelle

        if (selectedTile == null) // İlk tıklama
        {
            selectedTile = clickedTile;
            selectedTile.transform.localScale *= 1.1f; // Seçildiğini belirt
        }
        else // İkinci tıklama
        {
            selectedTile.transform.localScale /= 1.1f; // Eski seçimi normale döndür

            if (selectedTile == clickedTile) // Aynı taşa tekrar tıklandı
            {
                selectedTile = null; // Seçimi iptal et
            }
            else if (IsAdjacent(selectedTile, clickedTile)) // Bitişik taş mı?
            {
                // Değiştirme ve kontrol işlemini başlat
                StartCoroutine(SwapAndProcess(selectedTile, clickedTile));
                selectedTile = null; // Seçimi temizle
            }
            else // Bitişik değilse, yeni taşı seç
            {
                selectedTile = clickedTile;
                selectedTile.transform.localScale *= 1.1f; // Yeni seçimi belirt
            }
        }
    }

    IEnumerator FillEmptySpaces()
    {
        List<Coroutine> fallingCoroutines = new List<Coroutine>();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (allTiles[x, y] == null) // Boşluk varsa
                {
                    int randomIndex = Random.Range(0, tilePrefabs.Length);
                    GameObject selectedPrefab = tilePrefabs[randomIndex];

                    // Ekranın üstünden başlat
                    Vector2 startPosition = GetWorldPosition(x, height); // y=height ekranın üstü gibi düşün
                    Vector2 targetPosition = GetWorldPosition(x, y);

                    GameObject newTileObject = Instantiate(selectedPrefab, startPosition, Quaternion.identity, transform);
                    newTileObject.name = $"Tile_{selectedPrefab.name} ({x},{y})_New";

                    Tile newTile = newTileObject.GetComponent<Tile>();
                    if (newTile == null) { Debug.LogError($"..."); Destroy(newTileObject); continue; } // Hata kontrolü

                    newTile.Initialize(x, y, this);
                    allTiles[x, y] = newTile; // Tahtaya yerleştir

                    // Düşme animasyonunu başlat ve listeye ekle
                    fallingCoroutines.Add(StartCoroutine(newTile.MoveToPosition(targetPosition, fallDuration)));
                }
            }
        }

        // Tüm düşme animasyonlarının bitmesini bekle
        foreach (var coroutine in fallingCoroutines)
        {
            yield return coroutine;
        }
        yield return new WaitForSeconds(0.05f); // Küçük bir bekleme
    }


    bool IsAdjacent(Tile tile1, Tile tile2)
    {
        // Manhattan mesafesi 1 ise bitişiktir
        return Mathf.Abs(tile1.xIndex - tile2.xIndex) + Mathf.Abs(tile1.yIndex - tile2.yIndex) == 1;
    }

    IEnumerator SwapAndProcess(Tile tile1, Tile tile2)
    {
        isProcessingMove = true; // İşlemi başlat, diğer tıklamaları engelle

        yield return StartCoroutine(SwapTilesAnimation(tile1, tile2)); // Animasyonlu değiştirme

        // Değişim sonrası her iki taşın konumunda eşleşme var mı kontrol et
        List<Tile> matches1 = FindMatchesAt(tile1.xIndex, tile1.yIndex);
        List<Tile> matches2 = FindMatchesAt(tile2.xIndex, tile2.yIndex);

        // İki listedeki eşleşmeleri birleştir (tekrarları kaldır)
        List<Tile> allMatches = GetUniqueMatches(matches1.Concat(matches2).ToList());

        if (allMatches.Count > 0) // Eşleşme bulunduysa
        {
            // Eşleşmeleri temizle, sayacı artır ve tahtayı yeniden doldur
            yield return StartCoroutine(ClearAndRefillBoard(allMatches));
        }
        else // Eşleşme yoksa
        {
            yield return new WaitForSeconds(0.1f); // Kısa bekleme
            // Taşları geri değiştir (animasyonlu)
            yield return StartCoroutine(SwapTilesAnimation(tile1, tile2));
        }

        isProcessingMove = false; // İşlemi bitir
    }

    IEnumerator SwapTilesAnimation(Tile tile1, Tile tile2)
    {
        // Pozisyonları al
        Vector2 pos1 = GetWorldPosition(tile1.xIndex, tile1.yIndex);
        Vector2 pos2 = GetWorldPosition(tile2.xIndex, tile2.yIndex);

        // Tahtadaki referansları ve taşların indexlerini değiştir
        int tempX = tile1.xIndex;
        int tempY = tile1.yIndex;

        allTiles[tile1.xIndex, tile1.yIndex] = tile2;
        allTiles[tile2.xIndex, tile2.yIndex] = tile1;

        tile1.xIndex = tile2.xIndex;
        tile1.yIndex = tile2.yIndex;
        tile2.xIndex = tempX;
        tile2.yIndex = tempY;

        // Hareket animasyonlarını başlat
        Coroutine move1 = StartCoroutine(tile1.MoveToPosition(GetWorldPosition(tile1.xIndex, tile1.yIndex), swapDuration));
        Coroutine move2 = StartCoroutine(tile2.MoveToPosition(GetWorldPosition(tile2.xIndex, tile2.yIndex), swapDuration));

        // Animasyonların bitmesini bekle
        yield return move1;
        yield return move2;
    }

    List<Tile> FindAllMatches()
    {
        List<Tile> combinedMatches = new List<Tile>();
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (allTiles[x, y] != null)
                {
                    // Her taş için o noktadaki eşleşmeleri bul ve listeye ekle
                    combinedMatches.AddRange(FindMatchesAt(x, y));
                }
            }
        }
        // Tüm bulunan eşleşmelerdeki tekrarları kaldır
        return GetUniqueMatches(combinedMatches);
    }

    List<Tile> FindMatchesAt(int x, int y)
    {
        List<Tile> matches = new List<Tile>();
        if (x < 0 || x >= width || y < 0 || y >= height || allTiles[x, y] == null) return matches; // Geçersiz konum

        Tile currentTile = allTiles[x, y];
        int currentType = currentTile.tileType;

        // Yatay eşleşmeleri kontrol et
        List<Tile> horizontalMatches = new List<Tile> { currentTile };
        // Sağa doğru
        for (int i = x + 1; i < width; i++) { if (allTiles[i, y] != null && allTiles[i, y].tileType == currentType) horizontalMatches.Add(allTiles[i, y]); else break; }
        // Sola doğru
        for (int i = x - 1; i >= 0; i--) { if (allTiles[i, y] != null && allTiles[i, y].tileType == currentType) horizontalMatches.Add(allTiles[i, y]); else break; }
        if (horizontalMatches.Count >= 3) matches.AddRange(horizontalMatches); // 3 veya daha fazla ise ekle

        // Dikey eşleşmeleri kontrol et
        List<Tile> verticalMatches = new List<Tile> { currentTile };
        // Yukarı doğru
        for (int i = y + 1; i < height; i++) { if (allTiles[x, i] != null && allTiles[x, i].tileType == currentType) verticalMatches.Add(allTiles[x, i]); else break; }
        // Aşağı doğru
        for (int i = y - 1; i >= 0; i--) { if (allTiles[x, i] != null && allTiles[x, i].tileType == currentType) verticalMatches.Add(allTiles[x, i]); else break; }
        if (verticalMatches.Count >= 3) matches.AddRange(verticalMatches); // 3 veya daha fazla ise ekle

        // Tekrarları kaldırarak döndür (bir taş hem yatay hem dikey eşleşmede olabilir)
        return GetUniqueMatches(matches);
    }


    // Bu metot ilk takas sonrası çağrılır
    IEnumerator ClearAndRefillBoard(List<Tile> matches)
    {
        // İlk eşleşen taşları yok et ve sayacı artır
        foreach (Tile tile in matches)
        {
            DestroyTileAndCount(tile); // Yardımcı metodu kullan
        }
        yield return new WaitForSeconds(destroyDelay + 0.05f); // Yok olma animasyonunu bekle

        // Zincirleme reaksiyonları kontrol et
        yield return StartCoroutine(CollapseColumns());      // Üstteki taşları düşür
        yield return StartCoroutine(FillEmptySpaces());       // Boşlukları doldur
        yield return StartCoroutine(CheckAndClearAllMatches());// Yeni eşleşmeleri kontrol et
    }


    List<Tile> GetUniqueMatches(List<Tile> matches)
    {
        // Null olmayan ve benzersiz taşları içeren yeni bir liste döndürür
        return matches.Where(t => t != null).Distinct().ToList();
    }


    IEnumerator CollapseColumns()
    {
        List<Coroutine> fallingCoroutines = new List<Coroutine>();
        // Her sütun için
        for (int x = 0; x < width; x++)
        {
            int emptySpaces = 0; // Alttaki boşluk sayısı
            // Sütunu aşağıdan yukarıya tara
            for (int y = 0; y < height; y++)
            {
                if (allTiles[x, y] == null) // Boşluk bulursan sayacı artır
                {
                    emptySpaces++;
                }
                else if (emptySpaces > 0) // Taş bulursan ve altında boşluk varsa
                {
                    Tile tileToMove = allTiles[x, y];
                    int targetY = y - emptySpaces; // Düşeceği hedef Y koordinatı

                    // Tahtadaki referansları güncelle
                    allTiles[x, targetY] = tileToMove;
                    allTiles[x, y] = null; // Eski yerini boşalt

                    // Taşın kendi indexini güncelle
                    tileToMove.yIndex = targetY;

                    // Düşme animasyonunu başlat
                    Vector2 targetPosition = GetWorldPosition(x, targetY);
                    fallingCoroutines.Add(StartCoroutine(tileToMove.MoveToPosition(targetPosition, fallDuration)));
                    // İsmini güncelle (isteğe bağlı, debug için faydalı)
                    string baseName = tileToMove.gameObject.name.Split('_')[1].Split(' ')[0];
                    tileToMove.name = $"Tile_{baseName} ({x},{targetY})";
                }
            }
        }
        // Tüm düşme animasyonlarının bitmesini bekle
        foreach (var coroutine in fallingCoroutines) { yield return coroutine; }
        yield return new WaitForSeconds(0.05f); // Küçük bekleme
    }

    // Zincirleme reaksiyonları ve başlangıç temizliğini kontrol eder
    IEnumerator CheckAndClearAllMatches(bool isInitialClear = false)
    {
        isProcessingMove = true; // İşlemi kilitle
        List<Tile> currentMatches = FindAllMatches(); // Mevcut tüm eşleşmeleri bul
        int loopGuard = 0;
        int maxLoops = width * height; // Sonsuz döngü koruması

        // Eşleşme olduğu sürece devam et
        while (currentMatches.Count > 0 && loopGuard < maxLoops)
        {
            loopGuard++;

            // Eşleşen taşları yok et (Eğer başlangıç temizliği değilse sayacı artır)
            yield return StartCoroutine(ClearAndRefillBoardInternal(currentMatches, !isInitialClear));

            // Yeni eşleşmeleri bul
            currentMatches = FindAllMatches();
            yield return new WaitForSeconds(0.1f); // Döngüler arası küçük bekleme
        }

        if (loopGuard >= maxLoops)
        {
            Debug.LogError("CheckAndClearAllMatches sonsuz döngüye girmiş olabilir!");
        }
        isProcessingMove = false; // İşlemi aç
    }

    // CheckAndClearAllMatches tarafından çağrılan iç metot
    IEnumerator ClearAndRefillBoardInternal(List<Tile> matches, bool countPops)
    {
        // Taşları yok et (ve gerekirse say)
        foreach (Tile tile in matches)
        {
            if (countPops)
            {
                DestroyTileAndCount(tile); // Sayacı artıran metot
            }
            else
            {
                // Sayacı artırmadan sadece yok et (başlangıç temizliği için)
                if (tile != null && allTiles[tile.xIndex, tile.yIndex] == tile)
                {
                    allTiles[tile.xIndex, tile.yIndex] = null;
                    Destroy(tile.gameObject, destroyDelay);
                }
            }
        }
        yield return new WaitForSeconds(destroyDelay + 0.05f); // Yok olma animasyonunu bekle

        // Tahtayı düzenle
        yield return StartCoroutine(CollapseColumns());
        yield return StartCoroutine(FillEmptySpaces());
        // Burada tekrar CheckAndClearAllMatches çağırmaya gerek YOK, çünkü döngü zaten orada.
    }

}