using Services;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class GameSession : MonoBehaviour
{
    public int Score => _rulePanelModel.Score;
    private RulePanelModel _rulePanelModel;

    public static event Action<RulePanelModel> ScoreChanged;
    public static event Action<GameSession> GameSessionChanged;
    public static event Action CriteriaMet;

    public UnityEvent OnPuzzleDone;

    [SerializeField] private float completionDelay = 1f;
    private float _criteriaMetTime = 0;

    [SerializeField]
    private Criteria[] _criteria;
    public Criteria[] CurrentCriteria => _criteria;

    [SerializeField]
    private AudioClip sfx_victory;

    private HashSet<ItemContainer> _containers = new();
    private void Awake()
    {
        ContainerController.ContainerItemsUpdated += OnContainerUpdated;
    }

    private void OnDestroy()
    {
        ContainerController.ContainerItemsUpdated -= OnContainerUpdated;
    }
    private void Start()
    {
        GameSessionChanged?.Invoke(this);
    }

    public void OnContainerUpdated(ItemContainer container)
    {
        StartCoroutine(nameof(NotifyScoreChange), container);
    }

    IEnumerator WaitForCompletion()
    {
        _criteriaMetTime = Time.time;

        yield return null;

        while( Time.time - _criteriaMetTime <= completionDelay)
        {
            if (IsAllCriteriaMet())
                yield return null;
            else
                yield break;
        }

        //Then set puzzle as done and begin transition sequence
        OnPuzzleDone?.Invoke();

        ServiceLocator.TryGetService(out AudioService audio);
        audio.Source.PlayOneShot(audio.sfx_victory);
    }

    private bool IsAllCriteriaMet()
    {
        float[] progressAmounts = new float[_criteria.Length]; 
        
        foreach(ItemContainer container in _containers)
        {
            for (int i = 0; i < _criteria.Length; i++)
            {
                var criterion = _criteria[i];
                progressAmounts[i] = criterion.Rule.GetProgress(container, criterion.number, criterion.invert);
            }

            bool allCriteriaMet = progressAmounts.All((prog) => prog >= 1);

            if (allCriteriaMet)
                continue;
            else
                return false;
        }
        
        return true;
    }

    private IEnumerator NotifyScoreChange(ItemContainer container)
    {
        _containers.Add(container);
        float[] progressAmounts = new float[_criteria.Length]; for (int i = 0; i < _criteria.Length; i++)
        {
            var criterion = _criteria[i];
            progressAmounts[i] = criterion.Rule.GetProgress(container, criterion.number, criterion.invert);
        }

        _rulePanelModel.Container = container;
        _rulePanelModel.Progress = progressAmounts;
        _rulePanelModel.Criteria = CurrentCriteria;

        yield return null; //Wait a frame for components to be added/destoryed

        ScoreChanged?.Invoke(_rulePanelModel);
        //Debug.Log("Score: " + Score);
        Func<bool> allCriteriaMet = () => progressAmounts.All((prog) => prog >= 1);

        if(allCriteriaMet())
        {
            CriteriaMet?.Invoke();
            StartCoroutine(WaitForCompletion());
            Debug.Log("Puzzle Completed");
        }
    }
}

[System.Serializable]
public class Criteria
{
    public ScoreRule<ItemContainer> Rule;
    public bool invert;
    public int number;
}
