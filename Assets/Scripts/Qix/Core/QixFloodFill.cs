using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class QixFloodFill
{
    public static readonly Vector2Int[] checkPositions = new Vector2Int[]
        {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right,
        };

    public delegate bool CheckDelegate<T>(T boardStatus);

    //public static void Fill<T>(T[][] board, Vector2Int pos, Vector2Int dir, T changeValue, T targetValue, List<Vector2Int> changedPositionList = null)
    //{
    //    if (board == null) return;
    //    if (pos.y < 0 || pos.y >= board.Length) return;
    //    if (pos.x < 0 || pos.x >= board[pos.y].Length) return;
    //    if (board[pos.y][pos.x].Equals(changeValue) == true) return;
    //    if (board[pos.y][pos.x].Equals(targetValue) == false) return;

    //    board[pos.y][pos.x] = changeValue;
    //    if (changedPositionList != null) changedPositionList.Add(pos);
    //    for (int i = 0; i < checkPositions.Length; i ++)
    //    {
    //        if (dir == checkPositions[i] * -1) continue;
    //        Fill(board, pos + checkPositions[i], checkPositions[i], changeValue, targetValue, changedPositionList);
    //    }
    //}

    public static void Fill<T>(T[][] board, Vector2Int startPos, T changeValue, T targetValue, List<Vector2Int> changedPositionList = null)
    {
        if (board == null) return;

        Vector2Int pos;
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        queue.Enqueue(startPos);

        while (queue.Count > 0)
        {
            pos = queue.Dequeue();

            if (pos.y < 0 || pos.y >= board.Length) continue;
            if (pos.x < 0 || pos.x >= board[pos.y].Length) continue;
            if (board[pos.y][pos.x].Equals(changeValue) == true) continue;
            if (board[pos.y][pos.x].Equals(targetValue) == false) continue;

            board[pos.y][pos.x] = changeValue;
            if (changedPositionList != null) changedPositionList.Add(pos);

            for (int i = 0; i < checkPositions.Length; i++)
            {
                //if (dir == checkPositions[i] * -1) continue;
                queue.Enqueue(pos + checkPositions[i]);
            }
        }
    }

    public static int Count<T>(T[][] board, Vector2Int pos, T changeValue, T targetValue)
    {
        Fill(board, pos, changeValue, targetValue);
        int count = 0;
        for (int y = 0; y < board.Length; y ++)
        {
            for (int x = 0; x < board[y].Length; x ++)
            {
                if (board[y][x].Equals(changeValue) == true)
                {
                    board[y][x] = targetValue;
                    count++;
                }
            }
        }
        return count;
    }

    public static Vector2Int[] AutoFill<T>(T[][] board, Vector2Int[] pos, T changeValue, T[] tempValues, T targetValue)
    {
        if (pos == null || pos.Length == 0)
            return null;

        int currentCount = 0;
        int minCount = int.MaxValue;
        int minIndex = -1;
        for (int i = 0; i < pos.Length; i ++)
        {
            currentCount = Count<T>(board, pos[i], tempValues[i], targetValue);

            if (currentCount < minCount)
            {
                minCount = currentCount;
                minIndex = i;
            }
        }

        var list = new List<Vector2Int>();
        Fill<T>(board, pos[minIndex], changeValue, targetValue, list);
        return list.ToArray();
    }
}
