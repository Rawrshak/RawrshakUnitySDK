using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using WalletConnectSharp;
using WalletConnectSharp.Models;
using UnityEngine.Events;

using ZXing;
using ZXing.QrCode;

using System;
using System.Threading;
using System.Threading.Tasks;

using Rawrshak;
using Nethereum.Web3;
using Nethereum.JsonRpc.Client;

public class WalletConnectManager : ScriptableObject {
    
    // Client Metadata information
    public string mDescription;
    public string mIconUri;
    public string mName;
    public string mUrl;
    
    // WalletConnect Info
    public Texture2D mQrCodeTexture;
    private WalletConnect mWalletConnect;
    private WCSessionData mSessionData;
    private string mPublicKey;

    // Callbacks
    private UnityEvent<WalletType> onWalletLoad = new UnityEvent<WalletType>();
    private UnityEvent<string> onWalletLoadError = new UnityEvent<string>();

    public void Init() {
        // Store this somewhere somehow
        mDescription = "Rawrshak Unity SDK Connection";
        mIconUri = "https://rawrshak.io/favicon.ico";
        mName = "Rawrshak Unity SDK";
        mUrl = "https://rawrshak.io";

        
        // Generate Client Metadata        
        var metadata = new ClientMeta()
        {
            Description = mDescription,
            Icons = new[] {mIconUri},
            Name = mName,
            URL = mUrl
        };
        mWalletConnect = new WalletConnect(metadata);

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
        
        mQrCodeTexture = new Texture2D(256,256);
        mQrCodeTexture.SetPixels32(writer.Write(mWalletConnect.URI));
        mQrCodeTexture.Apply();
    }

    public void SetListeners(UnityAction<WalletType> onWalletLoadCallback, UnityAction<string> onWalletLoadErrorCallback)
    {
        onWalletLoad.AddListener(onWalletLoadCallback);
        onWalletLoadError.AddListener(onWalletLoadErrorCallback);
    }

    public void LoadUI(VisualElement root)
    {
        // Set QR Code
        root.Query<Image>("wallet-connect-qrcode").First().image = mQrCodeTexture;
        
        // Set Wallet Connect Button
        var walletConnectButton = root.Query<Button>("connect-wallet-button").First();
        walletConnectButton.clicked += async () => {
            Debug.Log("Waiting for WalletConnect...");
            await LoadWalletFromWalletConnect();
            Debug.Log("Wallet Connected!");
        };
    }
    
    public async Task LoadWalletFromWalletConnect() {
        // Once it's shown to the user, call walletConnect.Connect(). 
        // This will block.
        Debug.Log("Waiting for connection...");
        mSessionData = await mWalletConnect.Connect();
        Debug.Log("WalletConnect Connected!");


        Debug.Log(mSessionData.accounts[0]);
        Debug.Log(mSessionData.chainId);

        mPublicKey = mSessionData.accounts[0];
        onWalletLoad.Invoke(WalletType.WalletConnect);

        // Todo: uncomment this
        // When connecting with Nethereum: <replace infura URI with ethereum url>
    }

    public string GetPublicKey()
    {
        return mPublicKey;
    }

    public Web3 GetWeb3(string uri)
    {
        // Bug: This currently fails with the following compiler error
        //      error CS0012: The type 'IClient' is defined in an assembly that is 
        //      not referenced. You must add a reference to assembly 'Nethereum.JsonRpc.Client,
        //      Version=3.8.0.0, Culture=neutral, PublicKeyToken=8768a594786aba4e'.
        // For some reason, it doesn't like Nethereum.JsonRpc.Client that has the PublicKeyToken 
        // equal to null.
        
        // "https://mainnet.infura.io/v3/<infruaId>"
        // return new Web3(mWalletConnect.CreateProvider(new Uri(uri)));
        return null;
    }
}