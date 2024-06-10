#if TOOLS
using System.Threading.Tasks;
using Godot;
using Thirdweb;
using System.Numerics;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Signer;

[GlobalClass,Tool]
public partial class BlockchainNetworkResource : Resource
{
    [Export]
    private string name { get; set; }  

    [Export]
    private string symbol { get; set; }

    [Export]
    private string rpc { get; set; }

    [Export]
    private string nativeToken { get; set; }

    [Export]
    private string nativeTokenAddress { get; set; }

	[Export(PropertyHint.Enum, "EthereumMainnet:1,EthereumGoerli:5,BaseMainnet:8453,BaseSepolia:84532,ArbitrumMainnet:42161,ArbitrumSepolia:421614,PolygonMainnet:137,PolygonzkEVM:1101,PolygonzkEVMTestnet:1442,OPMainnet:10,OPBedrock:28528,OPKovan:69,DogechainMainnet:2000,DogechainTestnet:568")]
	public int chainId {get; set;}

    public BlockchainNetworkResource()
    {
        name = "";
        symbol = "";
        rpc = "";
        nativeToken = "";
        nativeTokenAddress = "0x";
        chainId = 82532; // BaseSepolia
    }      

}

#endif


