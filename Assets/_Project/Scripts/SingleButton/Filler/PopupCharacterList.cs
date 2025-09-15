using UnityEngine;

public class PopupCharacterList : MonoBehaviour
{
    private ButtonOpenPopupCharacterSelector _openPopupCharacterSelector;

    public void NeedSelect(ButtonOpenPopupCharacterSelector button) =>
        _openPopupCharacterSelector = button;
}