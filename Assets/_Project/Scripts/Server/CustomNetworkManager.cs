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

    // �������� ��������� ��������� ����
    internal readonly Dictionary<NetworkConnectionToClient, string> connectionToGuid
        = new Dictionary<NetworkConnectionToClient, string>();

    public DeviceAuthenticator AuthenticatorComponent => GetComponent<DeviceAuthenticator>();

    public override void OnStartServer()
    {
        base.OnStartServer();
        // ����������� ���� ��������������
        if (authenticator == null)
            authenticator = GetComponent<DeviceAuthenticator>();
    }

    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        // ������� ���������� ���� isOnline
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

        // ����� ��������� Mirror-������
        base.OnServerDisconnect(conn);
    }
}
