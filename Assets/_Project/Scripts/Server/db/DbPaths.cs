using System.IO;
using UnityEngine;

public static class DbPaths
{
    // Корневая папка "db"
    public static readonly string DbFolder =
        Path.Combine(Application.persistentDataPath, "db");

    // Папка для аккаунтов и профилей игроков
    public static readonly string PlayerFolder =
        Path.Combine(DbFolder, "Player");

    // Общий JSON всех аккаунтов
    public static readonly string AccountsFile =
        Path.Combine(PlayerFolder, "accounts.json");

    // Папка с файлами индивидуальных профилей
    public static readonly string PlayersDataFolder =
        Path.Combine(PlayerFolder, "PlayersData");

    // Путь к JSON промокодов
    public static readonly string PromoFile =
        Path.Combine(DbFolder, "promo.json");
}