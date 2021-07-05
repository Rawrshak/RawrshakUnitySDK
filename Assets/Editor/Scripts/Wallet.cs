using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using System;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using WalletConnectSharp;
using WalletConnectSharp.Models;
using System.Threading;
using System.Threading.Tasks;
using System.IO;

using ZXing;
using ZXing.QrCode;

using System.Text;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.KeyStore.Model;

public class Wallet : ScriptableObject {
    public enum ConnectionType {
        None,
        PrivateWallet,
        WalletConnect
    };

    // Private Key Account
    public string publicKey;
    public string keyStoreLocation;
    private Account account;
    private string password;

    // Client Metadata information
    string wcDescription;
    string wcIconUri;
    string wcName;
    string wcUrl;
    
    // WalletConnect Info
    public Texture2D qrCodeTexture;
    private WalletConnect walletConnect;
    private WCSessionData  wcSessionData;
    
    // Menu Callback
    private UnityEvent onWalletLoad = new UnityEvent();
    private UnityEvent<string> onWalletLoadError = new UnityEvent<string>();

    // Wallet Properties
    public ConnectionType connectionType;

    // Web3 
    public Web3 web3;

    public void Init(string description, string iconUri, string name, string url) {
        wcDescription = description;
        wcIconUri = iconUri;
        wcName = name;
        wcUrl = url;

        connectionType = ConnectionType.None;
        keyStoreLocation = "Assets/Editor/Data/Keystore/WalletKeyStore.json";
    }

    public void Awake()
    {
        // Generate Client Metadata        
        var metadata = new ClientMeta()
        {
            Description = wcDescription,
            Icons = new[] {wcIconUri},
            Name = wcName,
            URL = wcUrl
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

    public void LoadWalletFromKeyStore(string pword) {
        try {
            account = Nethereum.Web3.Accounts.Account.LoadFromKeyStoreFile(keyStoreLocation, pword);
        } catch (Exception ex) {
            Debug.LogError(ex.Message);
            onWalletLoadError.Invoke(ex.Message);
            return;
        }
        password = pword;
        publicKey = account.Address;
        web3 = new Nethereum.Web3.Web3(account);
        connectionType = ConnectionType.PrivateWallet;
        onWalletLoad.Invoke();
    }

    public void LoadWalletFromPrivateKey(string privateKey) {        
        // using private key
        try {
            account = new Account(privateKey);
        } catch (Exception ex) {
            Debug.LogError(ex.Message);
            onWalletLoadError.Invoke("Error: Private Key Invalid.");
            return;
        }
        publicKey = account.Address;
        web3 = new Nethereum.Web3.Web3(account);
        connectionType = ConnectionType.PrivateWallet;
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
        connectionType = ConnectionType.WalletConnect;
        onWalletLoad.Invoke();

        // Todo: uncomment this
        // When connecting with Nethereum: <replace infura URI with ethereum url>
        // var web3 = new Web3(walletConnect.CreateProvider(new Uri("https://mainnet.infura.io/v3/<infruaId>"));
    }

    public void SaveWalletToFile(string newPassword) {
        if (account == null) {
            onWalletLoadError.Invoke("Error: No Wallet Loaded.");
            return;
        }

        try {
            var keyStoreService = new Nethereum.KeyStore.KeyStoreScryptService();
            var scryptParams = new ScryptParams {Dklen = 32, N = 262144, R = 1, P = 8};
            
            var keyStore = keyStoreService.EncryptAndGenerateKeyStore(newPassword, account.PrivateKey.HexToByteArray(), account.Address, scryptParams);
            var json = keyStoreService.SerializeKeyStoreToJson(keyStore);

            // Delete file first
            File.Delete(keyStoreLocation);
            File.Delete(keyStoreLocation + ".meta");
            AssetDatabase.Refresh();

            // write file 
            StreamWriter writer = new StreamWriter(keyStoreLocation, true);
            writer.WriteLine(json);
            writer.Close();

            // keyStoreLocation = json;
            password = newPassword;
        } catch (Exception ex) {
            Debug.LogError(ex.Message);
            onWalletLoadError.Invoke(ex.Message);
        }
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