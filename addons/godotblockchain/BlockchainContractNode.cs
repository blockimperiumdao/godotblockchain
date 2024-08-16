using System.Threading.Tasks;
using Godot;
using Thirdweb;
using System.Numerics;

namespace BIGConnect.addons.godotblockchain;

[GlobalClass,Tool]
public partial class BlockchainContractNode : Node
{
	[Signal]
	public delegate void BlockchainContractInitializedEventHandler();

    [Export]
    public BlockchainContractResource ContractResource { get; internal set; }

    public ThirdwebContract InternalThirdwebContract { get; protected set; }

	public override void _Ready()
	{
		AddToGroup("Blockchain", true);
	}

    public async void Initialize()
    {
		InternalThirdwebContract = await ThirdwebContract.Create(
			client: BlockchainClientNode.Instance.internalClient,
			address: ContractResource.contractAddress,
			chain: ContractResource.chainId
		);

		// emit a signal so systems will know that we are ready
		//
		EmitSignal(SignalName.BlockchainContractInitialized);
    }    

	public async Task<string> Abi()
	{
		return await ThirdwebContract.FetchAbi(BlockchainClientNode.Instance.internalClient, ContractResource.contractAddress, ContractResource.chainId);	
	}

	public async Task<T> Read<T>(string methodName, params object[] args)
	{
		return await ThirdwebContract.Read<T>(InternalThirdwebContract, methodName, args);
	}

	public async Task<ThirdwebTransactionReceipt> Write(string methodName, BigInteger weiValue, params object[] args)
	{
		return await ThirdwebContract.Write(BlockchainClientNode.Instance.smartWallet, InternalThirdwebContract, methodName, weiValue, args);
	}

}