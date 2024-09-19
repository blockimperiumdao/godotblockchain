using System.Numerics;
using System.Threading.Tasks;
using Godot;
using Thirdweb;

namespace GodotBlockchain.addons.godotblockchain.utils;

static class TokenUtils {
	public const string BASE_USDC_TOKEN = "0x833589fCD6eDb6E08f4c7C32D4f71b54bdA02913";
	public const string BASE_DEGEN_TOKEN = "0x4ed4e862860bed51a9570b96d89af5e1b0efefed";
	public const string BASE_AERO_TOKEN = "0x940181a94a35a4569e4529a3cdfb74e38fd98631";
	public const string BASE_TESTNET_PAPER_TOKEN = "0xd4b856A271dd46845D924Ae72a4e9b890128cbA7";

    public const string THIRDWEB_CHAIN_NATIVE_TOKEN = "0xEeeeeEeeeEeEeeEeEeEeeEEEeeeeEeeeeeeeEEeE";

    private static readonly byte[] PngSignature = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
    private static readonly byte[] JpegSignature = new byte[] { 0xFF, 0xD8, 0xFF };
    private static readonly byte[] Mp3Signature = new byte[] { 0x49, 0x44, 0x33 }; // ID3 tag
    private static readonly byte[] Mp4Signature = new byte[] { 0x00, 0x00, 0x00, 0x18, 0x66, 0x74, 0x79, 0x70 }; // ftyp
	
	public static string PNG = "PNG";
    public static string JPEG = "JPEG";
    public static string MP3 = "MP3";
    public static string MP4 = "MP4";
	public static string GetFileType( byte[] fileBytes )
	{
        if (fileBytes.Length >= PngSignature.Length && MatchSignature(fileBytes, PngSignature))
            return PNG;
        if (fileBytes.Length >= JpegSignature.Length && MatchSignature(fileBytes, JpegSignature))
            return JPEG;
        if (fileBytes.Length >= Mp3Signature.Length && MatchSignature(fileBytes, Mp3Signature))
            return MP3;
        if (fileBytes.Length >= Mp4Signature.Length && MatchSignature(fileBytes, Mp4Signature))
            return MP4;
        
        return "Unknown";		
	}
    
    private static void Log( string message )
    {
        BlockchainLogManager.Instance.EmitLog("TokenUtils: " + message);
    } 

   private static bool MatchSignature(byte[] fileBytes, byte[] signature)
    {
        for (int i = 0; i < signature.Length; i++)
        {
            if (fileBytes[i] != signature[i])
                return false;
        }
        return true;
    }
   
	public static async Task<Sprite2D> GetERC1155AsSprite2D( ThirdwebContract contract, BigInteger nftId  )
    {
        NFT nft = await contract.ERC1155_GetNFT( nftId );

        return await GetNFTAsSprite2D( nft );
    }

    public static async Task<ImageTexture> GetERC1155AsTexture( ThirdwebContract contract, BigInteger nftId )
    {
        NFT nft = await contract.ERC1155_GetNFT( nftId );

        return await GetNFTAsTexture( nft );
    }    

    public static async Task<StandardMaterial3D> GetERC1155AsStandardMaterial3D( ThirdwebContract contract, BigInteger nftId )
    {
        NFT nft = await contract.ERC1155_GetNFT( nftId );

        return await GetNFTAsStandardMaterial3D( nft );
    }
    
    
    public static async Task<Sprite2D> GetERC721AsSprite2D( ThirdwebContract contract, BigInteger nftId  )
    {
        NFT nft = await contract.ERC721_GetNFT( nftId );

        return await GetNFTAsSprite2D( nft );
    }

    public static async Task<ImageTexture> GetERC721AsTexture( ThirdwebContract contract, BigInteger nftId )
    {
        NFT nft = await contract.ERC721_GetNFT( nftId );

        return await GetNFTAsTexture( nft );
    }    

    public static async Task<StandardMaterial3D> GetERC721AsStandardMaterial3D( ThirdwebContract contract, BigInteger nftId )
    {
        NFT nft = await contract.ERC721_GetNFT( nftId );

        return await GetNFTAsStandardMaterial3D( nft );
    }    
    
    public static async Task<AudioStreamMP3> GetERC1155AsAudioStreamMP3(ThirdwebContract contract, int nftId)
    {
        NFT nft = await contract.ERC1155_GetNFT( nftId );
        
        return await GetNFTAsAudioStreamMP3(nft);
    }
    
