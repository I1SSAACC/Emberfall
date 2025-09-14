using System.IO;
using UnityEngine;

[DefaultExecutionOrder(-100)]
public class ServerDataManager : MonoBehaviour
{
    public static ServerDataManager Instance { get; private set; }

    public AccountsDb accountsDb;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        EnsureDbFolders();
        LoadAccounts();
    }

    // ������ ��������� db/Player � �������
    void EnsureDbFolders()
    {
        Debug.Log($"[ServerDataManager] DB root: {DbPaths.DbFolder}");
        Directory.CreateDirectory(DbPaths.DbFolder);
        Directory.CreateDirectory(DbPaths.PlayerFolder);
        Directory.CreateDirectory(DbPaths.PlayersDataFolder);
    }

    // ��������� ��� ������ ����� accounts.json
    void LoadAccounts()
    {
        if (File.Exists(DbPaths.AccountsFile))
        {
            string json = File.ReadAllText(DbPaths.AccountsFile);
            accountsDb = JsonUtility.FromJson<AccountsDb>(json);
        }
        else
        {
            accountsDb = new AccountsDb();
            SaveAccounts();
        }
    }

    // ��������� ����� JSON ���������
    public void SaveAccounts()
    {
        string json = JsonUtility.ToJson(accountsDb, true);
        File.WriteAllText(DbPaths.AccountsFile, json);
    }

    // ��������� ��� ������ ������� ������ �� GUID
    public PlayerData LoadOrCreatePlayer(string guid, string email = "", string nickname = "")
    {
        string path = Path.Combine(DbPaths.PlayersDataFolder, $"{guid}.json");
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            return JsonUtility.FromJson<PlayerData>(json);
        }
        else
        {
            var pd = new PlayerData
            {
                GUID = guid,
                Email = email,
                Nickname = nickname,
                Level = 1,
                Gold = 100
            };
            SavePlayer(pd);
            return pd;
        }
    }

    // ��������� ������ ����������� PlayerData
    public void SavePlayer(PlayerData pd)
    {
        string path = Path.Combine(DbPaths.PlayersDataFolder, $"{pd.GUID}.json");
        string json = JsonUtility.ToJson(pd, true);
        File.WriteAllText(path, json);
    }
}