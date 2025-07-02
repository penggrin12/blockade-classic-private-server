using System.Threading;
using System.Threading.Tasks;
using BlockadeClassicPrivateServer.Net.Shared;

namespace BlockadeClassicPrivateServer.Interfaces;

// TODO: spread this out
public interface IGameManager
{
	// TODO: use c# events?
	Godot.Timer RoundTimer { get; }
	Godot.Timer ScoreUpdateTimer { get; }

	GameMode GetGameMode();
	void SetGameMode(GameMode gameMode);

	string GetMapId();

	int GetTeamScore(Team team);
	void AddTeamScore(Team team, int score);

	Task ScoreUpdateLoopAsync(CancellationToken cancellationToken);
}
