using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public static class Extentions
{
    public static bool IsPointerOverUIObject()
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
#if UNITY_EDITOR
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        return results.Count > 0;
#elif UNITY_ANDROID || UNITY_IOS
        foreach (Touch touch in Input.touches)
        {
            int id = touch.fingerId;
            if (EventSystem.current.IsPointerOverGameObject(id))
            {
                return true;
            }
        }

        return false;
#endif
    }
}
public class EventQueue<T> : Queue<T>
{
    public delegate void QueueChangedEvent();
    public event QueueChangedEvent OnQueueChanged;

    public new void Enqueue(T item)
    {
        base.Enqueue(item);
        if(OnQueueChanged != null)
        {
            OnQueueChanged();
        }
    }

    public new T Dequeue()
    {
        T obj = base.Dequeue();
        if (OnQueueChanged != null)
        {
            OnQueueChanged();
        }
        return obj;
    }
}
