using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public abstract class ButtonClickInformer : MonoBehaviour
{
    private Button _button;

    private void Awake() =>
      _button = GetComponent<Button>();

    private void OnEnable() =>
        _button.onClick.AddListener(OnClick);

    private void OnDisable() =>
        _button.onClick.RemoveListener(OnClick);

    protected abstract void OnClick();
}