#if TOOLS
using System.Threading.Tasks;
using Godot;
using Thirdweb;
using System.Numerics;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Signer;


public partial class ERC20BlockchainContractNode : Node
{
	[Signal]
	public delegate void BlockchainContractInitializedEventHandler();

    [Export]
    public BlockchainContractResource contractResource { get; internal set; }

	public string tokenName;
	public string symbol;
	public BigInteger totalSupply;
	public BigInteger balanceOf;

    protected ThirdwebContract contract { get; private set; }
    public string currencyAddress { get; private set; }
    public BigInteger maxClaimable { get; private set; }
    public byte[] merkleRoot { get; private set; }
    public BigInteger tokenPrice { get; private set; }
    public BigInteger walletLimit { get; private set; }
    public BigInteger supplyClaimed { get; private set; }
	public int decimals { get; private set; }

    public async void Initialize()
    {
		contract = await ThirdwebContract.Create(
			client: BlockchainManager.Instance.internalClient,
			address: contractResource.contractAddress,
			chain: contractResource.chainId
		);

		Metadata();

		// emit a signal so systems will know that we are ready
		//
		EmitSignal(SignalName.BlockchainContractInitialized);
    }   

	public void Log( string message )
	{
		//EmitSignal(SignalName.ClientLogMessage, "ERC20BlockchainContractNode: " + message );
		BlockchainManager.Instance.EmitLog("ERC20BlockchainContractNode: " + message);
	} 

	// fills the node with the metadata from the Blockchain based on the active (i.e currently use) claim condition
    public async void Metadata()
    {
        var claimConditions = await contract.DropERC20_GetActiveClaimCondition( );

        currencyAddress = claimConditions.Currency;
        maxClaimable = claimConditions.MaxClaimableSupply;
        merkleRoot = claimConditions.MerkleRoot;
        tokenPrice = claimConditions.PricePerToken;
        walletLimit = claimConditions.QuantityLimitPerWallet;
        supplyClaimed = claimConditions.SupplyClaimed;
    }

	public async Task<BigInteger> BalanceOf(  )
	{
		balanceOf = await contract.ERC20_BalanceOf( await BlockchainClientNode.Instance.smartWallet.GetAddress() );
		return balanceOf;
	}

	public async Task<BigInteger> TotalSupply(  )
	{
		totalSupply = await contract.ERC20_TotalSupply();

		return totalSupply;
	}
	public async Task<string> TokenName(  )
	{
		tokenName = await contract.ERC20_Name();

		return tokenName;
	}

	public async Task<string> Symbol(  )
	{
		symbol = await contract.ERC20_Symbol();

		return symbol;
	}

	public async Task<int> Decimals(  )
	{
		decimals = await contract.ERC20_Decimals();

		return decimals;
	}

	public async Task<ThirdwebTransactionReceipt> Claim( string amount )
	{
		return await contract.DropERC20_Claim( BlockchainClientNode.Instance.smartWallet, await BlockchainClientNode.Instance.smartWallet.GetAddress(), amount );
	}	
	
	public async Task<BigInteger> Allowance( string owner, string spender )
	{
		return await contract.ERC20_Allowance( owner, spender );
	}

	public async Task<ThirdwebTransactionReceipt> Transfer( string toAddress, BigInteger amount )
	{
		return await contract.ERC20_Transfer( BlockchainClientNode.Instance.smartWallet, toAddress, amount );
	}

	public async Task<ThirdwebTransactionReceipt> TransferFrom( string fromAddress, string toAddress, BigInteger amount )
	{
		return await contract.ERC20_TransferFrom( BlockchainClientNode.Instance.smartWallet, fromAddress, toAddress, amount );
	}

}

#endif