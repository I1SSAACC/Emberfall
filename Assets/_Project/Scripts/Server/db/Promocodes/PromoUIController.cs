using System.Collections;
using Mirror;
using PromoCodes;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PromoUIController : MonoBehaviour
{
    [Header("Promo Input Panel")]
    [SerializeField] private GameObject _settingsPanel;
    [SerializeField] private GameObject inputPanel;
    [SerializeField] private TMP_InputField promoInput;
    [SerializeField] private Button redeemButton;
    [SerializeField] private TMP_Text feedbackText;

    [Header("Reward Panel")]
    [SerializeField] private CanvasGroup rewardGroup;      // same GameObject as reward panel
    [SerializeField] private Image rewardIcon;
    [SerializeField] private TMP_Text rewardAmountText;
    [SerializeField] private Button rewardOkButton;
    [SerializeField] private float fadeDuration = 0.5f;

    [Header("Reward Sprites")]
    [SerializeField] private Sprite goldSprite;
    [SerializeField] private Sprite diamondsSprite;
    [SerializeField] private Sprite tokensSprite;

    private void Awake()
    {
        NetworkClient.RegisterHandler<RedeemPromoResponse>(OnClientPromoResponse, false);
    }

    private void OnDestroy()
    {
        NetworkClient.UnregisterHandler<RedeemPromoResponse>();
    }

    private void Start()
    {
        redeemButton.onClick.AddListener(OnRedeemClicked);
        rewardOkButton.onClick.AddListener(OnOkClicked);

        // hide reward panel at start
        rewardGroup.gameObject.SetActive(false);
    }

    private void OnRedeemClicked()
    {
        feedbackText.text = "";
        string code = promoInput.text.Trim();
        if (ClientGameState.Instance.CurrentPlayerData == null)
        {
            feedbackText.text = "Please log in first.";
            return;
        }
        NetworkClient.Send(new RedeemPromoRequest { code = code });
    }

    private void OnClientPromoResponse(RedeemPromoResponse resp)
    {
        if (!resp.success)
        {
            feedbackText.text = resp.message;
            return;
        }

        // update local profile and HUD
        var pd = ClientGameState.Instance.CurrentPlayerData;
        switch (resp.rewardType)
        {
            case RewardType.Gold: pd.Gold += resp.amount; break;
            case RewardType.Diamonds: pd.Diamonds += resp.amount; break;
            case RewardType.Tokens: pd.TokenField += resp.amount; break;
        }
        FindObjectOfType<PlayerStatsDisplay>()?.UpdateStats();

        // prepare reward UI
        rewardAmountText.text = resp.amount.ToString();
        switch (resp.rewardType)
        {
            case RewardType.Gold: rewardIcon.sprite = goldSprite; break;
            case RewardType.Diamonds: rewardIcon.sprite = diamondsSprite; break;
            default: rewardIcon.sprite = tokensSprite; break;
        }

        // hide input panel
        inputPanel.SetActive(false);
        _settingsPanel.SetActive(false);

        // show & fade in reward panel
        var panelGO = rewardGroup.gameObject;
        panelGO.SetActive(true);
        rewardGroup.alpha = 0f;
        rewardGroup.interactable = false;
        rewardGroup.blocksRaycasts = false;

        StartCoroutine(FadeCanvasGroup(
            rewardGroup, 0f, 1f, fadeDuration,
            onComplete: () =>
            {
                rewardGroup.interactable = true;
                rewardGroup.blocksRaycasts = true;
            }));
    }

    private void OnOkClicked()
    {
        // fade out reward panel, then disable both panels
        rewardGroup.interactable = false;
        rewardGroup.blocksRaycasts = false;
        StartCoroutine(FadeCanvasGroup(
            rewardGroup, 1f, 0f, fadeDuration,
            onComplete: () =>
            {
                rewardGroup.gameObject.SetActive(false);
                inputPanel.SetActive(false);
                _settingsPanel.SetActive(false);
            }));
    }

    private IEnumerator FadeCanvasGroup(
        CanvasGroup cg,
        float from,
        float to,
        float duration,
        System.Action onComplete = null)
    {
        float t = 0f;
        cg.alpha = from;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            cg.alpha = Mathf.Lerp(from, to, t / duration);
            yield return null;
        }
        cg.alpha = to;
        onComplete?.Invoke();
    }
}