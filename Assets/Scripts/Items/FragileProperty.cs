using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FragileProperty : ItemProperty, IPropertyHandler
{
    public bool isBroken { get; private set; } = false;

    public override void Invoke()
    {
        throw new System.NotImplementedException();
    }

    public void ProcessProperty(ItemProperty property)
    {
        if(property.TryGetComponent(out HeavyProperty heavy))
        {//crush this object
            isBroken = true;
        }
    }
}
