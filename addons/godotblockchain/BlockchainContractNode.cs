#if TOOLS
using System.Threading.Tasks;
using Godot;
using Thirdweb;
using System.Numerics;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Signer;


public partial class BlockchainContractNode : Node
{
	[Signal]
	public delegate void BlockchainContractInitializedEventHandler();

    [Export]
    public BlockchainContractResource contractResource { get; internal set; }

    protected ThirdwebContract contract { get; private set; }

    public async void Initialize()
    {
		contract = await ThirdwebContract.Create(
			client: BlockchainManager.Instance.internalClient,
			address: contractResource.contractAddress,
			chain: contractResource.chainId
		);

		// emit a signal so systems will know that we are ready
		//
		EmitSignal(SignalName.BlockchainContractInitialized);
    }    

}

#endif