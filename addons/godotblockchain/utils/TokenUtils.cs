using System.Numerics;
using System.Threading.Tasks;
using Godot;
using Thirdweb;

namespace BIGConnect.addons.godotblockchain.utils;

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

    public static async Task<byte[]> GetNFTImage( NFT nft )
    {
        return await ThirdwebExtensions.GetNFTImageBytes( nft, BlockchainClientNode.Instance.internalClient );
    }

	public static async Task<Sprite2D> GetNFTAsSprite2D( ThirdwebContract contract, BigInteger nftId  )
    {
        NFT nft = await contract.ERC1155_GetNFT( nftId );

        return await GetNFTAsSprite2D( nft );
    }

    public static async Task<ImageTexture> GetNFTAsTexture( ThirdwebContract contract, BigInteger nftId )
    {
        NFT nft = await contract.ERC1155_GetNFT( nftId );

        return await GetNFTAsTexture( nft );
    }    

    public static async Task<StandardMaterial3D> GetNFTAsStandardMaterial3D( ThirdwebContract contract, BigInteger nftId )
    {
        NFT nft = await contract.ERC1155_GetNFT( nftId );

        return await GetNFTAsStandardMaterial3D( nft );
    }

    public static async Task<StandardMaterial3D> GetNFTAsStandardMaterial3D( NFT nft )
    {
        ImageTexture texture = await GetNFTAsTexture( nft );

        StandardMaterial3D material = new StandardMaterial3D();
        material.AlbedoTexture = texture;

        return material;
    }

    public static async Task<Sprite2D> GetNFTAsSprite2D( NFT nft )
    {
        ImageTexture texture = await GetNFTAsTexture(nft );
        
        Sprite2D sprite = new Sprite2D();
        sprite.Texture = texture;

        return sprite;        
    }

    public static async Task<ImageTexture> GetNFTAsTexture(NFT nft )
    {
        byte[] nftImageBytes = await nft.GetNFTImageBytes(BlockchainClientNode.Instance.internalClient);

        StreamPeerBuffer stream = new StreamPeerBuffer();
        stream.DataArray = nftImageBytes;

        Image image = new Image();

        if ( TokenUtils.GetFileType(nftImageBytes) == TokenUtils.PNG )
        {
            image.LoadPngFromBuffer(nftImageBytes);
        }
        else if ( TokenUtils.GetFileType(nftImageBytes) == TokenUtils.JPEG )
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

    public static async Task<AudioStreamMP3> GetNFTAsAudioStreamMP3(NFT nft)
    {
        byte[] downloadedData = await ThirdwebStorage.Download<byte[]>(BlockchainClientNode.Instance.internalClient, 
                                                                        nft.Metadata.AnimationUrl);		

        Log("Getting NFT as audio stream: " + nft.Metadata.Name);
        Log("Getting NFT as audio stream: " + nft.Metadata.Description);
        Log("Getting NFT as audio stream URL: " + nft.Metadata.AnimationUrl);

        Log("Received bytes for audio: " + downloadedData.Length);

        var audioStream = new AudioStreamMP3();
        audioStream.Data = downloadedData;

        return audioStream;
    }

    public static async Task<AudioStreamMP3> GetNFTAsAudioStreamMP3(ThirdwebContract contract, int nftId)
    {
        NFT nft = await contract.ERC1155_GetNFT( nftId );
        
        return await GetNFTAsAudioStreamMP3(nft);
    }

    public static async Task<byte[]> GetNFTAsByteArray(ThirdwebContract contract, int nftId)
    {
        NFT nft = await contract.ERC1155_GetNFT( nftId );

        return await GetNFTAsByteArray(nft);
    }

    public static async Task<byte[]> GetNFTAsByteArray(NFT nft)
    {
        byte[] downloadedData = await ThirdwebStorage.Download<byte[]>(BlockchainClientNode.Instance.internalClient, 
                                                                        nft.Metadata.AnimationUrl);		

        Log("Received bytes for audio: " + downloadedData.Length);

        return downloadedData;
    } 
}