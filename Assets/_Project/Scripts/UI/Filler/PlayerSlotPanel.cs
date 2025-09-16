using System.Collections.Generic;
using UnityEngine;

public class PlayerSlotPanel : MonoBehaviour
{
    [SerializeField] private Hero[] _availableHeroes;
    [SerializeField] private HeroSlot _playerSlotPrefab;
    [SerializeField] private Transform _content;

    private List<HeroSlot> _playerSlots = new();

    private void Awake()
    {
        foreach (Transform child in _content)
            Destroy(child.gameObject);

        foreach (Hero hero in _availableHeroes)
            InstantiateSlot(hero);
    }

    private void InstantiateSlot(Hero hero)
    {
        HeroSlot playerSlot = Instantiate(_playerSlotPrefab, _content);
        playerSlot.SetHero(hero);
        _playerSlots.Add(playerSlot);
    }
}