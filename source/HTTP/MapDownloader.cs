using System.Net.Http;
using System.Threading.Tasks;
using Godot;
using Ionic.Zlib;

namespace BlockadeClassicPrivateServer.Http;

public static class MapDownloader
{
	private const string BaseUri = "http://localhost:7778/maps/";

	public static async Task<byte[]> DownloadMap(string mapId)
	{
		GD.Print($"[HTTP ] Gonna download {mapId}.map ...");
		using System.Net.Http.HttpClient httpClient = new();
		HttpResponseMessage response = await httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, BaseUri + mapId + ".map"));
		GD.Print($"[HTTP ] Done downloading {mapId}.map");
		return GZipStream.UncompressBuffer(await response.Content.ReadAsByteArrayAsync());
	}
}