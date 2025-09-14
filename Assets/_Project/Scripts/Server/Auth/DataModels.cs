using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using UnityEngine;

[Serializable]
public class AccountEntry
{
    public string guid;
    public string deviceId;      // для «Запомнить меня»
    public string nickname;
    public string email;
    public string passwordHash;  // SHA512
    public bool isOnline;      // чтобы запретить повторный вход
}

[Serializable]
public class AccountsDb
{
    public List<AccountEntry> accounts = new List<AccountEntry>();
}

[Serializable]
public class PlayerData
{
    public string GUID;
    public string DeviceId;
    public string Nickname;
    public string Email;
    public int Level = 1;
    public int Gold = 0;
    public int Diamonds = 0;
    public List<string> OwnedCharacters = new List<string>();
    public int LevelField = 0;
    public int TokenField = 0;
    public PreferencesData PreferencesData = new();

    public List<string> RedeemedPromoCodes = new List<string>();
}
[Serializable]
public class PreferencesData
{
    public float SFXVolume = 1;
    public float MusicVolume = 1;
}

public static class JsonPaths
{
    public static string AccountsFolder => Path.Combine(Application.persistentDataPath, "Accounts");
    public static string PlayersFolder => Path.Combine(Application.persistentDataPath, "PlayerData");
    public static string AccountsFile => Path.Combine(AccountsFolder, "accounts.json");
    public static string PlayerFile(string guid) =>
        Path.Combine(PlayersFolder, guid + ".json");
}