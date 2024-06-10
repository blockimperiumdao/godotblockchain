#if TOOLS
using System.Threading.Tasks;
using Godot;
using Thirdweb;
using System.Numerics;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Signer;


public partial class BlockchainTokenNode : Node
{
	[Signal]
	public delegate void BlockchainTokenInitializedEventHandler();

    [Export]
    public BlockchainTokenResource tokenContractResource { get; internal set; }


    protected ThirdwebContract contract { get; private set; }

	public async void Initialize()
	{
		contract = await ThirdwebContract.Create(
			client: BlockchainManager.Instance.internalClient,
			address: tokenContractResource.contractAddress,
			chain: tokenContractResource.chainId
		);

        EmitSignal(SignalName.BlockchainTokenInitialized);
	}

	public async Task<BigInteger> Balance(string contractAddress, BigInteger chainId)
	{
		string myWalletAddress = await BlockchainClientNode.Instance.smartWallet.GetAddress();

		var result = await Balance( contractAddress, myWalletAddress, chainId );

		return result;
	}

	public async Task<BigInteger> Balance(string contractAddress, string queryAddress, BigInteger chainId)
	{
		BlockchainManager.Instance.EmitLog($"Creating contract -> {chainId}:{contractAddress} - balanceOf {queryAddress} being called");

		string readResult = await ThirdwebContract.Read<string>(contract, "name");
		BlockchainManager.Instance.EmitLog($"Contract result {readResult}");
		
		var balance = await ThirdwebContract.Read<BigInteger>(contract, "balanceOf", $"{queryAddress}");
		BlockchainManager.Instance.EmitLog($"Contract result {balance}");

		return balance;
	}

}

#endif