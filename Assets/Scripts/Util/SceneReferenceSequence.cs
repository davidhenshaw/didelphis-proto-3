using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneReferenceSequence : MonoBehaviour
{
    public const string LEVEL_PREFIX = "Lvl";

    [SerializeField]
    bool loadFirstImmediate;

    [SerializeField]
    SceneReference[] levels;

    private int currentLevel;

    // Start is called before the first frame update
    void Start()
    {
        if (IsLevelLoaded())
            return;

        if (loadFirstImmediate && levels.Length > 0)
        {
            SceneManager.LoadSceneAsync(levels[currentLevel].ScenePath, LoadSceneMode.Additive);
        }
    }

    bool IsLevelLoaded()
    {
        var index = 0;
        var scene = SceneManager.GetSceneAt(index);
        while(scene.IsValid())
        {
            if (scene.name.StartsWith(LEVEL_PREFIX))
                return true;

            index++;
            scene = SceneManager.GetSceneAt(index);
        }

        return false;
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
