using UnityEngine;
using TMPro;

public class PlayerStatsDisplay : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text _nicknameText;
    [SerializeField] private TMP_Text _levelText;
    [SerializeField] private TMP_Text _levelField;
    [SerializeField] private TMP_Text _goldText;
    [SerializeField] private TMP_Text _diamondText;
    [SerializeField] private TMP_Text _tokenField;

    private void Start()
    {
        UpdateStats();
    }

    public void UpdateStats()
    {
        var pd = ClientGameState.Instance.CurrentPlayerData;
        if (pd == null)
        {
            Debug.LogWarning("CurrentPlayerData == null. Проверьте, что логин прошёл успешно.");
            return;
        }

        _nicknameText.text = pd.Nickname;
        _levelText.text = pd.Level.ToString();
        _levelField.text = $"{pd.LevelField.ToString()}/{Constant.GetLevelThreshold(pd.Level)}";
        _goldText.text = pd.Gold.ToString("N0");
        _diamondText.text = pd.Diamonds.ToString("N0");
        _tokenField.text = $"{pd.TokenField.ToString()}/100";
    }
}

public static class Constant
{
    private static int[] _levelThresholdList = { 100, 250, 500, 650, 850, 1000, 1350, 1650, 2000, 2500 };

    public static int GetLevelThreshold(int level)
    {
        return _levelThresholdList[level - 1];
    }
}