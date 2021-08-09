import arweave
import sys
import clr
from arweave.arweave_lib import Wallet, Transaction, ArweaveTransactionException
from arweave.transaction_uploader import TransactionUploaderException, get_uploader

# This is a template for making callbacks into our C# code
clr.AddReference('Rawrshak')
import UnityEngine
from Rawrshak import AssetBundleMenu

# Note:
# SIGPIPE Fails in transaction_uploader so I went into transaction_uploader.py
# and replaced SIGPIPE with a try catch
# Library\PythonInstall\Lib\site-packages\arweave\transaction_uploader.py

# try:
#     from signal import signal, SIGPIPE, SIG_DFL
#     signal(SIGPIPE, SIG_DFL)
# except ImportError:  # If SIGPIPE is not available (win32),
#     pass 

all_asset_bundle_menus = UnityEngine.Resources.FindObjectsOfTypeAll(AssetBundleMenu)

if all_asset_bundle_menus.Length == 0:
    print("Upload Manager was not found.")
    quit()

menu = all_asset_bundle_menus[0]
uploadConfig = menu.mUploadManager.mConfig

if uploadConfig == None:
    menu.AddErrorHelpbox("No Upload Config Found.")
    print("No Upload Config found.")
    quit()

try:
    wallet = arweave.Wallet(uploadConfig.walletFile)
    wallet.api_url = uploadConfig.gatewayUri
    filename = menu.mUploadManager.bundleForUpload.mName
    fileLocation = menu.mUploadManager.bundleForUpload.mFileLocation


    # print(filename)
    # print(fileLocation)
    # print("Wallet API: " + wallet.api_url)
    
    # Get the transaction's status
    transaction = Transaction(wallet, id=menu.mUploadManager.bundleForUpload.mTransactionId)
    status = transaction.get_status()
    
    # Update the status in the UI
    if status == "PENDING":
        menu.mUploadManager.bundleForUpload.mStatus = "Uploading..."
    else:
        menu.mUploadManager.bundleForUpload.mStatus = "Uploaded"
        menu.mUploadManager.bundleForUpload.mNumOfConfirmations = status['number_of_confirmations']
    
except TransactionUploaderException as tue:
    print(tue.args)
    menu.AddErrorHelpbox(tue.args)
except ArweaveTransactionException as ae:
    print(ae.args)
    menu.AddErrorHelpbox(ae.args)
except Exception as e:
    print(e.args)
    menu.AddErrorHelpbox(e.args)

