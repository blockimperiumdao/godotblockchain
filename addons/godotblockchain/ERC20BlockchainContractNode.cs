using System.Threading.Tasks;
using Godot;
using Thirdweb;
using System.Numerics;

[GlobalClass,Tool]
public partial class ERC20BlockchainContractNode : BlockchainContractNode
{
	[Signal]
	public delegate void ERC20BlockchainContractInitializedEventHandler();

	public string tokenName;
	public string symbol;
	public BigInteger totalSupply;
	public BigInteger balanceOf;

    public string currencyAddress { get; private set; }
    public BigInteger maxClaimable { get; private set; }
    public byte[] merkleRoot { get; private set; }
    public BigInteger tokenPrice { get; private set; }
    public BigInteger walletLimit { get; private set; }
    public BigInteger supplyClaimed { get; private set; }
	public int decimals { get; private set; }

	public override void _Ready()
	{
		if (contractResource == null)
		{
			Log("contractResource is null");
			return;
		}
		else
		{
			Log("initializing contract");
			Initialize();
		}

		AddToGroup("Blockchain", true);

	}

    public new async void Initialize()
    {
	    if ( ( contractResource == null ) || ( BlockchainClientNode.Instance == null ) )
        {
            Log("contractResource or BlockchainClientNode is null");
            return;
        }

		contract = await ThirdwebContract.Create(
			client: BlockchainClientNode.Instance.internalClient,
			address: contractResource.contractAddress,
			chain: contractResource.chainId
		);


		// emit a signal so systems will know that we are ready
		//
		if (contract != null )
		{
			EmitSignal(SignalName.ERC20BlockchainContractInitialized);

		}		
    }   

	public void Log( string message )
	{
		BlockchainLogManager.Instance.EmitLog("ERC20BlockchainContractNode: " + message);
	} 

	// fills the node with the metadata from the Blockchain based on the active (i.e currently use) claim condition
    public async void FetchMetadata()
    {
		Log("Getting claim conditions");

		if (contract != null)
		{
			Log("Contract is not null");
		}
		else
		{
			Log("Contract is null");
		}
        var claimConditions = await contract.DropERC20_GetActiveClaimCondition( );

		Log("Setting metadata");
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

	public async Task<BigInteger> BalanceOf( string address )
	{
		balanceOf = await contract.ERC20_BalanceOf( address );
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