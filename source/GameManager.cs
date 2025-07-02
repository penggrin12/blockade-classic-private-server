using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BlockadeClassicPrivateServer.Interfaces;
using BlockadeClassicPrivateServer.Shared;

namespace BlockadeClassicPrivateServer;

public class GameManager(IPacketSender packetSender) : IMapIdQuery, IGameModeQuery, IGameModeCommand, IGameScoreQuery, IGameScoreCommand
{
	private const int DefaultTime = 900;

	public Dictionary<Team, int> scores = new() { { Team.Blue, 0 }, { Team.Red, 0 }, { Team.Green, 0 }, { Team.Yellow, 0 } };

	private int TimeLeft = DefaultTime;
	private GameMode _mode = GameMode.Build;

	public int GetRemainingTime()
	{
		return TimeLeft;
	}

	public int GetTeamScore(Team team)
	{
		return scores[team];
	}

	public void AddTeamScore(Team team, int score)
	{
		scores[team] += score;
	}

	public string GetMapId() => "1";

	public GameMode GetGameMode()
	{
		return _mode;
	}

	public void SetGameMode(GameMode gameMode)
	{
		_mode = gameMode;
	}

	public async Task ScoreUpdateLoopAsync(CancellationToken cancellationToken)
	{
		PeriodicTimer timer = new(TimeSpan.FromSeconds(1));

		try
		{
			while (await timer.WaitForNextTickAsync(cancellationToken))
			{
				if (_mode != GameMode.Build)
				{
					TimeLeft--;
					if (TimeLeft <= 0)
					{
						// TODO: trigger server restart/map change
						TimeLeft = DefaultTime;
					}

					var scoresPacket = Packet.Create(PacketName.Scores);
					scoresPacket.Write<int>(GetTeamScore(Team.Blue));
					scoresPacket.Write<int>(GetTeamScore(Team.Red));
					scoresPacket.Write<int>(GetTeamScore(Team.Green));
					scoresPacket.Write<int>(GetTeamScore(Team.Yellow));
					scoresPacket.Write<int>(TimeLeft);
					packetSender.BroadcastPacket(scoresPacket);
				}
			}
		}
		catch (OperationCanceledException) {}
		finally { timer.Dispose(); }
	}
}
