
![SCR-20240613-u2](https://github.com/blockimperiumdao/godotblockchain/assets/94347075/c9a7724f-b7fa-446f-9b3b-2f08b64c865e)

## Overview

The BlockImperiumGames (BIG) Godot Blockchain plugin is a game specific optimization of the Thirdweb .NET SDK speifically tailored for games built with the opensource Godot 
gaming engine. It simplifies the integration of Web3 functionality into a game with a specific focus on the behaviors typically need for game developers (query token, 
purchase/load NFTs, etc) all while operating in the traditional Godot Node/Composition architecture to make this compatible with existing tools and Visual Programming solutions.

This project is offered under the LGPL Version 3 (https://www.gnu.org/licenses/lgpl-3.0.txt). A copy of the license for this project is in the license file atached to this project.

## Features


## Installation

1/ Ensure that you have the .NET SDK installed. Version 7.0 or later is recommended for this project.

2/ Ensure you have the godotblockchain addon (requires .NET for the plugin, but not for the game).

3/ Add the following to the PackageReference inside the ItemGroup in your csproj file. If there is no ItemGroup, drop this block as a child of the Project tag:

```
<ItemGroup>
  <!-- Other NuGet package references -->
  <PackageReference Include="Thirdweb" Version="0.4.0" />
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
	<PackageReference Include="Thirdweb" Version="1.0.0" />
	<!-- Update with the latest version when necessary -->
  </ItemGroup>
</Project>
```

4/ Ensure that the GodotBlockchain plugin is enables in Project->Project Settings->Plugins

If this is a new project you will also need to create a C# file so that the Godot Engine (as of Godot 4.3) will identify that you have a C# project. There is also an assembly which 
needs to be added to pull in the Thirdweb .NET SDK.

## Usage

There are a series of Blockchain nodes attached to the project which have reasonably clear purposes. The two that must be present are the BlockchainManager, which keeps track of the 
various contracts, and the BlockchainClient which is what will perform the actual connection to the blockchain using Thirdweb. This library does not provide simply functionality
for calling contracts, it is intended as an integrated solution.

## Getting Started

1/ Add a BlockchainManager node to the project

2/ Add a BlockchainClient to the project as a child of the BlockchainManager

3/ Create a new BlockchainClientConfigurationResource

4/ Set the address of the WalletFactory contract 

5/ Select the ChainID of the Blockchain that your Wallet Factor is on

6/ Provide a bundle id for your project (your Thirdweb configuration should whitelist this bundleid)

7/ Provide the ClientID from your Thirdweb Configuration

8/ Check the box if you want these transactions to be gasless (consult Thirdweb documentation for this setting)

You are now ready for connections to your Thirdweb AccountFactory and other Blockchain functionality in your game. 

NOTE: If you are using version control, ensure that the configuration is NOT committed to your version control repository by adding the file to your .gitignore
