// DeviceAuthenticator.cs
using System;
using System.Linq;
using Mirror;
using UnityEngine;

#region Messages
public struct RegisterRequestMessage : NetworkMessage
{
    public string email;
    public string nickname;
    public string passwordHash;
}

public struct RegisterResponseMessage : NetworkMessage
{
    public bool success;
    public string message;
}

public struct LoginRequestMessage : NetworkMessage
{
    public string nickname;
    public string passwordHash;
    public string deviceId;
    public bool rememberMe;
}

public struct LoginResponseMessage : NetworkMessage
{
    public bool success;
    public string message;
    public string playerJson;
}

public struct AutoLoginRequestMessage : NetworkMessage
{
    public string deviceId;
}
#endregion

public enum AuthType { None, Login, Auto }

public static class AuthRequestData
{
    public static AuthType Type;
    public static string Nickname;
    public static string Password;
    public static bool RememberMe;
}

public class DeviceAuthenticator : NetworkAuthenticator
{
    // SERVER SIDE

    public override void OnStartServer()
    {
        NetworkServer.RegisterHandler<RegisterRequestMessage>(OnRegisterRequest, false);
        NetworkServer.RegisterHandler<LoginRequestMessage>(OnLoginRequest, false);
        NetworkServer.RegisterHandler<AutoLoginRequestMessage>(OnAutoLoginRequest, false);
    }

    public override void OnServerAuthenticate(NetworkConnectionToClient conn)
    {
        // waiting for Register/Login/AutoLogin
    }

    private void OnRegisterRequest(NetworkConnectionToClient conn, RegisterRequestMessage msg)
    {
        var dm = ServerDataManager.Instance;

        if (dm.accountsDb.accounts.Any(a => a.email == msg.email))
        {
            conn.Send(new RegisterResponseMessage
            {
                success = false,
                message = "Email already in use"
            });
            return;
        }

        if (dm.accountsDb.accounts.Any(a => a.nickname == msg.nickname))
        {
            conn.Send(new RegisterResponseMessage
            {
                success = false,
                message = "Nickname already in use"
            });
            return;
        }

        var newGuid = Guid.NewGuid().ToString();
        var entry = new AccountEntry
        {
            guid = newGuid,
            email = msg.email,
            nickname = msg.nickname,
            passwordHash = msg.passwordHash,
            isOnline = false,
            deviceId = ""
        };
        dm.accountsDb.accounts.Add(entry);
        dm.SaveAccounts();

        // create player file with defaults
        dm.LoadOrCreatePlayer(newGuid, msg.email, msg.nickname);

        conn.Send(new RegisterResponseMessage
        {
            success = true,
            message = "Registered successfully"
        });
    }

    private void OnLoginRequest(NetworkConnectionToClient conn, LoginRequestMessage msg)
    {
        var dm = ServerDataManager.Instance;
        var entry = dm.accountsDb.accounts
                     .FirstOrDefault(a => a.nickname == msg.nickname);

        if (entry == null || entry.passwordHash != msg.passwordHash)
        {
            conn.Send(new LoginResponseMessage
            {
                success = false,
                message = "Incorrect username or password"
            });
            return;
        }

        if (entry.isOnline)
        {
            conn.Send(new LoginResponseMessage
            {
                success = false,
                message = "This account is already logged in"
            });
            return;
        }

        if (msg.rememberMe)
            entry.deviceId = msg.deviceId;

        entry.isOnline = true;
        dm.SaveAccounts();

        if (NetworkManager.singleton is CustomNetworkManager nm)
            nm.connectionToGuid[conn] = entry.guid;

        var pd = dm.LoadOrCreatePlayer(entry.guid);
        conn.Send(new LoginResponseMessage
        {
            success = true,
            playerJson = JsonUtility.ToJson(pd, true)
        });

        ServerAccept(conn);
    }

    private void OnAutoLoginRequest(NetworkConnectionToClient conn, AutoLoginRequestMessage msg)
    {
        var dm = ServerDataManager.Instance;
        var entry = dm.accountsDb.accounts
                     .FirstOrDefault(a => a.deviceId == msg.deviceId);

        if (entry == null || entry.isOnline)
        {
            conn.Send(new LoginResponseMessage
            {
                success = false,
                message = "Auto-login not possible"
            });
            return;
        }

        entry.isOnline = true;
        dm.SaveAccounts();

        if (NetworkManager.singleton is CustomNetworkManager nm)
            nm.connectionToGuid[conn] = entry.guid;

        var pd = dm.LoadOrCreatePlayer(entry.guid);
        conn.Send(new LoginResponseMessage
        {
            success = true,
            playerJson = JsonUtility.ToJson(pd, true)
        });

        ServerAccept(conn);
    }

    // CLIENT SIDE

    public override void OnStartClient()
    {
        NetworkClient.RegisterHandler<LoginResponseMessage>(OnClientLoginResponse, false);
    }

    public override void OnClientAuthenticate()
    {
        switch (AuthRequestData.Type)
        {
            case AuthType.Login:
                NetworkClient.Send(new LoginRequestMessage
                {
                    nickname = AuthRequestData.Nickname,
                    passwordHash = HashUtility.SHA512(AuthRequestData.Password),
                    deviceId = SystemInfo.deviceUniqueIdentifier,
                    rememberMe = AuthRequestData.RememberMe
                });
                break;

            case AuthType.Auto:
                NetworkClient.Send(new AutoLoginRequestMessage
                {
                    deviceId = SystemInfo.deviceUniqueIdentifier
                });
                break;

            case AuthType.None:
            default:
                break;
        }
    }

    private void OnClientLoginResponse(LoginResponseMessage msg)
    {
        if (!msg.success)
        {
            ClientReject();
            AuthUIController.Instance.ShowLoginPanel(msg.message);
            return;
        }

        var pd = JsonUtility.FromJson<PlayerData>(msg.playerJson);
        ClientGameState.Instance.Initialize(pd);
        ClientAccept();
    }
}