using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIQixPlayer : MonoBehaviour
{
    [Header("Movement")]
    [Range(0f, 1f)]
    public float m_MoveCooltime = 0.25f;

    [Header("Ground")]
    public RectTransform m_RectGround;

    [Header("Line")]
    public UIQixLineRenderer m_LineRenderer;



    QixGame m_QixGame = null;
    float m_LastMovedTime = 0f;
    Vector2Int m_CurrentPosition = Vector2Int.zero;

    bool m_IsDrawing = false;
    Vector2Int m_DrawDirection;
    List<Vector2Int> m_DrawLinePoints = new List<Vector2Int>();


    Vector2 m_BlockSize = Vector2.zero;


    RectTransform m_CachedRectTransform;
    public RectTransform CachedRectTransform
    {
        get
        {
            if (m_CachedRectTransform == null)
                m_CachedRectTransform = transform as RectTransform;
            return m_CachedRectTransform;
        }
    }


    public void Initialize(QixGame qixGame)
    {
        this.m_QixGame = qixGame;
        m_BlockSize = new Vector2(m_RectGround.rect.size.x / m_QixGame.BoardSize.x, m_RectGround.rect.size.y / m_QixGame.BoardSize.y);
        UpdateObjectPosition();
    }

    void Update()
    {
        if (m_RectGround != null)
        {
            if (m_RectGround.hasChanged == true)
                m_BlockSize = new Vector2(m_RectGround.rect.size.x / m_QixGame.BoardSize.x, m_RectGround.rect.size.y / m_QixGame.BoardSize.y);
        }
        else
        {
            m_BlockSize = Vector2.zero;
        }

        if ((Time.timeSinceLevelLoad - m_LastMovedTime) > m_MoveCooltime)
        {
            bool isMoved = false;
            if (Input.GetKey(KeyCode.LeftArrow))
                isMoved = Move(Vector2Int.left, Input.GetKey(KeyCode.Space));
            else if (Input.GetKey(KeyCode.RightArrow))
                isMoved = Move(Vector2Int.right, Input.GetKey(KeyCode.Space));
            else if (Input.GetKey(KeyCode.UpArrow))
                isMoved = Move(Vector2Int.up, Input.GetKey(KeyCode.Space));
            else if (Input.GetKey(KeyCode.DownArrow))
                isMoved = Move(Vector2Int.down, Input.GetKey(KeyCode.Space));

            if (isMoved == true)
            {
                m_LastMovedTime = Time.timeSinceLevelLoad;
                UpdateObjectPosition();
            }
        }
    }

    bool Move(Vector2Int movement, bool pressDrawKey)
    {
        if (m_QixGame == null) return false;

        Vector2Int movedPosition = m_CurrentPosition + movement;

        if (movedPosition.x < 0)
            movedPosition.x = 0;
        else if (movedPosition.x >= m_QixGame.BoardSize.x)
            movedPosition.x = m_QixGame.BoardSize.x - 1;

        if (movedPosition.y < 0)
            movedPosition.y = 0;
        else if (movedPosition.y >= m_QixGame.BoardSize.y)
            movedPosition.y = m_QixGame.BoardSize.y - 1;

        if (movedPosition == m_CurrentPosition) return false;

        QixBoardState currentPosState = m_QixGame.GetBoardState(Mathf.FloorToInt(m_CurrentPosition.x), Mathf.FloorToInt(m_CurrentPosition.y));
        QixBoardState movedPosState = m_QixGame.GetBoardState(movedPosition.x, movedPosition.y);

        if (currentPosState == QixBoardState.NULL || movedPosState == QixBoardState.NULL)
        {
            //�̰� �� �̻��� ��Ȳ��.
            Debug.LogErrorFormat("[UIQixPlayer] Move - pos null {0}, {1}", m_CurrentPosition, movedPosition);
            return false;
        }

        if (m_IsDrawing == true)
        {
            // �ݴ�� ���°�?
            if (movement * -1 == m_DrawDirection)
            {
                Debug.Log("[UIQixPlayer] ���α׸��� �ݴ��");
                if (m_DrawLinePoints[m_DrawLinePoints.Count - 1] == movedPosition)
                {
                    m_DrawLinePoints.RemoveAt(m_DrawLinePoints.Count - 1);
                    if (m_DrawLinePoints.Count == 0)
                    {
                        m_IsDrawing = false;
                    }
                }
                m_CurrentPosition = movedPosition;
                return true;
            }

            // ���� �׸����ִ� ������ ���� �� ����?!?!
            if (m_DrawLinePoints.Count >= 2)
            {
                Vector2Int direction = Vector2Int.zero;
                Vector2Int posTemp = Vector2Int.one * -1;
                for (int i = 1; i < m_DrawLinePoints.Count - 1; i++) //�������� ������ �˻��� �ʿ䰡 �����.
                {
                    if (m_DrawLinePoints[i - 1].x == m_DrawLinePoints[i].x)
                    {
                        if (m_DrawLinePoints[i - 1].y < m_DrawLinePoints[i].y)
                            direction = Vector2Int.up;
                        else
                            direction = Vector2Int.down;
                    }
                    else if (m_DrawLinePoints[i - 1].y == m_DrawLinePoints[i].y)
                    {
                        if (m_DrawLinePoints[i - 1].x < m_DrawLinePoints[i].x)
                            direction = Vector2Int.right;
                        else
                            direction = Vector2Int.left;
                    }
                    else
                    {
                        // error....
                        continue;
                    }

                    posTemp = m_DrawLinePoints[i - 1];
                    while (posTemp != m_DrawLinePoints[i])
                    {
                        if (posTemp == m_CurrentPosition) continue;
                        if (movedPosition == posTemp ||
                            movedPosition == posTemp + Vector2Int.left ||
                            movedPosition == posTemp + Vector2Int.right ||
                            movedPosition == posTemp + Vector2Int.up ||
                            movedPosition == posTemp + Vector2Int.down ||
                            movedPosition == posTemp + Vector2Int.left + Vector2Int.up ||
                            movedPosition == posTemp + Vector2Int.left + Vector2Int.down ||
                            movedPosition == posTemp + Vector2Int.right + Vector2Int.up ||
                            movedPosition == posTemp + Vector2Int.right + Vector2Int.down)
                        {
                            // �ȵ�.. ��ó�� ������....
                            Debug.Log("[UIQixPlayer] ���α׸��� ��ó�� �� �ֳ�");
                            return false;
                        }
                        posTemp += direction;
                    }
                }
            }

            // ���������̸� ����
            if (movement == m_DrawDirection)
            {
                Debug.Log("[UIQixPlayer] ���α׸��� ��������");
                m_CurrentPosition = movedPosition;
            }
            else
            {
                // �������� ���� �������.
                Debug.Log("[UIQixPlayer] ���α׸��� ����");
                m_DrawDirection = movement;
                m_DrawLinePoints.Add(new Vector2Int(Mathf.FloorToInt(m_CurrentPosition.x), Mathf.FloorToInt(m_CurrentPosition.y)));
                m_CurrentPosition = movedPosition;
            }

            // ��ó�� �׷��� ������ �ֳ�?
            // �׳� �� �׸��ɷ� Ĩ�ô�!!!
            if (m_QixGame.IsNearLine(movedPosition.x, movedPosition.y) == true)
            {
                Debug.Log("[UIQixPlayer] ���α׸��� ��");
                m_IsDrawing = false;
                m_DrawLinePoints.Add(new Vector2Int(Mathf.FloorToInt(movedPosition.x), Mathf.FloorToInt(movedPosition.y)));
                m_QixGame.DrawLine(m_DrawLinePoints.ToArray());
                m_DrawLinePoints.Clear();
            }
            return true;
        }
        else
        {
            // ������ �׸���?
            if (currentPosState == QixBoardState.Line && 
                movedPosState == QixBoardState.Empty && 
                pressDrawKey == true)
            {
                Debug.Log("[UIQixPlayer] ���α׸��� ����");
                m_IsDrawing = true;
                m_DrawDirection = movement;
                m_DrawLinePoints.Add(new Vector2Int(Mathf.FloorToInt(m_CurrentPosition.x), Mathf.FloorToInt(m_CurrentPosition.y)));
                m_CurrentPosition = movedPosition;
                return true;
            }

            // �⺻�̵�
            if (currentPosState != QixBoardState.Empty && 
                movedPosState != QixBoardState.Empty)
            {
                Debug.Log("[UIQixPlayer] �⺻�̵�");
                m_CurrentPosition = movedPosition;
                return true;
            }
        }

        Debug.Log($"[UIQixPlayer] ??? - {m_CurrentPosition}({currentPosState}), {movedPosition}({movedPosState})");
        return false;
    }

    void UpdateObjectPosition()
    {
        if (CachedRectTransform != null)
        {
            CachedRectTransform.localPosition = BoardPosToUIPos(m_CurrentPosition);
        }

        if (m_LineRenderer != null)
        {
            if (m_IsDrawing == true && m_DrawLinePoints.Count > 0)
            {
                m_LineRenderer.gameObject.SetActive(true);
                List<Vector2> pointList = new List<Vector2>();

                // ����Ʈ ����
                for (int i = 0; i < m_DrawLinePoints.Count; i++)
                    pointList.Add(BoardPosToUIPos(m_DrawLinePoints[i]));

                // �������� ���� �� ������ ����.
                if (m_DrawLinePoints[m_DrawLinePoints.Count - 1] != m_CurrentPosition)
                    pointList.Add(BoardPosToUIPos(m_CurrentPosition));

                m_LineRenderer.SetPoints(pointList.ToArray());
            }
            else
            {
                m_LineRenderer.gameObject.SetActive(false);
            }
        }
    }

    Vector2 BoardPosToUIPos(Vector2Int pos)
    {
        if (m_RectGround == null) return Vector2.zero;
        return new Vector2(
            -m_RectGround.rect.size.x * 0.5f + m_BlockSize.x * 0.5f + m_BlockSize.x * pos.x,
            -m_RectGround.rect.size.y * 0.5f + m_BlockSize.y * 0.5f + m_BlockSize.y * pos.y);
    }
}
