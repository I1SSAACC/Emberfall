using System;
using System.Collections.Generic;

namespace PromoCodes
{
    // типы наград
    [Serializable]
    public enum RewardType
    {
        Gold,
        Diamonds,
        Tokens
        // Е другие, если нужно
    }

    // одна опци€ награды
    [Serializable]
    public class RewardOption
    {
        public RewardType type;
        public int amount;
    }

    // запись промокода
    [Serializable]
    public class PromoCodeEntry
    {
        public string code;
        public List<RewardOption> rewards = new List<RewardOption>();
    }

    // обЄртка дл€ сериализации списка
    [Serializable]
    public class PromoCodeList
    {
        public List<PromoCodeEntry> codes = new List<PromoCodeEntry>();
    }
}