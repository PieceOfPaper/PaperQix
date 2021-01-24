using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIQixLineRenderer : Graphic
{ 
    [SerializeField] float m_Thickness = 10f;
    [SerializeField] Vector2[] m_Points;


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


    public void SetPoints(List<Vector2> points)
    {
        m_Points = points.ToArray();
        SetVerticesDirty();
    }

    public void SetPoints(Vector2[] points)
    {
        m_Points = points;
        SetVerticesDirty();
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        if (m_Points == null || m_Points.Length < 2)
            return;

        Vector2 pos1;
        Vector2 pos2;
        float angle = 0;
        int startIndex = 0;
        UIVertex vertexTemp;
        for (int i = 0; i < m_Points.Length - 1; i ++)
        {
            pos1 = m_Points[i];
            pos2 = m_Points[i + 1];
            angle = Mathf.Atan2(pos2.y - pos1.y, pos2.x - pos1.x) * Mathf.Rad2Deg;

            // 0
            vertexTemp = UIVertex.simpleVert;
            vertexTemp.color = color;
            vertexTemp.position = new Vector3(pos1.x, pos1.y) + Quaternion.Euler(0, 0, angle) * Vector3.up * m_Thickness;
            vh.AddVert(vertexTemp);

            // 1
            vertexTemp = UIVertex.simpleVert;
            vertexTemp.color = color;
            vertexTemp.position = new Vector3(pos1.x, pos1.y) + Quaternion.Euler(0, 0, angle) * Vector3.down * m_Thickness;
            vh.AddVert(vertexTemp);

            // 2
            vertexTemp = UIVertex.simpleVert;
            vertexTemp.color = color;
            vertexTemp.position = new Vector3(pos2.x, pos2.y) + Quaternion.Euler(0, 0, angle) * Vector3.down * m_Thickness;
            vh.AddVert(vertexTemp);

            // 3
            vertexTemp = UIVertex.simpleVert;
            vertexTemp.color = color;
            vertexTemp.position = new Vector3(pos2.x, pos2.y) + Quaternion.Euler(0, 0, angle) * Vector3.up * m_Thickness;
            vh.AddVert(vertexTemp);

            startIndex = i * 4;
            vh.AddTriangle(startIndex + 0, startIndex + 1, startIndex + 2);
            vh.AddTriangle(startIndex + 2, startIndex + 3, startIndex + 0);
        }
    }
}
