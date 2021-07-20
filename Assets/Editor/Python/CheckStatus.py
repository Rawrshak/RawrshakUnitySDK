import arweave
import sys
import clr
from arweave.arweave_lib import Wallet, Transaction, ArweaveTransactionException
from arweave.transaction_uploader import TransactionUploaderException, get_uploader

# This is a template for making callbacks into our C# code
clr.AddReference('Rawrshak')
import UnityEngine
from Rawrshak import ArweaveSettings, AssetBundleManager

# Note:
# SIGPIPE Fails in transaction_uploader so I went into transaction_uploader.py
# and replaced SIGPIPE with a try catch
# Library\PythonInstall\Lib\site-packages\arweave\transaction_uploader.py

# try:
#     from signal import signal, SIGPIPE, SIG_DFL
#     signal(SIGPIPE, SIG_DFL)
# except ImportError:  # If SIGPIPE is not available (win32),
#     pass 

assetBundleManager = UnityEngine.Object.FindObjectOfType(AssetBundleManager)
arweaveSettings = UnityEngine.Object.FindObjectOfType(ArweaveSettings)

if assetBundleManager == None:
    print("Error: No Asset Bundle Manager Found.")
    quit()

arweaveSettings = assetBundleManager.mArweaveSettings

try:
    wallet = arweave.Wallet(arweaveSettings.arweaveWalletFile)
    # wallet.api_url = arweaveSettings.arweaveGatewayUri
    filename = arweaveSettings.bundleForUpload.mName
    fileLocation = arweaveSettings.bundleForUpload.mFileLocation

    # print(filename)
    # print(fileLocation)
    # print("Wallet API: " + wallet.api_url)
    
    transaction = Transaction(wallet, id=arweaveSettings.bundleForUpload.mTransactionId)
    # print("Transaction Id: " + transaction.id)
    status = transaction.get_status()
    
    if status == "PENDING":
        arweaveSettings.bundleForUpload.mStatus = "Pending"
    else:
        arweaveSettings.bundleForUpload.mStatus = "Ready"
        arweaveSettings.bundleForUpload.mNumOfConfirmations = status['number_of_confirmations']

    # print(status)
    
except TransactionUploaderException as tue:
    print(tue.args)
    assetBundleManager.AddErrorHelpbox(tue.args)
except ArweaveTransactionException as ae:
    print(ae.args)
    assetBundleManager.AddErrorHelpbox(ae.args)
except Exception as e:
    print(e.args)
    assetBundleManager.AddErrorHelpbox(e.args)
    # assetBundleManager.AddErrorHelpbox(str(sys.exc_info()[0]) + "\n" + str(sys.exc_info()[1]))

