using UnityEngine;

public class VegetationSpawner : MonoBehaviour
{

    public GameObject treePrefab;
    public GameObject stonePrefab;
    public GameObject landschaft;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Vector3 randomSpawnPosition;
        for (int i = 0; i < 100; i++) {
            randomSpawnPosition = new Vector3(Random.Range(-5f, 21f), 0, Random.Range(-5f, 21f));
            int j = Random.Range(1, 5);
            if (j == 1)
                Instantiate(stonePrefab, randomSpawnPosition, Quaternion.identity, parent: landschaft.transform);
            else
                Instantiate(treePrefab, randomSpawnPosition, Quaternion.identity, parent: landschaft.transform);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}