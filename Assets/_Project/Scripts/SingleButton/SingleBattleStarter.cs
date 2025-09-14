// MainMenuBattleStarter.cs
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;

#region Network Messages
public struct StartSingleBattleRequest : NetworkMessage
{
    public string[] teamConfig;  // IDs (names) of selected heroes
    public long seed;        // deterministic seed for local + server sim
}

public struct StartSingleBattleResponse : NetworkMessage
{
    public bool accepted;        // server acknowledgment
}
#endregion

/// <summary>
/// Вешается на кнопку «Начать бой» в главном меню.
/// Собирает список выбранных героев (Hero), отправляет запрос на сервер и
/// по подтверждению загружает сцену боя.
/// </summary>
public class SingleBattleStarter : MonoBehaviour
{
    [Header("Выбранные герои для боя")]
    [Tooltip("Drag & drop Hero-префабы, которые игрок выбрал для этого боя")]
    [SerializeField] private List<Hero> selectedHeroes = new List<Hero>();

    private void Start()
    {
        // Регистрируем приёмник ответа от сервера перед первым запросом
        NetworkClient.RegisterHandler<StartSingleBattleResponse>(OnStartBattleResponse, false);
    }

    /// <summary>
    /// Привяжите этот метод к кнопке «Начать бой» на главном меню.
    /// </summary>
    public void OnStartBattleButtonPressed()
    {
        if (selectedHeroes == null || selectedHeroes.Count == 0)
        {
            Debug.LogWarning("MainMenuBattleStarter: нет выбранных героев для боя.");
            return;
        }

        // Формируем конфиг команды по именам префабов (или любому другому уникальному ID)
        string[] teamConfig = selectedHeroes
            .Select(hero => hero.gameObject.name)
            .ToArray();

        // Генерируем уникальный seed для детерминированной симуляции
        long seed = DateTime.UtcNow.Ticks;

        var msg = new StartSingleBattleRequest
        {
            teamConfig = teamConfig,
            seed = seed
        };

        Debug.Log($"MainMenuBattleStarter: отправка запроса на бой (heroes={teamConfig.Length}, seed={seed})");
        NetworkClient.Send(msg);
    }

    private void OnStartBattleResponse(StartSingleBattleResponse msg)
    {
        if (!msg.accepted)
        {
            Debug.LogError("MainMenuBattleStarter: сервер отклонил запрос на бой.");
            return;
        }

        Debug.Log("MainMenuBattleStarter: сервер подтвердил бой, загружаю сцену BattleScene...");
        SceneManager.LoadScene("BattleScene");
    }
}
