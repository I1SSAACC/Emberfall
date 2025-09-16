using System;
using UnityEngine;
using UnityEngine.UI;

public class HeroIcon : ButtonClickInformer
{
    [SerializeField] private Image _image;
    
    private Hero _hero;

    public event Action<HeroIcon> Clicked;

    public Hero Hero => _hero;

    public void SetHero(Hero hero)
    {
        _hero = hero;
        SetIcon(hero.Sprite);
    }

    public void SetIcon(Sprite sprite) =>
        _image.sprite = sprite;

    protected override void OnClick() =>
        Clicked?.Invoke(this);
}