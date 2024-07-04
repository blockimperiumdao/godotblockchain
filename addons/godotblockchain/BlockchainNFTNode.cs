#if TOOLS
using System.Threading.Tasks;
using Godot;
using Thirdweb;
using System.Numerics;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Signer;


public partial class BlockchainNFTNode : Node
{
	[Signal]
	public delegate void BlockchainNFTContractInitializedEventHandler();

	[Export]
	public BlockchainNFTResource nftContractResource { get; internal set; }

	protected ThirdwebContract contract { get; private set; }

	public async void Initialize()
	{
		contract = await ThirdwebContract.Create(
			client: BlockchainManager.Instance.internalClient,
			address: nftContractResource.contractAddress,
			chain: nftContractResource.chainId
		);

		EmitSignal(SignalName.BlockchainNFTContractInitialized);
	}     

	public async Task<ThirdwebTransactionReceipt> Claim( string contractAddress,  BigInteger quantity, BigInteger tokenPrice, string currencyAddress)
	{
		var smartWallet = BlockchainClientNode.Instance.smartWallet;
		var receiver = await smartWallet.GetAddress();
		var allowlistProof = new object[] { new byte[] { }, BigInteger.Zero, BigInteger.Zero, Constants.ADDRESS_ZERO };
		var data = new byte[] { };
		var result = await ThirdwebContract.Write(smartWallet, contract, "claim", 0, receiver, quantity, currencyAddress, tokenPrice, allowlistProof, data);
		var receipt = await ThirdwebTransaction.WaitForTransactionReceipt(BlockchainManager.Instance.internalClient, nftContractResource.chainId, result.TransactionHash);

		BlockchainManager.Instance.EmitLog(receipt.ToString());

		return receipt;
	} 

}

#endif
