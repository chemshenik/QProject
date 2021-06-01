using System.Linq;
using System.Collections.ObjectModel;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour, IMovable
{
    public float Speed = 10f;

    public float SpeedRotation = 3f;

    private Queue<GameObject> m_Path = new Queue<GameObject>();

    private bool m_IsMoving = false;

    private GameObject m_CurrentPoint = null;

    public static System.Action<GameObject> OnWaypointComplete = null;

    public static System.Action OnPathComplete = null;

    public static System.Action<List<GameObject>> OnPathChanged = null;

    public event System.Action<Vector2> OnPlayerPositionChanged = null;

    private void OnEnable()
    {
        GameManager.OnPointCreated += AddPointToQueue;
    }

    private void OnDisable()
    {
        GameManager.OnPointCreated -= AddPointToQueue;
    }

    public void Move()
    {
        if (m_IsMoving)
        {
            return;
        }
        m_CurrentPoint = m_Path.Dequeue();
        StartCoroutine(Movement());
    }

    private IEnumerator Movement()
    {
        m_IsMoving = true;
        //Vector2 initialPosition = transform.position;
        Vector2 targetPosition = m_CurrentPoint.transform.position;

        yield return StartCoroutine(Rotate());

        while (transform.position != m_CurrentPoint.transform.position)
        {
            transform.position = Vector2.MoveTowards(transform.position, targetPosition, Speed * Time.deltaTime);
            OnPlayerPositionChanged?.Invoke(transform.position);
            yield return null;
        }
        m_IsMoving = false;
        MoveWaypointComplete();
    }

    private IEnumerator Rotate()
    {
        Vector3 dir = m_CurrentPoint.transform.position - transform.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.AngleAxis(angle - 90f, Vector3.forward);

        while (Quaternion.Angle(transform.rotation, targetRotation) > 1f)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, SpeedRotation * Time.deltaTime);
            yield return null;
        }
    }

    private void AddPointToQueue(GameObject point)
    {
        m_Path.Enqueue(point);
        Move();
    }

    private void MoveWaypointComplete()
    {
        OnWaypointComplete?.Invoke(m_CurrentPoint);
        if (m_Path.Count == 0)
        {
            PathComplete();
            return;
        }
        Move();
    }

    private void PathComplete()
    {
        OnPathComplete?.Invoke();
        Debug.Log("PathComplete!");
    }

    //private void Item_OnPathChanged()
    //{
    //    List<GameObject> path = m_Path.ToList();
    //    OnPathChanged?.Invoke(path);
    //}
}
