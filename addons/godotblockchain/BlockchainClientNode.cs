#if TOOLS
using System.Threading.Tasks;
using Godot;
using Thirdweb;
using System.Numerics;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Signer;


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
	public delegate void ClientLogMessageEventHandler( string logMessage );

	public ThirdwebClient internalClient { get; internal set; }
	public InAppWallet inAppWallet { get; internal set; }
	public SmartWallet smartWallet { get; internal set; }

	public static BlockchainClientNode Instance { get; private set; }

	public void Log( string message )
	{
		EmitSignal(SignalName.ClientLogMessage, "BlockchainClientNode: " + message );
		BlockchainManager.Instance.EmitLog("BlockchainClientNode: " + message);
	}

	public override void _Ready()
	{
		// emit a signal so systems will know that we are ready
		//
		EmitSignal(SignalName.BlockchainClientReady);		
		
		if (Instance != null)
		{
			Log("Multiple instances of BlockchainClientNode are not allowed");
			return;
		}
		else
		{
			Instance = this;
			Log("BlockchainClientNode initialized");
		}

		if (clientConfiguration == null )
		{
			Log("Create a ClientConfiguration resource and assign it to the BlockchainClient");
		}


		Log("Starting client with " + clientConfiguration.clientId + " and bundleId " + clientConfiguration.bundleId );
		
		// create a ThirdwebClient based on the exported chainID and bundleId
		internalClient = ThirdwebClient.Create(
			clientId: clientConfiguration.clientId,
			bundleId: clientConfiguration.bundleId
		);

		// emit a signal so systems will know that we are ready
		//
		EmitSignal(SignalName.BlockchainClientInitiatized);

		if (OS.GetName() == "Android")
		{
			OS.RequestPermissions();
		}
			
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

			if (smartWallet == null)
			{
				Log("Creating SmartWallet for account");
				CreateSmartWallet();
			}	
			else
			{
				Log("SmartWallet already created for account");
			}
		}	
	}

	public async void OnOTPSubmit( string otp )
	{
		Log("Submitting OTP " + otp);

		var (address, canRetry) = await inAppWallet.SubmitOTP(otp);

		if (address != null)
		{
			Log($"Address: {address}");
			CreateSmartWallet();
		}
		else
		{
			Log("Invalid OTP. Try again.");
		}
	}

	private async void CreateSmartWallet()
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

}

#endif
