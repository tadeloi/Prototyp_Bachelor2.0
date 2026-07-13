using UnityEngine;

public class VegetationSpawner : MonoBehaviour
{
    public GameObject[] treePrefabs;
    public GameObject[] stonePrefabs;
    public GameObject[] bushPrefabs;
    public GameObject[] grassPrefabs;
    public GameObject landschaft;
    public int maxPOIs = 1000;

    [Header("Raycast Settings")]
    public LayerMask groundLayerMask; // im Inspector nur auf "Landscape"-Layer setzen
    public float raycastHeight = 100f; // Startpunkt des Rays über der Welt

    void Start()
    {
        Vector3 randomSpawnPosition;

        for (int i = 0; i < maxPOIs; i++)
        {
            float x = Random.Range(-350f, 250f);
            float z = Random.Range(-300f, 300f);

            // Von oben nach unten Ray schießen
            Vector3 rayOrigin = new Vector3(x, raycastHeight, z);
            if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, raycastHeight * 2f, groundLayerMask))
            {
                randomSpawnPosition = hit.point;
                Debug.Log("Trying to spawn Object");
                SpawnObject(randomSpawnPosition);
            }
            else
            {
                Debug.Log("Couldn't hit Landscape");
                // Fallback, falls kein Treffer (z.B. außerhalb des Landschafts-Mesh)
            }
        }
    }

    void SpawnObject(Vector3 spawnPoint)
    {
        int j = Random.Range(1, 20);

        if (j <= 5)
            SpawnStone(spawnPoint);
        else if (j > 5 && j <= 10)
            SpawnBush(spawnPoint);
        else if (j > 10 && j <= 15)
            SpawnGrass(spawnPoint);
        else if (j > 15)
            SpawnTree(spawnPoint);
    }

    void SpawnTree(Vector3 spawnPoint)
    {
        int k = Random.Range(0, treePrefabs.Length - 1);
        //Debug.Log("Trying to spawn Tree of Type: " + k);
        Quaternion randomRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
        spawnPoint.y -= 1.5f;
        GameObject spawned = Instantiate(treePrefabs[k], spawnPoint, randomRotation, parent: landschaft.transform);
        FixTags(spawned, "Tree", k);
    }

    void SpawnBush(Vector3 spawnPoint)
    {
        int k = Random.Range(0, bushPrefabs.Length - 1);
        //Debug.Log("Trying to spawn Bush of Type: " + k);
        Quaternion randomRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
        GameObject spawned = Instantiate(bushPrefabs[k], spawnPoint, randomRotation, parent: landschaft.transform);
        FixTags(spawned, "Busch", k);
    }

    void SpawnGrass(Vector3 spawnPoint)
    {
        int k = Random.Range(0, grassPrefabs.Length - 1);
        //Debug.Log("Trying to spawn Bush of Type: " + k);
        Quaternion randomRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
        Instantiate(grassPrefabs[k], spawnPoint, randomRotation, parent: landschaft.transform);
    }

    void SpawnStone(Vector3 spawnPoint)
    {
        int k = Random.Range(0, stonePrefabs.Length - 1);
        //Debug.Log("Trying to spawn Stone of Type: " + k);
        Quaternion randomRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
        Instantiate(stonePrefabs[k], spawnPoint, randomRotation, parent: landschaft.transform);
    }

    void FixTags(GameObject spawnedObject, string type, int index)
    {
        string stumpTag = $"{type}{index + 1}Stamm";
        string leavesTag = $"{type}{index + 1}Leaves";

        foreach (Transform child in spawnedObject.GetComponentsInChildren<Transform>())
        {
            if (child.name.StartsWith("Cube"))
                child.tag = stumpTag;
            else if (child.name.StartsWith("Sphere"))
                child.tag = leavesTag;
        }
    }
}