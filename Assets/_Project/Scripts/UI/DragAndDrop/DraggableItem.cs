using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(CanvasGroup))]
public class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private float _moveDuration;
    [SerializeField] private float _snapDistance;
    [SerializeField] private PanelSlot _originalSlot;
    [SerializeField] private DropPoint _currentPoint;
    public RectTransform RectTransform { get; private set; }

    Canvas _canvas;
    CanvasGroup _canvasGroup;
    Transform _startParent;
    Vector3 _startWorldPos;
    int _startSiblingIndex;

    void Awake()
    {
        RectTransform = GetComponent<RectTransform>();
        _canvasGroup = GetComponent<CanvasGroup>();
        _canvas = GetComponentInParent<Canvas>();

        if (_originalSlot == null)
        {
            if (transform.parent != null)
                _originalSlot = transform.parent.GetComponent<PanelSlot>();

            if (_originalSlot == null)
                _originalSlot = GetComponentInParent<PanelSlot>();
        }

        _startParent = transform.parent;
        _startWorldPos = RectTransform.position;
        _startSiblingIndex = transform.GetSiblingIndex();
    }

    void Start()
    {
        if (_currentPoint == null)
            _currentPoint = GetComponentInParent<DropPoint>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (DragManager.Instance == null) return;
        if (DragManager.Instance.IsBusy) return;

        _startWorldPos = RectTransform.position;
        _startParent = transform.parent;
        _startSiblingIndex = transform.GetSiblingIndex();

        _canvasGroup.blocksRaycasts = false;
        transform.SetParent(_canvas.transform, true);
        RectTransform.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (_canvas == null) return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _canvas.transform as RectTransform,
            eventData.position,
            _canvas.worldCamera,
            out Vector2 localPoint
        );

        RectTransform.localPosition = localPoint;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (DragManager.Instance == null)
        {
            RestoreToPrevious();
            return;
        }

        _canvasGroup.blocksRaycasts = true;

        if (DragManager.Instance.IsBusy)
        {
            RestoreToPrevious();
            return;
        }

        DropPoint targetPoint = DragManager.Instance.FindDropPointUnderPointer(eventData);

        if (targetPoint == null)
        {
            targetPoint = DragManager.Instance.NearestPointWithinDistance(RectTransform.position, DragManager.Instance.SnapDistance);
        }

        RectTransform panelUnderPointer = DragManager.Instance.FindPanelUnderPointer(eventData);

        if (targetPoint != null)
        {
            float dist = Vector3.Distance(RectTransform.position, targetPoint.Rect.position);
            if (dist <= _snapDistance)
            {
                TryPlaceOnPoint(targetPoint);
                return;
            }
        }

        if (panelUnderPointer != null)
        {
            ReturnToPanel(panelUnderPointer);
            return;
        }

        RestoreToPrevious();
    }

    void TryPlaceOnPoint(DropPoint target)
    {
        if (DragManager.Instance.IsBusy) { RestoreToPrevious(); return; }

        if (target == _currentPoint)
        {
            StartCoroutine(MoveAndParent(RectTransform.position, target.Rect.position, target.Rect, () => { }));
            return;
        }

        if (target.IsOccupied == false)
        {
            target.Reserve(this);

            var prev = _currentPoint;
            if (prev != null) prev.Clear();

            StartCoroutine(MoveAndParent(RectTransform.position, target.Rect.position, target.Rect, () =>
            {
                target.Commit(this);
                _currentPoint = target;
            }));

            return;
        }

        var other = target.Occupant;
        if (other == null)
        {
            target.Reserve(this);
            StartCoroutine(MoveAndParent(RectTransform.position, target.Rect.position, target.Rect, () =>
            {
                target.Commit(this);
                _currentPoint = target;
            }));
            return;
        }

        if (_currentPoint != null)
        {
            var myOldPoint = _currentPoint;
            target.Reserve(this);
            myOldPoint.Reserve(other);

            StartCoroutine(SwapAnimate(other, myOldPoint, this, target));

            return;
        }

        {
            target.Reserve(this);

            RectTransform destPanel = other._originalSlot != null ? other._originalSlot.Rect : DragManager.Instance.PanelRect;

            if (other._currentPoint != null)
            {
                other._currentPoint.Clear();
                other._currentPoint = null;
            }

            other.StartCoroutine(other.MoveAndParent(other.RectTransform.position, destPanel.position, destPanel, () =>
            {
                other._currentPoint = null;
            }));

            StartCoroutine(MoveAndParent(RectTransform.position, target.Rect.position, target.Rect, () =>
            {
                target.Commit(this);
                _currentPoint = target;
            }));

            return;
        }
    }

    void ReturnToPanel(RectTransform panel)
    {
        if (DragManager.Instance.IsBusy) { RestoreToPrevious(); return; }

        if (_currentPoint != null)
        {
            _currentPoint.Clear();
            _currentPoint = null;
        }

        StartCoroutine(MoveAndParent(RectTransform.position, panel.position, panel, () =>
        {
            if (_originalSlot != null)
            {
                transform.SetSiblingIndex(_originalSlot.transform.GetSiblingIndex());
            }
        }));
    }

    void RestoreToPrevious()
    {
        if (_currentPoint != null)
        {
            StartCoroutine(MoveAndParent(RectTransform.position, _currentPoint.Rect.position, _currentPoint.Rect, null));
            return;
        }

        if (_originalSlot != null)
        {
            StartCoroutine(MoveAndParent(RectTransform.position, _originalSlot.Rect.position, _originalSlot.Rect, null));
            return;
        }

        StartCoroutine(MoveAndParent(RectTransform.position, _startWorldPos, _startParent as RectTransform, () =>
        {
            transform.SetSiblingIndex(_startSiblingIndex);
        }));
    }

    IEnumerator MoveAndParent(Vector3 from, Vector3 to, RectTransform finalParent, Action onComplete = null)
    {
        DragManager.Instance.IsBusy = true;

        float t = 0f;
        float dur = Mathf.Max(0.0001f, DragManager.Instance.MoveDuration);
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / dur;
            RectTransform.position = Vector3.Lerp(from, to, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }

        transform.SetParent(finalParent, false);
        RectTransform.anchoredPosition = Vector2.zero;

        onComplete?.Invoke();

        DragManager.Instance.IsBusy = false;
    }

    IEnumerator SwapAnimate(DraggableItem a, DropPoint aDest, DraggableItem b, DropPoint bDest)
    {
        DragManager.Instance.IsBusy = true;

        Vector3 aFrom = a.RectTransform.position;
        Vector3 aTo = aDest.Rect.position;
        Vector3 bFrom = b.RectTransform.position;
        Vector3 bTo = bDest.Rect.position;

        float t = 0f;
        float dur = Mathf.Max(0.0001f, DragManager.Instance.MoveDuration);
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / dur;
            a.RectTransform.position = Vector3.Lerp(aFrom, aTo, Mathf.SmoothStep(0f, 1f, t));
            b.RectTransform.position = Vector3.Lerp(bFrom, bTo, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }

        a.transform.SetParent(aDest.Rect, false);
        a.RectTransform.anchoredPosition = Vector2.zero;
        b.transform.SetParent(bDest.Rect, false);
        b.RectTransform.anchoredPosition = Vector2.zero;

        aDest.Commit(a);
        bDest.Commit(b);

        a._currentPoint = aDest;
        b._currentPoint = bDest;

        DragManager.Instance.IsBusy = false;

        yield break;
    }
}