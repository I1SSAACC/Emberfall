using System.Collections.Generic;
using UnityEngine;

public class PopupCharacterList : MonoBehaviour
{
    [SerializeField] private SlotAddButton[] _slotAddButtons;
    [SerializeField] private Hero[] _availableHeroes;
    [SerializeField] private HeroIcon _heroIconPrefab;
    [SerializeField] private Transform _content;

    private SlotAddButton _slotAddButtonSelected;
    private Panel _panel;
    private List<HeroIcon> _heroIcons = new();

    private void Awake()
    {
        _panel = GetComponentInChildren<Panel>(true);
        HidePopUp();

        foreach (Transform child in _content)
            Destroy(child.gameObject);

        foreach (Hero hero in _availableHeroes)
            InstantiateHeroIcon(hero);
    }

    public void ShowPopUp() =>
        _panel.Show();

    public void HidePopUp()
    {
        _panel.Hide();

        foreach (HeroIcon icon in _heroIcons)
            icon.gameObject.SetActive(true);
    }

    public void NeedSelect(SlotAddButton button) =>
        _slotAddButtonSelected = button;

    public void HideIcon(Hero hero)
    {
        foreach (HeroIcon icon in _heroIcons)
                icon.gameObject.SetActive(true);

        foreach (HeroIcon icon in _heroIcons)
        {
            if (icon.Hero == hero)
            {
                icon.gameObject.SetActive(false);
                return;
            }
        }
    }

    private void OnSelectedHero(HeroIcon heroIcon)
    {
        if (_slotAddButtonSelected == null)
            return;

        foreach (HeroIcon icon in _heroIcons)
            icon.gameObject.SetActive(true);

        foreach (SlotAddButton button in _slotAddButtons)
        {
            if (button.Hero == heroIcon.Hero)
            {
                button.ResetHero();
                break;
            }
        }

        _slotAddButtonSelected.SetHero(heroIcon.Hero);
        _slotAddButtonSelected = null;
        HidePopUp();
    }

    private void InstantiateHeroIcon(Hero hero)
    {
        HeroIcon heroIcon = Instantiate(_heroIconPrefab, _content);
        heroIcon.SetHero(hero);
        heroIcon.Clicked += OnSelectedHero;
        _heroIcons.Add(heroIcon);
    }
}