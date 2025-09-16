using UnityEngine;

public class HeroSpawnPoint : MonoBehaviour
{
    [SerializeField] private bool _isAvailablePlayer;

    public bool IsAvailablePlayer => _isAvailablePlayer;
}