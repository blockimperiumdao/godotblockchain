using System.Threading.Tasks;
using Godot;
using Thirdweb;
using System.Numerics;


namespace BIGConnect.addons.godotblockchain;


[GlobalClass,Tool]
public partial class BlockchainTokenNode : Node
{
	[Signal]
	public delegate void BlockchainTokenInitializedEventHandler();

    [Export]
    public BlockchainTokenResource tokenContractResource { get; internal set; }

    protected ThirdwebContract contract { get; private set; }

	public override void _Ready()
	{
		AddToGroup("Blockchain", true);
	}

	public async void Initialize()
	{
		contract = await ThirdwebContract.Create(
			client: BlockchainClientNode.Instance.internalClient,
			address: tokenContractResource.contractAddress,
			chain: tokenContractResource.chainId
		);

        EmitSignal(SignalName.BlockchainTokenInitialized);

		AddToGroup("Blockchain", true);
	}

	public async Task<BigInteger> Balance(string contractAddress, BigInteger chainId)
	{
		string myWalletAddress = await BlockchainClientNode.Instance.smartWallet.GetAddress();

		var result = await Balance( contractAddress, myWalletAddress, chainId );

		return result;
	}

	public async Task<BigInteger> Balance(string contractAddress, string queryAddress, BigInteger chainId)
	{
		BlockchainLogManager.Instance.EmitLog($"Creating contract -> {chainId}:{contractAddress} - balanceOf {queryAddress} being called");

		string readResult = await ThirdwebContract.Read<string>(contract, "name");
		BlockchainLogManager.Instance.EmitLog($"Contract result {readResult}");
		
		var balance = await ThirdwebContract.Read<BigInteger>(contract, "balanceOf", $"{queryAddress}");
		BlockchainLogManager.Instance.EmitLog($"Contract result {balance}");

		return balance;
	}

}