using UnityEngine;

public class ButtonClosePopupCharacterCelector : ButtonClickInformer
{
    [SerializeField] private PopupCharacterList _characterList;

    protected override void OnClick() =>
        _characterList.HidePopUp();
}