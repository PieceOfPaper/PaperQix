using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIQixGamePlay : MonoBehaviour
{
    [Header("Setting")]
    public Vector2Int m_BoardSize = new Vector2Int(100, 100);


    [Header("Elements")]
    public UIQixBoard m_Board;
    public UIQixPlayer m_Player;



    QixGame m_QixGame = null;


    // Start is called before the first frame update
    void Start()
    {
        m_QixGame = new QixGame(m_BoardSize);

        if (m_Board != null) m_Board.Initialize(m_QixGame);
        if (m_Player != null) m_Player.Initialize(m_QixGame);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
