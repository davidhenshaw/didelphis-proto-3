using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro.EditorUtilities;
using UnityEngine;

public class ContainerController : MonoBehaviour
{
    public static Action<ItemContainer> ContainerClosed;

    [SerializeField] private ItemContainer _container;
    [SerializeField] private GameObject _containerPrefab;

    [SerializeField]
    private GridContainerMovement _mover;

    [SerializeField]
    Vector2Int _moveOffset = new Vector2Int(0, -1);

    [SerializeField]
    [Min(0f)]
    float _moveInterval = 1;

    [Header("SFX")]
    public AudioSource _audio;
    public AudioClip sfx_bagDone;
    public AudioClip sfx_bagSpawn;

    WaitForSeconds _waitForMove;

    private void Awake()
    {
        _waitForMove = new WaitForSeconds(_moveInterval);
    }

    private void Start()
    {
        InvokeRepeating(nameof(DoMove), 0, _moveInterval);
    }

    [ContextMenu("Destroy Container")]
    private void CloseContainer()
    {
        if(_container)
        {
            _audio.PlayOneShot(sfx_bagDone);
            ContainerClosed?.Invoke(_container);
            Destroy(_container.gameObject);
        }
    }

    private void OnGUI()
    {
        if ( GUILayout.Button("Done!", GUILayout.MinHeight(50), GUILayout.MinWidth(200)) )
        {
            CloseContainer();
        }

        if ( GUILayout.Button("Spawn Container",  GUILayout.MinHeight(50), GUILayout.MinWidth(200)) )
        {
            SpawnContainer();
        }
    }


    private void SpawnContainer()
    {
        if (_container)
            return;

        ItemContainer newContainer = Instantiate(_containerPrefab, transform).GetComponent<ItemContainer>();
        _container = newContainer;
        _mover.Container = newContainer;
        _audio.PlayOneShot(sfx_bagSpawn);
    }

    private void DoMove()
    {
        if (!_container)
            return;

        var cellsToRemove = new List<Vector2Int>();

        foreach (var item in _container.Cells.Values.Distinct())
        {
            _mover.RegisterMove(item, _moveOffset);
            var itemProperties = item.Owner.GetComponents<ItemProperty>();
            foreach (var property in itemProperties)
            {
                property.Invoke();
                cellsToRemove.AddRange(property.GetCellsToRemove());
            }
        }

        foreach(var cell in cellsToRemove)
        {
            _container.Cells.Remove(cell);
        }
        _mover.DoMoves();
    }
}
