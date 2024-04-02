using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ScoreRule<T> : ScriptableObject
{
    public abstract int GetScore(T obj);
}
