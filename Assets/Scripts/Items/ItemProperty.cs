using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Item))]
public abstract class ItemProperty : MonoBehaviour
{
    public Item Item => _Item;
    protected Item _Item;



    protected void Awake()
    {
        _Item = GetComponent<Item>();
    }

    /// <summary>
    /// Invoke the property's specific property action
    /// </summary>
    public abstract void Invoke();
    
    public virtual Vector2Int[] GetCellsToRemove()
    {
        return new Vector2Int[0];
    }
}

public interface IPropertyHandler
{
    void ProcessProperty(ItemProperty property);
}
