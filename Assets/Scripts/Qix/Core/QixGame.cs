using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum QixBoardState
{
    NULL = int.MinValue,

    TEMP_2 = -11,
    TEMP_1 = -10,

    Line = -1,
    Empty = 0,

    Colored_Start = 1,
    Colored_End = int.MaxValue,
}

public class QixGame
{
    QixBoardState[][] m_Board;
    int m_ColoredCount = 0;
    bool m_IsAllColored = false;


    public Vector2Int BoardSize => m_Board == null ? Vector2Int.zero : new Vector2Int(m_Board[0] == null ? 0 : m_Board[0].Length, m_Board.Length);
    public int ColoredCount => m_ColoredCount;
    public bool IsAllColored => m_IsAllColored;
    public Action OnUpdateBoard;
    public Action<Vector2Int[]> OnDrawLine;
    public Action<Vector2Int[]> OnFloodFill;


    System.Threading.Thread m_DrawLineThread;

    public QixGame(Vector2Int boardSize)
    {
        m_Board = new QixBoardState[boardSize.y][];
        for (int i = 0; i < m_Board.Length; i++)
            m_Board[i] = new QixBoardState[boardSize.y];

        Clear();
    }

    public void Clear()
    {
        if (m_Board != null)
        {
            for (int y = 0; y < m_Board.Length; y++)
            {
                for (int x = 0; x < m_Board[y].Length; x++)
                {
                    if (x == 0 || y == 0 || x == m_Board[y].Length - 1 || y == m_Board.Length - 1)
                        m_Board[y][x] = QixBoardState.Line; //끝부분 라인처리
                    else
                        m_Board[y][x] = QixBoardState.Empty;
                }
            }
        }

        m_ColoredCount = 0;
        m_IsAllColored = false;
    }


    public Vector2Int[] DrawLine(params Vector2Int[] positions)
    {
        if (m_Board == null) return null;
        if (positions == null || positions.Length < 2) return null;

        if ((QixBoardState.Colored_Start + m_ColoredCount) >= QixBoardState.Colored_End)
        {
            return null;
        }

        // 줄긋기
        List<Vector2Int> drawLinePosList = new List<Vector2Int>();
        Vector2Int direction = Vector2Int.zero;
        Vector2Int boardIndex = Vector2Int.one * -1;
        for (int i = 1; i < positions.Length; i ++)
        {
            if (positions[i - 1].x == positions[i].x)
            {
                if (positions[i - 1].y < positions[i].y)
                    direction = Vector2Int.up;
                else
                    direction = Vector2Int.down;
            }
            else if (positions[i - 1].y == positions[i].y)
            {
                if (positions[i - 1].x < positions[i].x)
                    direction = Vector2Int.right;
                else
                    direction = Vector2Int.left;
            }
            else
            {
                // error....
                return null;
            }

            boardIndex = positions[i - 1];
            while(boardIndex != positions[i])
            {
                if (boardIndex.y >= 0 && boardIndex.y < m_Board.Length &&
                    m_Board[boardIndex.y] != null && boardIndex.x >= 0 && boardIndex.x < m_Board[boardIndex.y].Length)
                {
                    drawLinePosList.Add(boardIndex);
                    m_Board[boardIndex.y][boardIndex.x] = QixBoardState.Line;
                }
                boardIndex += direction;
            }
            drawLinePosList.Add(positions[i]);
            m_Board[positions[i].y][positions[i].x] = QixBoardState.Line;
        }
        OnDrawLine?.Invoke(drawLinePosList.ToArray());
        drawLinePosList.Clear();


        // 마지막줄을 기준으로 좌우 나눠보자.
        Vector2Int[] qixPos = new Vector2Int[2];
        if (direction == Vector2Int.up)
        {
            qixPos[0] = boardIndex + Vector2Int.down + Vector2Int.left;
            qixPos[1] = boardIndex + Vector2Int.down + Vector2Int.right;
        }
        else if (direction == Vector2Int.down)
        {
            qixPos[0] = boardIndex + Vector2Int.up + Vector2Int.left;
            qixPos[1] = boardIndex + Vector2Int.up + Vector2Int.right;
        }
        else if (direction == Vector2Int.left)
        {
            qixPos[0] = boardIndex + Vector2Int.right + Vector2Int.up;
            qixPos[1] = boardIndex + Vector2Int.right + Vector2Int.down;
        }
        else if (direction == Vector2Int.right)
        {
            qixPos[0] = boardIndex + Vector2Int.left + Vector2Int.up;
            qixPos[1] = boardIndex + Vector2Int.left + Vector2Int.down;
        }


        // 체워라!!!
        var changedPositions = QixFloodFill.AutoFill<QixBoardState>(
            m_Board, 
            qixPos, 
            QixBoardState.Colored_Start + m_ColoredCount, 
            new QixBoardState[] { QixBoardState.TEMP_1, QixBoardState.TEMP_2 },
            QixBoardState.Empty);
        m_ColoredCount ++;


        // 가득 체웠는지 체크해보자.
        m_IsAllColored = true;
        for (int y = 0; y < m_Board.Length; y++)
        {
            for (int x = 0; x < m_Board[y].Length; x++)
            {
                if (m_Board[y][x] == QixBoardState.Empty)
                {
                    m_IsAllColored = false;
                    break;
                }
            }
            if (m_IsAllColored == false)
                break;
        }

        OnFloodFill?.Invoke(changedPositions);
        return changedPositions;
    }

