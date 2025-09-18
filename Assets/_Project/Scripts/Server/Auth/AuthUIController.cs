using System.Collections;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AuthUIController : MonoBehaviour
{
    public static AuthUIController Instance;

    [Header("Registration Panel")]
    [SerializeField] private GameObject regPanel;
    [SerializeField] private TMP_InputField regEmail;
    [SerializeField] private TMP_InputField regNickname;
    [SerializeField] private TMP_InputField regPassword;
    [SerializeField] private Button regButton;
    [SerializeField] private TMP_Text regFeedback;

    [Header("Login Panel")]
    [SerializeField] private GameObject loginPanel;
    [SerializeField] private TMP_InputField loginNickname;
    [SerializeField] private TMP_InputField loginPassword;
    [SerializeField] private Toggle rememberToggle;
    [SerializeField] private Button loginButton;
    [SerializeField] private TMP_Text loginFeedback;

    private string _pendingRegEmail;
    private string _pendingRegNickname;
    private string _pendingRegPassword;
    private bool _isWaitingForRegister;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        NetworkClient.RegisterHandler<RegisterResponseMessage>(OnRegisterResponse, false);
        NetworkClient.RegisterHandler<LoginResponseMessage>(OnLoginResponse, false);
    }

    private void Start()
    {
        if (PlayerPrefs.GetInt("RememberMe", 0) == 1)
        {
            loginFeedback.text = "Attempting auto-login...";
            AuthRequestData.Type = AuthType.Auto;
            NetworkManager.singleton.StartClient();
            return;
        }

        regButton.onClick.AddListener(OnRegisterClicked);
        loginButton.onClick.AddListener(OnLoginClicked);
    }

    public void ShowLoginPanel(string message = "")
    {
        regPanel.SetActive(false);
        loginPanel.SetActive(true);
        loginFeedback.text = message;
    }

    public void ShowRegisterPanel(string message = "")
    {
        loginPanel.SetActive(false);
        regPanel.SetActive(true);
        regFeedback.text = message;
    }

    private void OnRegisterClicked()
    {
        if (string.IsNullOrEmpty(regEmail.text) ||
            string.IsNullOrEmpty(regNickname.text) ||
            string.IsNullOrEmpty(regPassword.text))
        {
            regFeedback.text = "Please fill out all fields";
            return;
        }

        regFeedback.text = "Registering...";
        _isWaitingForRegister = true;
        _pendingRegEmail = regEmail.text;
        _pendingRegNickname = regNickname.text;
        _pendingRegPassword = regPassword.text;

        if (!NetworkClient.isConnected)
        {
            NetworkManager.singleton.StartClient();
            StartCoroutine(WaitAndSendRegister());
        }
        else
        {
            SendRegister();
        }
    }

    private IEnumerator WaitAndSendRegister()
    {
        while (!NetworkClient.isConnected)
            yield return null;
        SendRegister();
    }

    private void SendRegister()
    {
        var msg = new RegisterRequestMessage
        {
            email = _pendingRegEmail,
            nickname = _pendingRegNickname,
            passwordHash = HashUtility.SHA512(_pendingRegPassword)
        };
        NetworkClient.Send(msg);
    }

    private void OnRegisterResponse(RegisterResponseMessage msg)
    {
        if (!_isWaitingForRegister) return;
        _isWaitingForRegister = false;

        Debug.Log($"[AuthUI] RegisterResponse: success={msg.success}, msg={msg.message}");
        regFeedback.text = msg.message;
        if (!msg.success) return;

        loginFeedback.text = "Registered! Logging in…";
        AuthRequestData.Type = AuthType.Login;
        AuthRequestData.Nickname = _pendingRegNickname;
        AuthRequestData.Password = _pendingRegPassword;
        AuthRequestData.RememberMe = false;

        var loginMsg = new LoginRequestMessage
        {
            nickname = _pendingRegNickname,
            passwordHash = HashUtility.SHA512(_pendingRegPassword),
            deviceId = SystemInfo.deviceUniqueIdentifier,
            rememberMe = false
        };
        NetworkClient.Send(loginMsg);
    }

    private void OnLoginClicked()
    {
        if (string.IsNullOrEmpty(loginNickname.text) ||
            string.IsNullOrEmpty(loginPassword.text))
        {
            loginFeedback.text = "Please enter username and password";
            return;
        }

        AuthRequestData.Type = AuthType.Login;
        AuthRequestData.Nickname = loginNickname.text;
        AuthRequestData.Password = loginPassword.text;
        AuthRequestData.RememberMe = rememberToggle.isOn;

        loginFeedback.text = "Logging in…";
        NetworkManager.singleton.StartClient();
    }

    private void OnLoginResponse(LoginResponseMessage msg)
    {
        Debug.Log($"[AuthUI] LoginResponse: success={msg.success}, msg={msg.message}");
        if (!msg.success)
        {
            ShowLoginPanel(msg.message);
            return;
        }

        PlayerPrefs.SetInt("RememberMe", rememberToggle.isOn ? 1 : 0);
        PlayerPrefs.Save();

    }
}