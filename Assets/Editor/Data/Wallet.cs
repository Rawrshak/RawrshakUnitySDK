using UnityEngine;
using UnityEngine.Events;
using System;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using WalletConnectSharp;
using WalletConnectSharp.Models;

// using WalletConnectSharp.Client.Nethereum;
// using WalletConnectSharp.Events;
// using WalletConnectSharp.Events.Request;
// using WalletConnectSharp.Network;

// Todo: Serialize and load from json file
public class Wallet : ScriptableObject {
    public string privateKey;

    private string publicKey;
    private Account account;
    
    private UnityEvent onWalletLoad = new UnityEvent();
    private UnityEvent<string> onWalletLoadError = new UnityEvent<string>();

    private WalletConnect walletConnect;

    public void Awake()
    {
        // Todo: load from json file
        // Generate WalletConnect
        
        var metadata = new ClientMeta()
        {
            Description = "Rawrshak Unity SDK Connection",
            Icons = new[] {"https://app.warriders.com/favicon.ico"},
            Name = "Rawrshak Unity SDK",
            URL = "https://app.warriders.com"
        };
        walletConnect = new WalletConnect(metadata);
    }
    public void OnDestroy() {
        // Todo: save to json file
    }

    public void ShowWalletConnectQRCode() {
        // Todo: turn this into a QR code
        Debug.Log(walletConnect.URI);
    }

    public void LoadWallet(int selectedWalletLoad) {
        // Todo: get public key given the private key
        if (selectedWalletLoad == 0) {
            // using private key
            try {
                account = new Account(privateKey);
                publicKey = account.Address;
            } catch (Exception ex) {
                Debug.Log(ex.Message);
                onWalletLoadError.Invoke("Error: Private Key Invalid.");
                return;
            }
        } else if (selectedWalletLoad == 1) {

            // Once it's shown to the user, call walletConnect.Connect(). 
            // This will block.
            // var walletConnectData = await walletConnect.Connect();

            // Debug.Log(walletConnectData.accounts[0]);
            // Debug.Log(walletConnectData.chainId);

            // When connecting with Nethereum: <replace infura URI with ethereum url>
            // var web3 = new Web3(walletConnect.CreateProvider(new Uri("https://mainnet.infura.io/v3/<infruaId>"));
        }
        onWalletLoad.Invoke();
    }

    public string getPublicKey() {
        return publicKey;
    }

    public void AddOnWalletLoadListner(UnityAction action)
    {
        onWalletLoad.AddListener(action);
    }

    public void AddOnWalletLoadErrorListner(UnityAction<string> action)
    {
        onWalletLoadError.AddListener(action);
    }
}