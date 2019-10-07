using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
	public GameObject enemyPrefab;
	[SerializeField] private float spawnTimer = 10;
	[SerializeField] private float spawnLength = 20;
	private float[,] spawnArea;
    void Start()
    {
	    StartCoroutine(SpawnEnemy());
    }

    public IEnumerator SpawnEnemy()
    {
	    while (true)
	    {
		    Instantiate(enemyPrefab, new Vector3(Random.Range(transform.position.x - spawnLength, transform.position.x + spawnLength), 0.5f,
				    Random.Range(transform.position.z - spawnLength, transform.position.z + spawnLength)), Quaternion.identity);
		    yield return new WaitForSeconds(spawnTimer);
        }
    }
}
