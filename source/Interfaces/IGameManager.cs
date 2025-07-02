using BlockadeClassicPrivateServer.Shared;

namespace BlockadeClassicPrivateServer.Interfaces;

public interface IMapIdQuery
{
	string GetMapId();
}

public interface IGameModeQuery
{
	GameMode GetGameMode();
}

public interface IGameModeCommand
{
	void SetGameMode(GameMode gameMode);
}

public interface IGameScoreQuery
{
	int GetRemainingTime();
	int GetTeamScore(Team team);
}

public interface IGameScoreCommand
{
	void AddTeamScore(Team team, int score);
}
