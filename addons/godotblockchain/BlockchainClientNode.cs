using System.Threading.Tasks;
using Godot;
using Thirdweb;
using System.Text;

namespace BIGConnect.addons.godotblockchain;

[GlobalClass,Tool]
public partial class BlockchainClientNode : Node
{
	[Export]
	public BlockchainClientConfigurationResource clientConfiguration { get; internal set; }
	
	[Signal]
	public delegate void BlockchainClientReadyEventHandler();
	
	[Signal]
	public delegate void BlockchainClientInitiatizedEventHandler();
	
	[Signal]
	public delegate void AwaitingOTPEventHandler();
	
	[Signal]
	public delegate void InAppWalletCreatedEventHandler( string inAppWalletAddress );
	
	[Signal]
	public delegate void SmartWalletCreatedEventHandler( string smartWalletAddress );

	[Signal]
	public delegate void SmartWalletCreationFailedEventHandler();	

	[Signal]
	public delegate void BlockchainClientIPFSUploadCompletedEventHandler( string ipfsURI );

	[Signal]
	public delegate void BlockchainClientIPFSDownloadCompletedEventHandler( byte[] downloadedData );
	
	[Signal]
	public delegate void ClientLogMessageEventHandler( string logMessage );

	public ThirdwebClient internalClient { get; internal set; }
	public InAppWallet inAppWallet { get; internal set; }
	public SmartWallet smartWallet { get; internal set; }

	public static BlockchainClientNode Instance { get; private set; }

	public void Log( string message )
	{
		EmitSignal(SignalName.ClientLogMessage, "BlockchainClientNode: " + message );
		BlockchainLogManager.Instance.EmitLog("BlockchainClientNode: " + message);
	}

	public override void _Ready()
	{
		// emit a signal so systems will know that we are ready
		//
		EmitSignal(SignalName.BlockchainClientReady);				
		
		Initialize();
	}

	public void Initialize()
	{
		Log("Initializing BlockchainClientNode");

		if (Instance is not null)
		{
			Log("Multiple instances of BlockchainClientNode are not allowed");
			return;
		}
		else
		{
			Instance = this;
			Log("BlockchainClientNode initialized");
		}

		if (clientConfiguration is null )
		{
			Log("Create a ClientConfiguration resource and assign it to the BlockchainClient");
		}


		Log("Starting client with " + clientConfiguration.clientId + " and bundleId " + clientConfiguration.bundleId );
		
		// create a ThirdwebClient based on the exported chainID and bundleId
		internalClient = ThirdwebClient.Create(
			clientId: clientConfiguration.clientId,
			bundleId: clientConfiguration.bundleId
		);

		if (OS.GetName() == "Android")
		{
			OS.RequestPermissions();
		}

		AddToGroup("Blockchain", true);	

		// emit a signal so systems will know that we are ready
		//
		EmitSignal(SignalName.BlockchainClientInitiatized);
	}
	
	public async void OnStartLogin( string emailAddress )
	{
		Log("Starting login for " + emailAddress);
			
		inAppWallet = await InAppWallet.Create(client: internalClient, email: emailAddress);

		if (!await inAppWallet.IsConnected())
		{
			Log("Sending OTP Challenge");
			await inAppWallet.SendOTP();
			Log( emailAddress + " sent OTP challenge for wallet access" );
			EmitSignal(SignalName.AwaitingOTP);
		}		
		else
		{
			Log("InAppWallet already connected logging in...");
			EmitSignal(SignalName.InAppWalletCreated, await inAppWallet.GetAddress() );
			Log(await inAppWallet.GetAddress());
		}	
	}

	public async Task<bool> OnOTPSubmit( string otp )
	{
		Log("Submitting OTP " + otp);

		var (address, canRetry) = await inAppWallet.LoginWithOtp(otp);

		if (address != null)
		{
			Log($"Address: {address}");
			return true;
		}
		else
		{
			Log("Invalid OTP. Try again.");
			return false;
		}
	}

	public async void CreateSmartWallet()
	{
		smartWallet	= await SmartWallet.Create(
			personalWallet: inAppWallet,
			factoryAddress: clientConfiguration.walletFactoryAddress,
			gasless: clientConfiguration.isGasless,
			chainId: clientConfiguration.chainId
		);

		if (smartWallet != null)
		{
			Log($"SmartWallet address: {await smartWallet.GetAddress()}");		
			EmitSignal(SignalName.SmartWalletCreated, await smartWallet.GetAddress());
		}
		else
		{
			Log("SmartWallet creation failed");
			EmitSignal(SignalName.SmartWalletCreationFailed);
		}
	}

	public async void IPFSUpload( byte[] uploadData )
	{
		IPFSUploadResult result = await ThirdwebStorage.UploadRaw(BlockchainClientNode.Instance.internalClient, uploadData);
		var ipfsURI = result.IpfsHash;

		BlockchainLogManager.Instance.EmitLog("Uploaded to IPFS: " + ipfsURI);
		BlockchainLogManager.Instance.EmitLog("Preview available at" + result.PreviewUrl);

		// emit a signal so systems will know that we are done uploading
		EmitSignal(SignalName.BlockchainClientIPFSUploadCompleted, ipfsURI);			
	}
	
	public async Task<byte[]> IPFSDownload( string ipfsURI )
	{
		var downloadedContent = await ThirdwebStorage.Download<string>(BlockchainClientNode.Instance.internalClient, ipfsURI);		
		byte[] downloadedData = Encoding.UTF8.GetBytes(downloadedContent);

		// emit a signal so systems will know that we are done downloading
		EmitSignal(SignalName.BlockchainClientIPFSDownloadCompleted, downloadedData );

		return downloadedData;
	}     	

}