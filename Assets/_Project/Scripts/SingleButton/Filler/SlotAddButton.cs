using UnityEngine;

public class SlotAddButton : ButtonClickInformer
{
    [SerializeField] private PopupCharacterList _characterList;
    [SerializeField] private HeroIcon _heroIcon;

    private Hero _hero;

    public Hero Hero => _hero;

    public void ResetHero()
    {
        _hero = null;
        _heroIcon.gameObject.SetActive(false);
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        _heroIcon.Clicked += OnClickIcon;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        _heroIcon.Clicked -= OnClickIcon;
    }

    public void SetHero(Hero hero)
    {
        _heroIcon.SetHero(hero);
        _heroIcon.gameObject.SetActive(true);
        _hero = hero;
    }

    protected override void OnClick()
    {
        _characterList.NeedSelect(this);
        _characterList.ShowPopUp();
    }

    private void OnClickIcon(HeroIcon heroIcon)
    {
        _characterList.HideIcon(heroIcon.Hero);
        OnClick();
    }
}