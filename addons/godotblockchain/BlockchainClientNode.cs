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
	
	[Export]
	private LineEdit emailEntry;
	
	[Export]
	private LineEdit otpEntry;
	
	
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
	public delegate void LogMessageEventHandler( string logMessage );

	public ThirdwebClient internalClient { get; internal set; }
	public InAppWallet inAppWallet { get; internal set; }
	public SmartWallet smartWallet { get; internal set; }

	public static BlockchainClientNode Instance { get; private set; }

	public override void _Ready()
	{
		if (Instance != null)
		{
			BlockchainManager.Instance.EmitLog("Multiple instances of BlockchainManager are not allowed");
			return;
		}
		else
		{
			Instance = this;
			BlockchainManager.Instance.EmitLog("BlockchainManager initialized");
		}

		if (clientConfiguration == null )
		{
			BlockchainManager.Instance.EmitLog("Create a ClientConfiguration resource and assign it to the BlockchainClient");
		}


		BlockchainManager.Instance.EmitLog("Starting client with " + clientConfiguration.clientId + " and bundleId " + clientConfiguration.bundleId );
		
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
		
		BlockchainClientNode.Instance.AwaitingOTP += SetStateAwaitingOTP;
		BlockchainClientNode.Instance.InAppWalletCreated += SetStateInAppWalletCreated;
		BlockchainClientNode.Instance.SmartWalletCreated += SetStateSmartWalletCreated;		
	}
	
	private void SetStateAwaitingOTP()
	{
		BlockchainManager.Instance.EmitLog("Awaiting OTP");
		
		otpEntry.Visible = true;
		emailEntry.Visible = false;
	}

	private void SetStateInAppWalletCreated( string address )
	{
		BlockchainManager.Instance.EmitLog("InAppWalletAddress " + address);
		
		otpEntry.Visible = false;
		emailEntry.Visible = false;
	}

	private void SetStateSmartWalletCreated( string address )
	{
		BlockchainManager.Instance.EmitLog("SmartWalletAddress " + address);
		
		otpEntry.Visible = false;
		emailEntry.Visible = false;
	}

	public async void OnStartLogin()
	{
		BlockchainManager.Instance.EmitLog("Submitting Email address");
	
		if (emailEntry == null)
		{
			BlockchainManager.Instance.EmitLog("You need to set the emailEntry field");
			return;
		}
		string emailAddress = emailEntry.Text;
		
		inAppWallet = await InAppWallet.Create(client: internalClient, email: emailAddress , authprovider: AuthProvider.Google);

		if (!await inAppWallet.IsConnected())
		{
			await inAppWallet.SendOTP();
			BlockchainManager.Instance.EmitLog( emailAddress + " submitted for wallet access" );
			EmitSignal(SignalName.AwaitingOTP);
		}		
		else
		{
			BlockchainManager.Instance.EmitLog("InAppWallet already connected");
			EmitSignal(SignalName.InAppWalletCreated, await inAppWallet.GetAddress() );
			BlockchainManager.Instance.EmitLog(await inAppWallet.GetAddress());

			if (smartWallet == null)
			{
				BlockchainManager.Instance.EmitLog("Creating SmartWallet");
				CreateSmartWallet();
			}	
			else
			{
				BlockchainManager.Instance.EmitLog("SmartWallet already created");
			}
		}	
	}

	public async void OnOTPSubmit()
	{
		BlockchainManager.Instance.EmitLog("Submitting OTP");
		
		if (otpEntry == null)
		{
			BlockchainManager.Instance.EmitLog("You must set the OTP entry field");
		}

		string otpInput = otpEntry.Text;
		var (address, canRetry) = await inAppWallet.SubmitOTP(otpInput);

		if (address != null)
		{
			BlockchainManager.Instance.EmitLog($"Address: {address}");
			CreateSmartWallet();
		}
		else
		{
			BlockchainManager.Instance.EmitLog("Invalid OTP");
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
			BlockchainManager.Instance.EmitLog($"SmartWallet address: {await smartWallet.GetAddress()}");		
			EmitSignal(SignalName.SmartWalletCreated, await smartWallet.GetAddress());
		}
		else
		{
			BlockchainManager.Instance.EmitLog("SmartWallet creation failed");
			EmitSignal(SignalName.SmartWalletCreationFailed);
		}
	}

}

#endif
