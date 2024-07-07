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

	public async Task<ThirdwebTransactionReceipt> ClaimERC20( string amount )
	{
		return await contract.DropERC20_Claim(  BlockchainClientNode.Instance.smartWallet, await BlockchainClientNode.Instance.smartWallet.GetAddress(), amount );
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

	public async Task<BigInteger> Allowance( string owner, string spender )
	{
		return await contract.ERC20_Allowance( owner, spender );
	}

	public async Task<ThirdwebTransactionReceipt> Transfer( string toAddress, BigInteger amount )
	{
		return await contract.ERC20_Transfer( BlockchainClientNode.Instance.smartWallet, toAddress, amount );
	}

}

#endif