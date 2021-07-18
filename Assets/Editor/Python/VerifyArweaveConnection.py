
import arweave
import sys
import clr
from arweave.arweave_lib import ArweaveTransactionException

# This is a template for making callbacks into our C# code
clr.AddReference('Rawrshak')
import UnityEngine
from Rawrshak import ArweaveSettings
from Rawrshak import SettingsManager

arweaveSettings = None
settingsManager = None

# arweaveSettings = UnityEngine.Object.FindObjectOfType(ArweaveSettings)
settingsManager = UnityEngine.Object.FindObjectOfType(SettingsManager)

if settingsManager == None:
    settingsManager.AddErrorHelpbox("No Settings Manager Found.")
    quit()

arweaveSettings = settingsManager.mArweaveSettings  

if arweaveSettings == None:
    settingsManager.AddErrorHelpbox("No Arweave Settings Found.")
    quit()

try:
    wallet_file_path = arweaveSettings.arweaveWalletFile
    # print(wallet_file_path)
    wallet = arweave.Wallet(wallet_file_path)
    # wallet.api_url = "http://localhost:1984"
    wallet.api_url = arweaveSettings.arweaveGatewayUri
    # print(wallet.api_url)
    # print(wallet.address)
    arweaveSettings.wallet = wallet.address
    # settingsManager.AddErrorHelpbox(wallet.address)
    print(str(wallet.balance))
except ArweaveTransactionException as ae:
    print(ae.message)
    settingsManager.AddErrorHelpbox(ae.message)
except Exception as e:
    # print("Error ", e.__class__, ", [Cause]: ", e.msg)
    # print(e.__class__)
    # print(sys.exc_info())
    settingsManager.AddErrorHelpbox(str(sys.exc_info()[0]) + "\n" + str(sys.exc_info()[1]))
