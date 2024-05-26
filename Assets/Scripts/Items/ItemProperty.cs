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

    public virtual void OnEnter()
    {

    }

    /// <summary>
    /// Invoke the property's specific property action
    /// </summary>
    public virtual void Tick()
    {

    }

    public virtual void OnExit()
    {

    }
}

public interface IPropertyHandler
{
    void ProcessProperty(ItemProperty property);
}
