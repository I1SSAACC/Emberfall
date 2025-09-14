using UnityEngine;

public class Hero : MonoBehaviour
{
    [SerializeField] private Sprite _sprite;

    public Sprite Sprite => _sprite;

    public void SetDirection(bool isLeft)
    {
        transform.localScale = new(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
    }
}
