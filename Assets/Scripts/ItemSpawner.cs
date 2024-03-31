using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    static int ItemCount;

    public bool SpawnOnAwake = true;

    [SerializeField]
    GameObject[] itemPrefabs;

    [SerializeField]
    Vector2 bounds;

    [SerializeField]
    [Min(1)]
    int spawnCount = 5;

    private AudioSource _audio;

    [Header("SFX")]
    public AudioClip sfx_spawn;

    private void Start()
    {
        _audio = AudioService.AudioSource;
        SpawnRandom(5);    
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireCube(transform.position, new Vector3(bounds.x, bounds.y, 0));
    }


    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(250, 0, 300, 500));

        if (GUILayout.Button("Spawn Item"))
        {
            SpawnRandom(spawnCount);
        }

        GUILayout.EndArea();
    }

    private void OnItemDestroyed()
    {
        ItemCount--;

        if(ItemCount <= 0 && gameObject != null)
        {
            SpawnRandom(5);
        }
    }

    public GameObject SpawnRandom()
    {
        var xPos = Random.Range(bounds.x * -1, bounds.x) + transform.position.x;
        var yPos = Random.Range(bounds.y * -1, bounds.y) + transform.position.y;

        var randIndex = Random.Range(0, itemPrefabs.Length);

        var obj = Instantiate(itemPrefabs[randIndex], new Vector3(xPos, yPos, 0), Quaternion.identity);
        obj.GetComponent<Item>().Disabled += OnItemDestroyed;
        ItemCount++;
        return obj;
    }

    public GameObject[] SpawnRandom(int numToSpawn)
    {
        GameObject[] items = new GameObject[numToSpawn];
        for(int i = 0; i < numToSpawn; i++)
        {
            items[i] = SpawnRandom();
        }

        _audio.PlayOneShot(sfx_spawn);
        return items;
    }
}
