using Unity.VisualScripting;
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
    public LayerMask groundLayerMask; // im Inspector nur auf "Ground"-Layer setzen
    public float raycastHeight; // Startpunkt des Rays über der Welt

    void Start()
    {
        Vector3 randomSpawnPosition;

        Debug.Log($"Ground layer index: {LayerMask.NameToLayer("Ground")}");
        Debug.Log($"Ground mask from index: {1 << LayerMask.NameToLayer("Ground")}");
        Debug.Log($"Inspector mask value: {groundLayerMask.value}");

        if (!Physics.Raycast(new Vector3(0, 100, 0), Vector3.down, out var fixedHit, 200f, groundLayerMask, QueryTriggerInteraction.Collide))
        {
            Debug.Log("Fixed test failed too");
        }
        else
        {
            Debug.Log("Fixed test hit: " + fixedHit.collider.name);
        }

        for (int i = 0; i < maxPOIs; i++)
        {
            float x = Random.Range(-350f, 250f);
            float z = Random.Range(-300f, 300f);

            // Von oben nach unten Ray schießen
            Vector3 rayOrigin = new(x, raycastHeight, z);
            Debug.DrawRay(rayOrigin, Vector3.down * raycastHeight * 2f, Color.red, 2f);

            int groundMask = LayerMask.GetMask("Ground");
            Debug.Log($"Mask={groundMask} InspectorMask={groundLayerMask.value}");
            Debug.Log(QueryTriggerInteraction.Collide.ToString());

            if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit Testhit, raycastHeight * 2f, groundMask, QueryTriggerInteraction.Collide))
            {
                Debug.Log("Hit: " + Testhit.collider.name);
            }
            else
            {
                Debug.Log($"Miss at {rayOrigin}, xz inside plane? {x},{z}");
            }

            if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, raycastHeight * 2f, groundLayerMask))
            {
                randomSpawnPosition = hit.point;
                //Debug.Log("Trying to spawn Object Number: " + i);
                SpawnObject(randomSpawnPosition);
            }
            else
            {
                //Debug.Log("Couldn't hit LandscapeNumber: " + i + " at position: " + rayOrigin);
                //Debug.Log("Ist der Hit leer: " + hit.point);
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
        FixTreeBushTags(spawned, "Tree", k);
    }

    void SpawnBush(Vector3 spawnPoint)
    {
        int k = Random.Range(0, bushPrefabs.Length - 1);
        //Debug.Log("Trying to spawn Bush of Type: " + k);
        Quaternion randomRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
        GameObject spawned = Instantiate(bushPrefabs[k], spawnPoint, randomRotation, parent: landschaft.transform);
        FixTreeBushTags(spawned, "Busch", k);
    }

    void SpawnGrass(Vector3 spawnPoint)
    {
        int k = Random.Range(0, grassPrefabs.Length - 1);
        //Debug.Log("Trying to spawn Bush of Type: " + k);
        Quaternion randomRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
        GameObject spawned = Instantiate(grassPrefabs[k], spawnPoint, randomRotation, parent: landschaft.transform);
        FixGrassTags(spawned);
    }

    void SpawnStone(Vector3 spawnPoint)
    {
        int k = Random.Range(0, stonePrefabs.Length - 1);
        //Debug.Log("Trying to spawn Stone of Type: " + k);
        Quaternion randomRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
        GameObject spawned = Instantiate(stonePrefabs[k], spawnPoint, randomRotation, parent: landschaft.transform);
        FixStoneTags(spawned);
    }

    void FixTreeBushTags(GameObject spawnedObject, string type, int index)
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
    void FixGrassTags(GameObject spawnedObject)
    {
        string stoneTag = "Grass";

        foreach (Transform child in spawnedObject.GetComponentsInChildren<Transform>())
        {
            child.tag = stoneTag;
        }
    }

    void FixStoneTags(GameObject spawnedObject)
    {
        string stoneTag = "Rocks";

        foreach (Transform child in spawnedObject.GetComponentsInChildren<Transform>())
        {
            child.tag = stoneTag;
        }
    }
}