using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIQixBoard : MonoBehaviour
{
    [Header("Texture")]
    public RawImage m_RawImage;
    public TextureFormat m_TextureFormat = TextureFormat.RGBA64;

    [Header("Empty Color")]
    public Color m_EmptyColor = Color.black;

    [Header("Line Color")]
    public Color m_LineColor = Color.white;

    [Header("Colored Color")]
    public Color[] m_ColoredColor;


    protected QixGame m_QixGame = null;
    protected Texture2D m_Texture = null;



    private void OnDestroy()
    {
        if (m_Texture != null)
            Destroy(m_Texture);
    }

    public void Initialize(QixGame qixGame)
    {
        m_QixGame = qixGame;
        m_QixGame.OnDrawLine += OnDrawLine;
        m_QixGame.OnFloodFill += OnFloodFill;

        if (m_Texture == null ||
            m_Texture.width != m_QixGame.BoardSize.x || m_Texture.height != m_QixGame.BoardSize.y)
        {
            if (m_Texture != null) Destroy(m_Texture);
            m_Texture = new Texture2D(m_QixGame.BoardSize.x, m_QixGame.BoardSize.y, m_TextureFormat, false);
        }

        for (int y = 0; y < m_QixGame.BoardSize.y; y++)
            for (int x = 0; x < m_QixGame.BoardSize.x; x++)
                SetTextureColorByBoardState(x, y, m_QixGame.GetBoardState(x, y));
        m_Texture.Apply();

        if (m_RawImage != null) m_RawImage.texture = m_Texture;
    }

    void OnDrawLine(Vector2Int[] changedPositions)
    {
        if (m_QixGame == null) return;
        if (m_Texture == null) return;

        for (int i = 0; i < changedPositions.Length; i++)
            SetTextureColorByBoardState(changedPositions[i].x, changedPositions[i].y, QixBoardState.Line);
        m_Texture.Apply();
    }

    void OnFloodFill(Vector2Int[] changedPositions)
    {
        if (m_QixGame == null) return;
        if (m_Texture == null) return;

        for (int i = 0; i < changedPositions.Length; i ++)
            SetTextureColorByBoardState(changedPositions[i].x, changedPositions[i].y, (QixBoardState)m_QixGame.ColoredCount);
        m_Texture.Apply();
    }


    void SetTextureColorByBoardState(int x, int y, QixBoardState state)
    {
        if (m_Texture == null) return;
        if (x < 0 || x >= m_Texture.width) return;
        if (y < 0 || y >= m_Texture.height) return;

        switch (state)
        {
            case QixBoardState.Empty:
                m_Texture.SetPixel(x, y, m_EmptyColor);
                break;
            case QixBoardState.Line:
                m_Texture.SetPixel(x, y, m_LineColor);
                break;
            default:
                if (state >= QixBoardState.Colored_Start && state <= QixBoardState.Colored_End)
                {
                    int index = (int)state;
                    if (m_ColoredColor != null && m_ColoredColor.Length > 0)
                        m_Texture.SetPixel(x, y, m_ColoredColor[index % m_ColoredColor.Length]);
                    else
                        m_Texture.SetPixel(x, y, Color.magenta);
                }
                break;
        }
    }
}
