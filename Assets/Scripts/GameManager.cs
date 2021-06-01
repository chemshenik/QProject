using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

using static Extentions;
public class GameManager : MonoBehaviour
{
    [SerializeField]
    private GameObject m_PointPrefab = null;

    [SerializeField]
    private LineRenderer m_PathLine = null;

    private EventQueue<GameObject> m_PathPoints = new EventQueue<GameObject>();

    [SerializeField]
    private Player m_Player = null;

    private Transform m_PointsHandler = null;

    private List<GameObject> m_PointPool = null;

    private Camera m_MainCamera = null;

    public static System.Action<GameObject> OnPointCreated;

    private bool m_IsPaused = false;

    private void OnEnable()
    {
        Player.OnWaypointComplete += ReturnPointToPool;
        m_PathPoints.OnQueueChanged += DrawLinePath;
        m_Player.OnPlayerPositionChanged += Player_OnPlayerChangePosition;
    }

    private void OnDisable()
    {
        Player.OnWaypointComplete -= ReturnPointToPool;
        m_PathPoints.OnQueueChanged += DrawLinePath;
        m_Player.OnPlayerPositionChanged -= Player_OnPlayerChangePosition;
    }

    private void Awake()
    {
        m_PointPrefab = Resources.Load<GameObject>("Prefabs/Point");
        m_MainCamera = Camera.main;
        CreatePointPool();
    }

    private void Start()
    {
        DrawLineFirstPoint();
    }

    private void Update()
    {
        if(m_IsPaused)
        {
            return;
        }

        if (IsPointerOverUIObject())
        {
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePos = m_MainCamera.ScreenToWorldPoint(Input.mousePosition);
            CreatePoint(mousePos);
        }
    }

    private void DrawLineFirstPoint()
    {
        if(m_PathLine.positionCount == 0)
        {
            Vector3 playerPosition = m_Player.transform.position;
            m_PathLine.positionCount++;
            m_PathLine.SetPosition(0, playerPosition);
        }
    }

    private void DrawLinePath()
    {
        if (m_PathLine.positionCount == 0)
        {
            DrawLineFirstPoint();
        }
        List<GameObject> pathPoints = m_PathPoints.ToList();
        List<Vector3> points = new List<Vector3>();
        points.Add(m_PathLine.GetPosition(0));
        m_PathLine.positionCount = pathPoints.Count() + 1;
        int length = pathPoints.Count();
        for (int i = 0; i < length; i++)
        {
            points.Add(pathPoints[i].transform.position);
        }
        m_PathLine.SetPositions(points.ToArray());
    }

    private void CreatePointPool()
    {
        m_PointsHandler = new GameObject("PointsPool").transform;
        m_PointPool = new List<GameObject>();

        int count = 5;
        for (int i = 0; i < count; i++)
        {
            m_PointPool.Add(AddPointToPool());
        }
    }

    private void CreatePoint(Vector2 position)
    {
        GameObject point = GetPointFromPool();
        point.transform.position = position;
        m_PathPoints.Enqueue(point);
        OnPointCreated?.Invoke(point);
    }

    private GameObject AddPointToPool()
    {
        GameObject @object = Instantiate(m_PointPrefab, m_PointsHandler);
        @object.SetActive(false);
        return @object;
    }

    private GameObject GetPointFromPool()
    {
        GameObject point = m_PointPool.FirstOrDefault(a => !a.activeInHierarchy);

        if(point == null)
        {
            point = AddPointToPool();
        }

        point.SetActive(true);
        return point;
    }

    private void ReturnPointToPool(GameObject point)
    {
        point.SetActive(false);
        m_PathPoints.Dequeue();
    }

    private void Player_OnPlayerChangePosition(Vector2 position)
    {
        m_PathLine.SetPosition(0, new Vector3(position.x, position.y, 0));
    }

    public void Pause()
    {
        m_IsPaused = !m_IsPaused;
        Time.timeScale = m_IsPaused ? 0 : 1;
    }
}
