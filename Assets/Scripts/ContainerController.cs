using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro.EditorUtilities;
using UnityEngine;
using UnityEngine.Serialization;

public class ContainerController : MonoBehaviour
{
    public static Vector3Int[] ADJACENCY_OFFSETS = new Vector3Int[]
    {
        Vector3Int.up,
        Vector3Int.right,
        Vector3Int.down,
        Vector3Int.left,
    };

    public static Vector3Int[] GetAdjacents(Vector3Int center)
    {
        List<Vector3Int> result = new List<Vector3Int>();

        foreach(Vector3Int offset in  ADJACENCY_OFFSETS)
        {
            result.Add(offset + center);
        }

        return result.ToArray();
    }

    public static Vector3Int[] GetAdjacents(Vector2Int center)
    {
        List<Vector3Int> result = new List<Vector3Int>();

        foreach(Vector3Int offset in  ADJACENCY_OFFSETS)
        {
            result.Add(offset + (Vector3Int)center);
        }

        return result.ToArray();
    }

    public static Action<ItemContainer> ContainerClosed;
    public static Action<ItemContainer> ContainerItemsUpdated;

    [SerializeField] private ItemContainer _container;
    [SerializeField] private GameObject _containerPrefab;

    [SerializeField]
    private GridContainerMovement _mover;

    [SerializeField]
    Vector2Int _moveOffset = new Vector2Int(0, -1);

    [SerializeField]
    [FormerlySerializedAs("_moveInterval")]
    [Min(0f)]
    float _tickInterval = 1;

    [Header("SFX")]
    public AudioSource _audio;
    public AudioClip sfx_bagDone;
    public AudioClip sfx_bagSpawn;

    WaitForSeconds _waitForMove;

    private void Awake()
    {
        _waitForMove = new WaitForSeconds(_tickInterval);
        _container.ItemAdded += UpdateItemAdjacency;
        _container.ItemRemoved += OnItemRemoved;
        _mover.ItemMoved += UpdateItemAdjacency;
    }

    private void UpdateItemAdjacency(IGridContainable item)
    {
        CheckAdjacencyEffects(item);

        var itemAnchor = _container.GetAnchorCell(item);
        var borderPositions = item.BorderPositions;
        //Loop through all cells of this item
        foreach(var borderPos in borderPositions) {
            var containerPosition = borderPos + itemAnchor;

            if(_container.Cells.TryGetValue(containerPosition, out var adjacentItem))
            {
                CheckAdjacencyEffects(adjacentItem);
            }
        }

        StartCoroutine(WaitThenEvokeUpdate(_container));
    }

    private void OnItemRemoved(IGridContainable item)
    {
        HashSet<ItemProperty> properties = new HashSet<ItemProperty>(item.Owner.GetComponents<ItemProperty>());
        foreach (var property in properties)
        {
            Destroy(property);
        }

        var itemAnchor = _container.GetAnchorCell(item);

        var borderPositions = item.BorderPositions;
        //Loop through all cells of this item
        foreach(var borderPos in borderPositions) {
            var containerPosition = borderPos + itemAnchor;

            //Look at adjacent tiles for item effects
            ItemTile tile = _container.ItemTileMap.GetTile<ItemTile>((Vector3Int)containerPosition);

            if (tile == null)
                continue;

            if(_container.Cells.TryGetValue(containerPosition, out var adjacentItem))
            {
                CheckAdjacencyEffects(adjacentItem);
            }
        }

        StartCoroutine(WaitThenEvokeUpdate(_container));
    }

    private void CheckAdjacencyEffects(IGridContainable item)
    {
        var itemAnchor = _container.GetAnchorCell(item);
        HashSet<ItemProperty> properties = new HashSet<ItemProperty>(item.Owner.GetComponents<ItemProperty>());

        foreach(var borderPos in item.BorderPositions)
        {
            var containerPosition = borderPos + itemAnchor;

            //Look at adjacent tiles for item effects
            ItemTile tile = _container.ItemTileMap.GetTile<ItemTile>((Vector3Int)containerPosition);

            if (tile == null)
                continue;

            if(properties.Any((property) => tile.IsTypeMatch(property.GetType())))
            {
                properties.RemoveWhere((itemProp) => tile.IsTypeMatch(itemProp.GetType()));
                continue;
            }
            tile.ApplyItemEffect(item.Owner);
        }

        if(properties.Count > 0)
        {
            foreach(var property in properties)
            {
                Destroy(property);
            }
        }


    }

    private IEnumerator WaitThenEvokeUpdate(ItemContainer container)
    {
        yield return null;
        ContainerItemsUpdated?.Invoke(container);
    }

    private void Start()
    {
        InvokeRepeating(nameof(DoMove), 0, _tickInterval);
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
                property.Tick();
            }
        }

        foreach(var cell in cellsToRemove)
        {
            _container.Cells.Remove(cell);
        }
        _mover.DoMoves();

    }
}
