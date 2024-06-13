
![SCR-20240613-u2](https://github.com/blockimperiumdao/godotblockchain/assets/94347075/c9a7724f-b7fa-446f-9b3b-2f08b64c865e)

## Overview

The BlockImperiumGames (BIG) Godot Blockchain plugin is a game specific optimization of the Thirdweb .NET SDK speifically tailored for games built with the opensource Godot 
gaming engine. It simplifies the integration of Web3 functionality into a game with a specific focus on the behaviors typically need for game developers (query token, 
purchase/load NFTs, etc) all while operating in the traditional Godot Node/Composition architecture to make this compatible with existing tools and Visual Programming solutions.

## Features


## Installation

Ensure that you have the .NET SDK installed. Version 7.0 or later is recommended for this project.

Download the archive or perform a git pull. 

Extract the godotblockchain addon folder and drop it in your project. 

If this is a new project you will also need to create a C# file so that the Godot Engine (as of Godot 4.3) will identify that you have a C# project. There is also an assembly which 
needs to be added to pull in the Thirdweb .NET SDK.

## Usage

There are a series of Blockchain nodes attached to the project which have reasonably clear purposes. The two that must be present are the BlockchainManager, which keeps track of the 
various contracts, and the BlockchainClient which is what will perform the actual connection to the blockchain using Thirdweb. This library does not provide simply functionality
for calling contracts, it is intended as an integrated solution.

## Getting Started

In version 0.3, the best way to get started is to read the Unit Tests. Sorry, need some time to build out better documentation and some videos - which should be coming in a later version.
