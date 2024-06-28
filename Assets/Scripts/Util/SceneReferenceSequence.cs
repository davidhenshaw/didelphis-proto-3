using Services;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneReferenceSequence : MonoBehaviour, IService
{
    public const string LEVEL_PREFIX = "Lvl";

    [SerializeField]
    bool loadFirstImmediate;

    [SerializeField]
    SceneReference[] levels;

    private int currentLevel;

    private void Awake()
    {
        ServiceLocator.RegisterAsService(this);
    }

    // Start is called before the first frame update
    void Start()
    {
        if (IsLevelLoaded(out Scene loadedLevel))
        {
            //Find the loaded level's position in the sequence
            for (int i = 0; i < levels.Length; i++)
            {
                if (levels[i].ScenePath.Contains(loadedLevel.name))
                {
                    currentLevel = i;
                    break;
                }
            }
            return;
        }

        if (loadFirstImmediate && levels.Length > 0)
        {
            SceneManager.LoadSceneAsync(levels[currentLevel].ScenePath, LoadSceneMode.Additive);
        }
    }

    bool IsLevelLoaded(out Scene loadedLevel)
    {
        loadedLevel = default;

        for(int i = 0; i < SceneManager.loadedSceneCount; i++)
        {
            var scene = SceneManager.GetSceneAt(i);
            if (scene.name.StartsWith(LEVEL_PREFIX))
            {
                loadedLevel = scene;
                return true;
            }

        }

        return loadedLevel.IsValid();
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
