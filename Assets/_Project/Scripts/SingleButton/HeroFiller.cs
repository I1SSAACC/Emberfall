using System.Collections.Generic;
using UnityEngine;

public class HeroFiller : MonoBehaviour
{
    [SerializeField] private Hero[] _availableHeroes;
    [SerializeField] private HeroSpawnPoint[] _spawnPoints;
    [SerializeField] private HeroIcon _heroIconPrefab;
    [SerializeField] private Transform _content;
    [SerializeField] private int _count = 3;

    private void Awake()
    {
        Fill();
    }

    private void Fill()
    {
        List<Hero> tempHeroes = new(_availableHeroes);
        List<HeroSpawnPoint> tempSpawnPoints = new(_spawnPoints);

        for (int i = 0; i < _count; i++)
        {
            HeroIcon icon = Instantiate(_heroIconPrefab, _content);
            Hero hero = tempHeroes[Random.Range(0, tempHeroes.Count)];
            HeroSpawnPoint heroSpawnPoint = tempSpawnPoints[Random.Range(0, tempHeroes.Count)];
            Instantiate(hero, heroSpawnPoint.transform);
            icon.SetIcon(hero.Sprite);
            hero.SetDirection(true);
            tempHeroes.Remove(hero);
            tempSpawnPoints.Remove(heroSpawnPoint);
        }
    }
}
