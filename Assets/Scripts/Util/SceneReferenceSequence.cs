using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneReferenceSequence : MonoBehaviour
{
    [SerializeField]
    bool loadFirstImmediate;

    [SerializeField]
    SceneReference[] levels;

    private int currentLevel;

    // Start is called before the first frame update
    void Start()
    {
        if (loadFirstImmediate && levels.Length > 0)
        {
            SceneManager.LoadSceneAsync(levels[currentLevel].ScenePath, LoadSceneMode.Additive);
        }
    }

    public SceneReference PeekNext()
    {
        if (currentLevel >= levels.Length - 1)
            return default;

        return levels[currentLevel + 1];
    }

    [ContextMenu("Load Next")]
    public void LoadNextAsync()
    {
        if(currentLevel >= levels.Length - 1)
            return;

        //Unload current level

        var unloadOp = SceneManager.UnloadSceneAsync(levels[currentLevel].ScenePath);

        unloadOp.completed += (op) =>
        {
            currentLevel += 1;
            SceneManager.LoadSceneAsync(levels[currentLevel].ScenePath, LoadSceneMode.Additive);
        };

    }


}
