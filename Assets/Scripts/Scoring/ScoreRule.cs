using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ScoreRule<T> : ScriptableObject
{
    public abstract int GetScore(T obj);

    /// <summary>
    /// Judges how far <paramref name="obj"/> is from fulfilling this rule 
    /// </summary>
    /// <param name="obj"></param>
    /// <returns>Float between 0 and 1 representing the progress toward this rule's completion</returns>
    public abstract float GetProgress(T obj, int goalCount, bool invert = false );
}
