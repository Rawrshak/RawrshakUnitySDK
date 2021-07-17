
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Events;
using System;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using System.Threading;
using System.Threading.Tasks;
using System.IO;

using System.Text;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.KeyStore.Model;

using Rawrshak;

namespace Rawrshak
{
    public class PrivateWalletManager : ScriptableObject {

        // Private Key Account
        private string mPublicKey;
        public Web3 mWeb3;
        private Account mAccount;
        private string mStoredPassword;
        // Callbacks
        private UnityEvent<WalletType> onWalletLoad = new UnityEvent<WalletType>();
        private UnityEvent<string> onWalletLoadError = new UnityEvent<string>();

        public void Init()
        {
        }

        public void SetListeners(UnityAction<WalletType> onWalletLoadCallback, UnityAction<string> onWalletLoadErrorCallback)
        {
            onWalletLoad.AddListener(onWalletLoadCallback);
            onWalletLoadError.AddListener(onWalletLoadErrorCallback);
        }

        public string GetPublicKey()
        {
            return mPublicKey;
        }

        public void LoadUI(VisualElement root, string defaultKeystoreLocation)
        {
            var loadWalletButton = root.Query<Button>("load-wallet-button").First();
            var privateKeyTextField = root.Query<TextField>("private-key-field").First();
            var keystoreLocation = root.Query<TextField>("keystore-location").First();
            var password = root.Query<TextField>("password").First();
            var newPassword = root.Query<TextField>("new-password").First();
            var keystoreLoadButton = root.Query<Button>("load-keystore-wallet-button").First();
            
            // Set Password field settings
            password.isPasswordField = true;
            password.maskChar = '*';
            newPassword.isPasswordField = true;
            newPassword.maskChar = '*';
            
            // Load Wallet from Keystore 
            keystoreLocation.value = defaultKeystoreLocation;
            keystoreLoadButton.clicked += () => {
                if (String.IsNullOrEmpty(password.value) || String.IsNullOrEmpty(keystoreLocation.value)) {
                    Debug.Log("shit is null");
                    onWalletLoadError.Invoke("Error: Empty Keystore location or password.");
                    return;
                }
                var pword = password.value;
                password.value = String.Empty;
                LoadWalletFromKeyStore(keystoreLocation.value, pword);
            };

            // Set up Load Wallet and Save to file Button
            loadWalletButton.clicked += () => {
                if (String.IsNullOrEmpty(newPassword.value)) {
                    onWalletLoadError.Invoke("Error: Password cannot be empty.");
                    return;
                }
                LoadWalletFromPrivateKey(privateKeyTextField.value);
                SaveWalletToFile(keystoreLocation.value, newPassword.value);
                privateKeyTextField.value = String.Empty;
                newPassword.value = String.Empty;
            };
        }

        public void LoadWalletFromKeyStore(string keyStoreLocation, string password) {
            try {
                mAccount = Nethereum.Web3.Accounts.Account.LoadFromKeyStoreFile(keyStoreLocation, password);
            } catch (Exception ex) {
                Debug.LogError(ex.Message);
                onWalletLoadError.Invoke(ex.Message);
                return;
            }
            // Todo: add a toggle for whether or not to store password
            mStoredPassword = password;
            mPublicKey = mAccount.Address;
            mWeb3 = new Nethereum.Web3.Web3(mAccount);
            onWalletLoad.Invoke(WalletType.PrivateWallet);
        }

        public void LoadWalletFromPrivateKey(string privateKey) {        
            // using private key
            try {
                mAccount = new Account(privateKey);
            } catch (Exception ex) {
                Debug.LogError(ex.Message);
                onWalletLoadError.Invoke("Error: Private Key Invalid.");
                return;
            }
            mPublicKey = mAccount.Address;
            mWeb3 = new Nethereum.Web3.Web3(mAccount);
            onWalletLoad.Invoke(WalletType.PrivateWallet);
        }

        public void SaveWalletToFile(string keyStoreLocation, string newPassword) {
            if (mAccount == null) {
                onWalletLoadError.Invoke("Error: No Wallet Loaded.");
                return;
            }

            try {
                var keyStoreService = new Nethereum.KeyStore.KeyStoreScryptService();
                var scryptParams = new ScryptParams {Dklen = 32, N = 262144, R = 1, P = 8};
                
                var keyStore = keyStoreService.EncryptAndGenerateKeyStore(newPassword, mAccount.PrivateKey.HexToByteArray(), mAccount.Address, scryptParams);
                var json = keyStoreService.SerializeKeyStoreToJson(keyStore);

                // Delete file and .meta filefirst
                File.Delete(keyStoreLocation);
                File.Delete(keyStoreLocation + ".meta");

                // write file 
                StreamWriter writer = new StreamWriter(keyStoreLocation, true);
                writer.WriteLine(json);
                writer.Close();
                AssetDatabase.Refresh();

                // save password
                mStoredPassword = newPassword;
            } catch (Exception ex) {
                Debug.LogError(ex.Message);
                onWalletLoadError.Invoke(ex.Message);
            }
        }

        public Web3 GetWeb3(string uri)
        {
            return new Web3(mAccount, uri);
        }
    }
}