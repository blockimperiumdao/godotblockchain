#if TOOLS
using System.Threading.Tasks;
using Godot;
using Thirdweb;
using System.Numerics;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Signer;


public partial class BlockchainManager : Node
{
	[Signal]
	public delegate void LogMessageEventHandler( string logMessage );

	public static BlockchainManager Instance { get; private set; }

	public ThirdwebClient internalClient { get; internal set; }

	public void EmitLog( string message )
	{
		GD.Print("BlockchainManager: " +  message );
		EmitSignal(SignalName.LogMessage, message);
	}

	public void EmitError( string message )
	{
		GD.PushError("BlockchainManager: " +  message );
		EmitSignal(SignalName.LogMessage, message);
	}

	public override void _Ready()
	{
		// listen for the intialization of the ThirdwebClient
		BlockchainClientNode.Instance.BlockchainClientInitiatized += () =>
		{
			internalClient = BlockchainClientNode.Instance.internalClient;
		};
	}

}

#endif
