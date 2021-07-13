using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

public class ArweaveSettings : ScriptableObject
{
    public string arweaveGatewayUri;
    public string arweaveWalletFile;

    public void Init()
    {
        arweaveGatewayUri = "http://arweave.net";
        arweaveWalletFile = "/Asset/WalletFile";
    }
}
