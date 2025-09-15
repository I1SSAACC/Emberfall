using System.IO;
using UnityEngine;

public static class DbPaths
{
    // 1) Корень билда: это папка, где лежит ваш .exe (в Linux/Mac — аналогично)
    private static readonly string BuildRoot =
#if UNITY_EDITOR
        // в редакторе удобнее смотреть в Assets, но можно переназначить
        Application.dataPath;
#else
        // в собранном билде Application.dataPath → "<MyGame>_Data"
        // Path.GetDirectoryName вернёт путь к папке с .exe
        Path.GetDirectoryName(Application.dataPath);
#endif

    // 2) Главная папка для всех JSON
    public static readonly string DbFolder = Path.Combine(BuildRoot, "db");

    // 3) Папка для аккаунтов и их общей базы
    public static readonly string PlayerFolder = Path.Combine(DbFolder, "Player");

    // 4) Файл accounts.json (список всех аккаунтов)
    public static readonly string AccountsFile = Path.Combine(PlayerFolder, "accounts.json");

    // 5) Папка со всеми личными данными игроков
    public static readonly string PlayersDataFolder = Path.Combine(PlayerFolder, "PlayersData");

    // 6) Файл для промокодов
    public static readonly string PromoFile = Path.Combine(DbFolder, "promo.json");
}