using System.Threading.Tasks;
using Godot;
using Thirdweb;
using System.Numerics;

[GlobalClass,Tool]
public partial class BlockchainContractNode : Node
{
	[Signal]
	public delegate void BlockchainContractInitializedEventHandler();

    [Export]
    public BlockchainContractResource contractResource { get; internal set; }

    public ThirdwebContract contract { get; protected set; }

	public override void _Ready()
	{
		AddToGroup("Blockchain", true);
	}

    public async void Initialize()
    {
		contract = await ThirdwebContract.Create(
			client: BlockchainClientNode.Instance.internalClient,
			address: contractResource.contractAddress,
			chain: contractResource.chainId
		);

		// emit a signal so systems will know that we are ready
		//
		EmitSignal(SignalName.BlockchainContractInitialized);
    }    

	public async Task<string> Abi()
	{
		return await ThirdwebContract.FetchAbi(BlockchainClientNode.Instance.internalClient, contractResource.contractAddress, contractResource.chainId);	
	}

	public async Task<T> Read<T>(string methodName, params object[] args)
	{
		return await ThirdwebContract.Read<T>(contract, methodName, args);
	}

	public async Task<ThirdwebTransactionReceipt> Write(string methodName, BigInteger weiValue, params object[] args)
	{
		return await ThirdwebContract.Write(BlockchainClientNode.Instance.smartWallet, contract, methodName, weiValue, args);
	}

}