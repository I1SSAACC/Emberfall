using UnityEngine;

public class ButtonOpenPopupCharacterSelector : ButtonClickInformer
{
    [SerializeField] private PopupCharacterList _characterList;

    protected override void OnClick()
    {
        _characterList.gameObject.SetActive(true);
        _characterList.NeedSelect(this);
    }
}