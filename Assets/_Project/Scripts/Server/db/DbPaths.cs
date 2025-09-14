using System.IO;
using UnityEngine;

public static class DbPaths
{
    // �������� ����� "db"
    public static readonly string DbFolder =
        Path.Combine(Application.persistentDataPath, "db");

    // ����� ��� ��������� � �������� �������
    public static readonly string PlayerFolder =
        Path.Combine(DbFolder, "Player");

    // ����� JSON ���� ���������
    public static readonly string AccountsFile =
        Path.Combine(PlayerFolder, "accounts.json");

    // ����� � ������� �������������� ��������
    public static readonly string PlayersDataFolder =
        Path.Combine(PlayerFolder, "PlayersData");

    // ���� � JSON ����������
    public static readonly string PromoFile =
        Path.Combine(DbFolder, "promo.json");
}