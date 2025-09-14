using UnityEngine;
using UnityEngine.UI;

public class HeroIcon : MonoBehaviour
{
    [SerializeField] private Image _image;

    public void SetIcon(Sprite sprite)
    {
        _image.sprite = sprite;
    }
}