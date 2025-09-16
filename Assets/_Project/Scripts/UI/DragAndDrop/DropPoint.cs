using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class DropPoint : MonoBehaviour
{
    public RectTransform Rect { get; private set; }
    public DraggableItem Occupant;
    public bool IsLocked;

    void Awake()
    {
        Rect = GetComponent<RectTransform>();
        Occupant = null;
        IsLocked = false;
    }

    public bool IsOccupied => Occupant != null || IsLocked;

    public void Reserve(DraggableItem item)
    {
        IsLocked = true;
        Occupant = item;
    }

    public void Commit(DraggableItem item)
    {
        IsLocked = false;
        Occupant = item;
    }

    public void Clear()
    {
        IsLocked = false;
        Occupant = null;
    }
}