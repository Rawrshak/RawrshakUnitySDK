using UnityEngine;
using UnityEngine.Events;
using System;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using WalletConnectSharp;
using WalletConnectSharp.Models;
using System.Threading;
using System.Threading.Tasks;

using ZXing;
using ZXing.QrCode;

// Todo: Serialize and load from json file
public class Wallet : ScriptableObject {
    private string publicKey;
    private Account account;
    
    private UnityEvent onWalletLoad = new UnityEvent();
    private UnityEvent<string> onWalletLoadError = new UnityEvent<string>();

    // WalletConnect Info
    public Texture2D qrCodeTexture;
    private WalletConnect walletConnect;
    private WCSessionData  wcSessionData;

    public void Awake()
    {
        // Todo: load from json file

        // Generate Client Metadata        
        var metadata = new ClientMeta()
        {
            Description = "Rawrshak Unity SDK Connection",
            Icons = new[] {"https://app.warriders.com/favicon.ico"},
            Name = "Rawrshak Unity SDK",
            URL = "https://app.warriders.com"
        };
        walletConnect = new WalletConnect(metadata);

        // Generate QR Code for WalletConnect     
        var writer = new BarcodeWriter()
        {
            Format = BarcodeFormat.QR_CODE,
            Options = new QrCodeEncodingOptions
            {
                Height = 256,
                Width = 256
            }
        };
        
        qrCodeTexture = new Texture2D(256,256);
        qrCodeTexture.SetPixels32(writer.Write(walletConnect.URI));
        qrCodeTexture.Apply();
    }
    public void OnDestroy() {
        // Todo: save to json file
    }

    public void LoadWalletFromPrivateKey(string privateKey) {        
        // using private key
        try {
            account = new Account(privateKey);
            publicKey = account.Address;
        } catch (Exception ex) {
            Debug.Log(ex.Message);
            onWalletLoadError.Invoke("Error: Private Key Invalid.");
            return;
        }
        onWalletLoad.Invoke();
    }

    public async Task LoadWalletFromWalletConnect() {
        // Once it's shown to the user, call walletConnect.Connect(). 
        // This will block.
        Debug.Log("Waiting for connection...");
        wcSessionData = await walletConnect.Connect();
        Debug.Log("WalletConnect Connected!");

        Debug.Log(wcSessionData.accounts[0]);
        Debug.Log(wcSessionData.chainId);

        publicKey = wcSessionData.accounts[0];
        onWalletLoad.Invoke();

        // When connecting with Nethereum: <replace infura URI with ethereum url>
        // var web3 = new Web3(walletConnect.CreateProvider(new Uri("https://mainnet.infura.io/v3/<infruaId>"));
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