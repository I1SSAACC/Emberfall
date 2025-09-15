using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;

public class CustomNetworkManager : NetworkManager
{
    [Header("Startup")]
    [SerializeField] private bool autoStartServer = false;

    public override void Start()
    {
        base.Start();
        if (autoStartServer && !NetworkServer.active)
            StartServer();
    }

    // туловище дикшенари перенести сюда
    internal readonly Dictionary<NetworkConnectionToClient, string> connectionToGuid
        = new Dictionary<NetworkConnectionToClient, string>();

    public DeviceAuthenticator AuthenticatorComponent => GetComponent<DeviceAuthenticator>();

    public override void OnStartServer()
    {
        base.OnStartServer();
        // подставляем свой аутентификатор
        if (authenticator == null)
            authenticator = GetComponent<DeviceAuthenticator>();
    }

    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        // сначала сбрасываем флаг isOnline
        if (connectionToGuid.TryGetValue(conn, out var guid))
        {
            var dm = ServerDataManager.Instance;
            var entry = dm.accountsDb.accounts.FirstOrDefault(a => a.guid == guid);
            if (entry != null)
            {
                entry.isOnline = false;
                dm.SaveAccounts();
            }
            connectionToGuid.Remove(conn);
        }

        // потом отпускаем Mirror-логику
        base.OnServerDisconnect(conn);
    }
}
