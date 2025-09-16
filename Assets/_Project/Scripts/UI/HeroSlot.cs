using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HeroSlot : MonoBehaviour
{
    [SerializeField] private Image _character;
    [SerializeField] private Image _frame;
    [SerializeField] private Image _levelShield;
    [SerializeField] private Image _gradient;
    [SerializeField] private TextMeshProUGUI _level;
    [SerializeField] private Stars _stars;

    private Hero _hero;

    public void SetHero(Hero hero, bool isLookLeft = false)
    {
        _hero = hero;
        UpdateView(isLookLeft);
    }

    private void UpdateView(bool isLookLeft)
    {
        if (_hero == null)
            return;

        Vector3 scale = _character.transform.localScale;
        scale.x = isLookLeft ? -Mathf.Abs(scale.x) : Mathf.Abs(scale.x);
        _character.transform.localScale = scale;

        _character.sprite = _hero.CharacterSprite;
        //_frame.sprite = _hero.FrameSprite;
        _levelShield.sprite = _hero.LevelShieldSprite;
        _gradient.color = _hero.GradientColor;
        _level.text = _hero.Level.ToString();
        _stars.ActivateStars(_hero.StarsCount);
    }
}