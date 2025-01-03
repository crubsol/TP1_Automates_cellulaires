using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName ="Game of life/Pattern")]
public class Pattern : ScriptableObject
{
    public Vector2Int[] cells;

    public Vector2Int GetCenter() //encontrar el centro del patron
    {
        if (cells == null || cells.Length == 0)
        {
            return Vector2Int.zero;
        }

        Vector2Int min = Vector2Int.zero;
        Vector2Int max = Vector2Int.zero;

        for (int i = 0; i < cells.Length; i++)
        {
            Vector2Int cell = cells[i];
            min.x = Mathf.Min(cell.x, min.x);
            min.y = Mathf.Min(cell.y, min.y);
            max.x = Mathf.Max(cell.x, min.x);
            max.y = Mathf.Max(cell.y, min.y);
        }

        return (min + max) / 2;
    }
}

