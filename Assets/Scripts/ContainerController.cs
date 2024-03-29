using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro.EditorUtilities;
using UnityEngine;

public class ContainerController : MonoBehaviour
{
    [SerializeField] private ItemContainer _container;
    [SerializeField]
    private GridContainerMovement _mover;

    [SerializeField]
    Vector2Int _moveOffset = new Vector2Int(0, -1);

    [SerializeField]
    [Min(0f)]
    float _moveInterval = 1;

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
        Destroy(_container.gameObject);
    }

    private void OnGUI()
    {
        if ( GUILayout.Button("Done!", GUILayout.MinHeight(50), GUILayout.MinWidth(200)) )
        {
            CloseContainer();
        }
    }

    private void DoMove()
    {
        if (!_container)
            return;

        foreach(var item in _container.Cells.Values.Distinct())
        {
            _mover.RegisterMove(item, _moveOffset);
        }

        _mover.DoMoves();
    }
}
