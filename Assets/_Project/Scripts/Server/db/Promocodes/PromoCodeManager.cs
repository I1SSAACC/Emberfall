using System;
using System.Collections.Generic;
using System.IO;
using Mirror;
using UnityEngine;
using PromoCodes;

[DefaultExecutionOrder(-90)]
public class PromoCodeManager : MonoBehaviour
{
    public static PromoCodeManager Instance { get; private set; }
    public List<PromoCodeEntry> promoCodes = new List<PromoCodeEntry>();

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadOrCreatePromoFile();

        // регистрируем серверный обработчик
        if (NetworkServer.active)
            NetworkServer.RegisterHandler<RedeemPromoRequest>(OnServerRedeemPromo, false);
    }

    private void LoadOrCreatePromoFile()
    {
        if (File.Exists(DbPaths.PromoFile))
        {
            string json = File.ReadAllText(DbPaths.PromoFile);
            promoCodes = JsonUtility.FromJson<PromoCodeList>(json).codes;
        }
        else
        {
            promoCodes = new List<PromoCodeEntry>
            {
                new PromoCodeEntry
                {
                    code = "FREECHOICE",
                    rewards = new List<RewardOption>
                    {
                        new RewardOption { type = RewardType.Gold,     amount = 100 },
                        new RewardOption { type = RewardType.Diamonds, amount = 10  },
                        new RewardOption { type = RewardType.Tokens,   amount = 5   },
                    }
                },
                new PromoCodeEntry
                {
                    code = "GEMBONUS",
                    rewards = new List<RewardOption>
                    {
                        new RewardOption { type = RewardType.Diamonds, amount = 50 }
                    }
                }
            };
            SavePromoCodes();
        }
    }

    public void SavePromoCodes()
    {
        var wrapper = new PromoCodeList { codes = promoCodes };
        string json = JsonUtility.ToJson(wrapper, true);
        File.WriteAllText(DbPaths.PromoFile, json);
    }

    // Обработка запроса от клиента
    private void OnServerRedeemPromo(NetworkConnectionToClient conn, RedeemPromoRequest req)
    {
        // 1. Вычисляем GUID игрока по соединению
        var nm = NetworkManager.singleton as CustomNetworkManager;
        if (nm == null || !nm.connectionToGuid.TryGetValue(conn, out string guid))
        {
            conn.Send(new RedeemPromoResponse
            {
                success = false,
                message = "Неизвестный клиент"
            });
            return;
        }

        // 2. Загружаем профиль этой GUID
        var pd = ServerDataManager.Instance.LoadOrCreatePlayer(guid);

        // 3. Валидируем код
        if (!ValidateCode(req.code, pd, out PromoCodeEntry entry, out string msg))
        {
            conn.Send(new RedeemPromoResponse
            {
                success = false,
                message = msg
            });
            return;
        }

        // 4. Применяем первую опцию награды
        var option = entry.rewards[0];
        ApplyReward(pd, entry, option);

        // 5. Отправляем ответ клиенту
        conn.Send(new RedeemPromoResponse
        {
            success = true,
            message = $"Вы получили {option.amount} {option.type}",
            rewardType = option.type,
            amount = option.amount
        });
    }

    private bool ValidateCode(string code, PlayerData pd, out PromoCodeEntry entry, out string message)
    {
        entry = null;
        code = code.Trim().ToUpperInvariant();
        entry = promoCodes.Find(e => e.code.Equals(code, StringComparison.OrdinalIgnoreCase));
        if (entry == null)
        {
            message = "Промокод не найден.";
            return false;
        }
        if (pd.RedeemedPromoCodes.Contains(entry.code))
        {
            message = "Вы уже использовали этот промокод.";
            return false;
        }
        message = "OK";
        return true;
    }

    private void ApplyReward(PlayerData pd, PromoCodeEntry entry, RewardOption option)
    {
        switch (option.type)
        {
            case RewardType.Gold:
                pd.Gold += option.amount; break;
            case RewardType.Diamonds:
                pd.Diamonds += option.amount; break;
            case RewardType.Tokens:
                pd.TokenField += option.amount; break;
        }
        pd.RedeemedPromoCodes.Add(entry.code);
        ServerDataManager.Instance.SavePlayer(pd);
    }
}