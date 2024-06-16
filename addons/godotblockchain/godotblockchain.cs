#if TOOLS
using Godot;
using System;

[Tool]
public partial class godotblockchain : EditorPlugin
{
	public override void _EnterTree()
	{
		// Initialization of the Contract plugin goes here.
		var managerScript = GD.Load<Script>("res://addons/godotblockchain/BlockchainContractNode.cs");
		var managerTexture = GD.Load<Texture2D>("res://addons/godotblockchain/svg/block-filled-svgrepo-com.svg");
		AddCustomType("BlockchainManager", "Node", managerScript, managerTexture);
		
		// Initialization of the client plugin goes here.
		var clientScript = GD.Load<Script>("res://addons/godotblockchain/BlockchainClientNode.cs");
		var clientTexture = GD.Load<Texture2D>("res://addons/godotblockchain/svg/user-id-svgrepo-com.svg");
		AddCustomType("BlockchainClient", "Node", clientScript, clientTexture);		
		
		// Initialization of the Contract plugin goes here.
		var contractScript = GD.Load<Script>("res://addons/godotblockchain/BlockchainContractNode.cs");
		var contractTexture = GD.Load<Texture2D>("res://addons/godotblockchain/svg/script-1605-svgrepo-com.svg");
		AddCustomType("BlockchainContract", "Node", contractScript, contractTexture);
		
		// Initialization of the NFT plugin goes here.
		var nftScript = GD.Load<Script>("res://addons/godotblockchain/BlockchainNFTNode.cs");
		var nftTexture = GD.Load<Texture2D>("res://addons/godotblockchain/svg/canvas-paint-svgrepo-com.svg");
		AddCustomType("BlockchainNFT", "Node", nftScript, nftTexture);


		// Initialization of the Token plugin goes here.
		var tokenScript = GD.Load<Script>("res://addons/godotblockchain/BlockchainTokenNode.cs");
		var tokenTexture = GD.Load<Texture2D>("res://addons/godotblockchain/svg/coinstack-svgrepo-com.svg");
		AddCustomType("BlockchainToken", "Node", tokenScript, tokenTexture);

//// Resources

		// Initialization of the network resource goes here.
		var clientResourceScript = GD.Load<Script>("res://addons/godotblockchain/BlockchainClientConfigurationResource.cs");
		var clientResourceTexture = GD.Load<Texture2D>("res://addons/godotblockchain/svg/user-id-svgrepo-com.svg");
		AddCustomType("BlockchainClientConfigurationResource", "Resource", clientResourceScript, clientResourceTexture);

		// Initialization of the network resource goes here.
		var contractResourceScript = GD.Load<Script>("res://addons/godotblockchain/BlockchainContractResource.cs");
		var contractResourceTexture = GD.Load<Texture2D>("res://addons/godotblockchain/svg/script-1605-svgrepo-com.svg");
		AddCustomType("BlockchainContractResource", "Resource", contractResourceScript, contractResourceTexture);

		// Initialization of the network resource goes here.
		var networkResourceScript = GD.Load<Script>("res://addons/godotblockchain/BlockchainNetworkResource.cs");
		var networkResourceTexture = GD.Load<Texture2D>("res://addons/godotblockchain/svg/chain-links-svgrepo-com.svg");
		AddCustomType("BlockchainNetworkResource", "Resource", networkResourceScript, networkResourceTexture);

		// Initialization of the network resource goes here.
		var nftResourceScript = GD.Load<Script>("res://addons/godotblockchain/BlockchainNFTResource.cs");
		var nftResourceTexture = GD.Load<Texture2D>("res://addons/godotblockchain/svg/canvas-paint-svgrepo-com.svg");
		AddCustomType("BlockchainNFTResource", "Resource", nftResourceScript, nftResourceTexture);

		// Initialization of the network resource goes here.
		var tokenResourceScript = GD.Load<Script>("res://addons/godotblockchain/BlockchainTokenResource.cs");
		var tokenResourceTexture = GD.Load<Texture2D>("res://addons/godotblockchain/svg/coinstack-svgrepo-com.svg");
		AddCustomType("BlockchainTokenResource", "Resource", tokenResourceScript, tokenResourceTexture);
	}

	public override void _ExitTree()
	{
		// Clean-up of the plugin goes here.
		RemoveCustomType("BlockchainManager");
		RemoveCustomType("BlockchainClient");
		RemoveCustomType("BlockchainContract");	
		RemoveCustomType("BlockchainNFT");
		RemoveCustomType("BlockchainToken");
		
		// clean-up of the custom resources.
		RemoveCustomType("BlockchainClientConfiguration");
		RemoveCustomType("BlockchainContractResource");
		RemoveCustomType("BlockchainNetworkResource");
		RemoveCustomType("BlockchainNFTResource");
		RemoveCustomType("BlockchainTokenResource");
	}
}
#endif