    public static async Task<AudioStreamMP3> GetERC721AsAudioStreamMP3(ThirdwebContract contract, int nftId)
    {
        NFT nft = await contract.ERC721_GetNFT( nftId );
        
        return await GetNFTAsAudioStreamMP3(nft);
    }    

    public static async Task<byte[]> GetERC1155AsByteArray(ThirdwebContract contract, int nftId)
    {
        NFT nft = await contract.ERC1155_GetNFT( nftId );

        return await GetNFTAsByteArray(nft);
    }
    
    public static async Task<byte[]> GetERC721AsByteArray(ThirdwebContract contract, int nftId)
    {
        NFT nft = await contract.ERC721_GetNFT( nftId );

        return await GetNFTAsByteArray(nft);
    }       
    
    /**
     * Get the NFT as a byte array using GetNFTImageBytes
     */
    public static async Task<byte[]> GetNFTImage( NFT nft )
    {
        return await ThirdwebExtensions.GetNFTImageBytes( nft, BlockchainClientNode.Instance.internalClient );
    }    

    /**
     * Get the NFT as a StandardMaterial3D
     */
    public static async Task<StandardMaterial3D> GetNFTAsStandardMaterial3D( NFT nft )
    {
        ImageTexture texture = await GetNFTAsTexture( nft );

        StandardMaterial3D material = new StandardMaterial3D();
        material.AlbedoTexture = texture;

        return material;
    }

    /**
     * Get the NFT as a Sprite2D
     */
    public static async Task<Sprite2D> GetNFTAsSprite2D( NFT nft )
    {
        ImageTexture texture = await GetNFTAsTexture(nft );
        
        Sprite2D sprite = new Sprite2D();
        sprite.Texture = texture;

        return sprite;        
    }

    /**
     * Get the NFT as an ImageTexture (supporting PNG and JPEG). Will return empty image texture if the media type is not supported.
     */
    public static async Task<ImageTexture> GetNFTAsTexture(NFT nft )
    {
        byte[] nftImageBytes = await nft.GetNFTImageBytes(BlockchainClientNode.Instance.internalClient);

        StreamPeerBuffer stream = new StreamPeerBuffer();
        stream.DataArray = nftImageBytes;

        Image image = new Image();

        if ( GetFileType(nftImageBytes) == TokenUtils.PNG )
        {
            image.LoadPngFromBuffer(nftImageBytes);
        }
        else if ( GetFileType(nftImageBytes) == TokenUtils.JPEG )
        {
            image.LoadJpgFromBuffer(nftImageBytes);
        }
        else
        {
            Log("Unknown or unsupported media type for NFT image - returning empty sprite");
            return new ImageTexture();
        }

        ImageTexture texture = new ImageTexture();
        texture.SetImage(image);

        return texture;
    }

    /**
     * Get the NFT as an MP3 AudioStream
     */
    public static async Task<AudioStreamMP3> GetNFTAsAudioStreamMP3(NFT nft)
    {
        byte[] downloadedData = await GetNFTAsByteArray(nft);		

        var audioStream = new AudioStreamMP3();
        audioStream.Data = downloadedData;

        return audioStream;
    }
    
    /**
     * Get the NFT as a GLTF document
     */
    public static async Task<GltfState> GetNFTAsGltfDocument(NFT nft)
    {
        byte[] downloadedData = await GetNFTAsByteArray(nft);
        
        // Load the GLTF model using GltfDocument
        GltfDocument gltfDocument = new GltfDocument();
        GltfState gltfState = new GltfState();
        Error error = gltfDocument.AppendFromBuffer(downloadedData, "", gltfState);
        if (error != Error.Ok)
        {
            Log("Error loading GLTF document: " + error);
        }

        return gltfState;
    }    

 
    /**
     * Get the NFT as a byte[]
     */
    public static async Task<byte[]> GetNFTAsByteArray(NFT nft)
    {
        Log("Getting NFT (Name): " + nft.Metadata.Name);
        Log("Getting NFT (Description): " + nft.Metadata.Description);
        Log("Getting NFT (AnimationUrl): " + nft.Metadata.AnimationUrl);
        
        byte[] downloadedData = await ThirdwebStorage.Download<byte[]>(BlockchainClientNode.Instance.internalClient, 
                                                                        nft.Metadata.AnimationUrl);		
        
        Log("Received bytes (length): " + downloadedData.Length);    
        
        return downloadedData;
    }


}