    public IEnumerator DrawLineRoutine(Vector2Int[] positions, System.Action onFinished)
    {
        if (m_Board == null)
        {
            onFinished?.Invoke();
            yield break;
        }

        if (positions == null || positions.Length < 2)
        {
            onFinished?.Invoke();
            yield break;
        }

        if ((QixBoardState.Colored_Start + m_ColoredCount) >= QixBoardState.Colored_End)
        {
            onFinished?.Invoke();
            yield break;
        }

        bool isCompleted = false;
        Vector2Int[] changedPositions = null;
        List<Vector2Int> drawLinePosList = new List<Vector2Int>();
        m_DrawLineThread = new System.Threading.Thread(() =>
        {
            // 줄긋기
            Vector2Int direction = Vector2Int.zero;
            Vector2Int boardIndex = Vector2Int.one * -1;
            for (int i = 1; i < positions.Length; i++)
            {
                if (positions[i - 1].x == positions[i].x)
                {
                    if (positions[i - 1].y < positions[i].y)
                        direction = Vector2Int.up;
                    else
                        direction = Vector2Int.down;
                }
                else if (positions[i - 1].y == positions[i].y)
                {
                    if (positions[i - 1].x < positions[i].x)
                        direction = Vector2Int.right;
                    else
                        direction = Vector2Int.left;
                }
                else
                {
                    // error....
                    isCompleted = true;
                    return;
                }

                boardIndex = positions[i - 1];
                while (boardIndex != positions[i])
                {
                    if (boardIndex.y >= 0 && boardIndex.y < m_Board.Length &&
                        m_Board[boardIndex.y] != null && boardIndex.x >= 0 && boardIndex.x < m_Board[boardIndex.y].Length)
                    {
                        drawLinePosList.Add(boardIndex);
                        m_Board[boardIndex.y][boardIndex.x] = QixBoardState.Line;
                    }
                    boardIndex += direction;
                }
                drawLinePosList.Add(positions[i]);
                m_Board[positions[i].y][positions[i].x] = QixBoardState.Line;
            }


            // 마지막줄을 기준으로 좌우 나눠보자.
            Vector2Int[] qixPos = new Vector2Int[2];
            if (direction == Vector2Int.up)
            {
                qixPos[0] = boardIndex + Vector2Int.down + Vector2Int.left;
                qixPos[1] = boardIndex + Vector2Int.down + Vector2Int.right;
            }
            else if (direction == Vector2Int.down)
            {
                qixPos[0] = boardIndex + Vector2Int.up + Vector2Int.left;
                qixPos[1] = boardIndex + Vector2Int.up + Vector2Int.right;
            }
            else if (direction == Vector2Int.left)
            {
                qixPos[0] = boardIndex + Vector2Int.right + Vector2Int.up;
                qixPos[1] = boardIndex + Vector2Int.right + Vector2Int.down;
            }
            else if (direction == Vector2Int.right)
            {
                qixPos[0] = boardIndex + Vector2Int.left + Vector2Int.up;
                qixPos[1] = boardIndex + Vector2Int.left + Vector2Int.down;
            }


            // 체워라!!!
            changedPositions = QixFloodFill.AutoFill<QixBoardState>(
                m_Board,
                qixPos,
                QixBoardState.Colored_Start + m_ColoredCount,
                new QixBoardState[] { QixBoardState.TEMP_1, QixBoardState.TEMP_2 },
                QixBoardState.Empty);
            m_ColoredCount++;


            // 가득 체웠는지 체크해보자.
            m_IsAllColored = true;
            for (int y = 0; y < m_Board.Length; y++)
            {
                for (int x = 0; x < m_Board[y].Length; x++)
                {
                    if (m_Board[y][x] == QixBoardState.Empty)
                    {
                        m_IsAllColored = false;
                        break;
                    }
                }
                if (m_IsAllColored == false)
                    break;
            }

            isCompleted = true;
        });

        m_DrawLineThread.Start();
        while (isCompleted == false) yield return null;
        m_DrawLineThread.Abort();
        m_DrawLineThread = null;

        OnDrawLine?.Invoke(drawLinePosList.ToArray());
        drawLinePosList.Clear();

        OnFloodFill?.Invoke(changedPositions == null ? new Vector2Int[0] : changedPositions);
        onFinished?.Invoke();
    }

    public QixBoardState GetBoardState(int x, int y)
    {
        if (m_Board == null) return QixBoardState.NULL;
        if (y < 0 || y >= m_Board.Length) return QixBoardState.NULL;
        if (m_Board[y] == null) return QixBoardState.NULL;
        if (x < 0 || x >= m_Board[y].Length) return QixBoardState.NULL;

        return m_Board[y][x];
    }

    public bool IsNearLine(int x, int y)
    {
        if (GetBoardState(x + 1, y) == QixBoardState.Line) return true;
        if (GetBoardState(x - 1, y) == QixBoardState.Line) return true;
        if (GetBoardState(x, y + 1) == QixBoardState.Line) return true;
        if (GetBoardState(x, y - 1) == QixBoardState.Line) return true;
        if (GetBoardState(x + 1, y + 1) == QixBoardState.Line) return true;
        if (GetBoardState(x + 1, y - 1) == QixBoardState.Line) return true;
        if (GetBoardState(x - 1, y + 1) == QixBoardState.Line) return true;
        if (GetBoardState(x - 1, y - 1) == QixBoardState.Line) return true;

        return false;
    }

}
