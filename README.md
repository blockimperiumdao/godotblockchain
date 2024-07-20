
![SCR-20240613-u2](https://github.com/blockimperiumdao/godotblockchain/assets/94347075/c9a7724f-b7fa-446f-9b3b-2f08b64c865e)

## Overview

The BlockImperiumGames (BIG) Godot Blockchain plugin is a game specific optimization of the Thirdweb .NET SDK speifically tailored for games built with the opensource Godot 
gaming engine. It simplifies the integration of Web3 functionality into a game with a specific focus on the behaviors typically need for game developers (query token, 
purchase/load NFTs, etc) all while operating in the traditional Godot Node/Composition architecture to make this compatible with existing tools and Visual Programming solutions.

This project is offered under the LGPL Version 3 (https://www.gnu.org/licenses/lgpl-3.0.txt). A copy of the license for this project is in the license file atached to this project.

## Features

Version 0.5.0 Removes entirely, the UI components which are now all in the BIGConnect project. This was
done to make the system more modular and enable users to bring their own UX.

Version 0.6.0 Optimizations and decoupling of the log management for the client from the BlockchainManagerNode
so that other components can listen to this via signals as opposed to it going direct
to a particular node.

Version 0.7.0 Optimizations, removal of the BlockchainManager and extracted out relevant functionality to the BlockchainLogManager which will keep track of history of transactions and logging information.

## Installation

1/ Ensure that you have the .NET SDK installed. Version 7.0 or later is recommended for this project.

2/ Ensure you have the godotblockchain addon (requires .NET for the plugin, but your game does not need to be written entirely in C#).

3/ Add the following to the PackageReference inside the ItemGroup in your csproj file. If there is no ItemGroup, drop this block as a child of the Project tag:

```
<ItemGroup>
  <!-- Other NuGet package references -->
  <PackageReference Include="Thirdweb" Version="1.0.1" />
  <!-- Update with the latest version when necessary -->
</ItemGroup>
```

An complete csproj file might look like
```
<Project Sdk="Godot.NET.Sdk/4.3.0">
  <PropertyGroup>
	<TargetFramework>net6.0</TargetFramework>
	<TargetFramework Condition=" '$(GodotTargetPlatform)' == 'android' ">net7.0</TargetFramework>
	<TargetFramework Condition=" '$(GodotTargetPlatform)' == 'ios' ">net8.0</TargetFramework>
	<EnableDynamicLoading>true</EnableDynamicLoading>
  </PropertyGroup>

  <ItemGroup>
	<!-- Other NuGet package references -->
	<PackageReference Include="Thirdweb" Version="1.0.1" />
	<!-- Update with the latest version when necessary -->
  </ItemGroup>
</Project>
```

4/ Ensure that the GodotBlockchain plugin is enabled in Project->Project Settings->Plugins

If this is a new project you will also need to create a C# file so that the Godot Engine (as of Godot 4.3) will identify that you have a C# project. There is also an assembly which 
needs to be added to pull in the Thirdweb .NET SDK.

## Usage

There are a series of Blockchain nodes attached to the project which have reasonably clear purposes. The two that must be present are the BlockchainManager, which keeps track of the 
various contracts, and the BlockchainClient which is what will perform the actual connection to the blockchain using Thirdweb. This library does not provide simply functionality
for calling contracts, it is intended as an integrated solution.

## Getting Started

Note - this will create a complete example project. You may ignore functionality that you don't intend to use in your game once you have everything wired up.

1/ Create an Account on thirdweb and go to the Dashboard.

2/ Create and deploy ManagedAccountFactory contract - keep track of the address. This is what will handle users creating accounts with OAuth, Email, Blockchain wallets, etc.

3/ In Thirdweb, create an Token Contract (note - not a token drop). This represents an ERC20 contract that is a currency token. You can create more of these later.

4/ In godot create a new project. 

5/ Add a BlockchainClientNode to the scene. In the properties, configure the BlockchainClientConfigurationResource with the data from your Thirdweb account. With this alone, your project can now utilize Smart Wallets to onboard users with only their email address.

5a/ Set the address of the WalletFactory contract 

5b/ Select the ChainID of the Blockchain that your Wallet Factor is on

5c/ Provide a bundle id for your project (your Thirdweb configuration should whitelist this bundleid)

5d/ Provide the ClientID from your Thirdweb Configuration

5e/ Check the box if you want these transactions to be gasless (consult Thirdweb documentation for this setting)

6/ Create a TextEdit for the email address and the one time password (OTP) that will be sent to the email address to confirm the users email address.

You are now ready for connections to your Thirdweb AccountFactory and other Blockchain functionality in your game. 

NOTE: If you are using version control, ensure that the configuration is NOT committed to your version control repository by adding the file to your .gitignore

7/ Create a C# script "MyChainStuff.cs" that will handle interactions with the library (you can do this with GDScript using the cross language functionality for having GDScript call C# classes)

8/ Add an ERC20BlockchainContractNode to the scene. Configure the contract and the chain that this 
ERC20 represents.

9/ In MyChainStuff you will start the login process by calling

```
			BlockchainClientNode.Instance.OnStartLogin( emailTextEntry.Text );
```

This will connect to the AccountFactory and create an InAppWallet for your user using their email address. They will receive an email with an OTP that must be submitted to confirm the email address.

### Listening for Events

Since the library is async you will listen for a signal to know when things are completed. The most common things to listen for with login are the following:

```
		BlockchainClientNode.Instance.AwaitingOTP += OnAwaitingOTP;
		BlockchainClientNode.Instance.InAppWalletCreated += OnInAppWalletCreated;
		BlockchainClientNode.Instance.SmartWalletCreated += OnSmartWalletCreated;
```
The first will let you know that the system is waiting for the OTP

You can confirm this with a call to
```
			bool success = await BlockchainClientNode.Instance.OnOTPSubmit( otpTextEntry.Text );
```
### Creating Smart Wallets

The second let's you know that the InAppWallet for the users email address has been created. Generally you would create the user's SmartWallet at this point with a call to

```
			await BlockchainClientNode.Instance.CreateSmartWallet( );
```

You will know that this is completed from the SmartWalletCreated, which in this example will be received in the OnSmartWalletCreated.

You can get the addresses to the various blockchain addresses using

```
			await BlockchainClientNode.Instance.smartWallet.GetAddress();
      await BlockchainClientNode.Instance.inAppWallet.GetAddress();
```
### Reading ERC20 Contracts

Similar to how you interact with the BlockchainClientNode to get information about the address of the account you are using to connect to the Blockchain, you can get information about your ERC20 currency contract with the following

```
		currencyContract.ERC20BlockchainContractInitialized += OnERC20BlockchainContractInitialized;
```

In this case we are listning to the ERC20BlockchainContractNode and waiting for it to be initialized so we can read information about it. We can get things like the users balance, the name of the symbol for this contract, etc.

```
	public async void OnERC20BlockchainContractInitialized()
	{
		GD.Print("ERC20BlockchainContractInitialized");

		string symbol = await currencyContract.Symbol();
		BigInteger balance = await currencyContract.BalanceOf();
		BigInteger decimals = await currencyContract.Decimals();

		balance = balance / BigInteger.Pow(10, (int)decimals);

		tokenSymbol.Text = symbol;
		tokenBalance.Text = balance.ToString();
	}
```