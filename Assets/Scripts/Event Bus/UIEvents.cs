using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIEvents
{
    public delegate void ModelUpdate<TModel>(TModel model);

    public static ModelUpdate<GridHighlightModel> OnGridPlacementPreview;
}
