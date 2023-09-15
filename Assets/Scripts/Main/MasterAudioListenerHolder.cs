using SpaceAce.Gameplay.Players;
using UnityEngine;
using Zenject;
using System;
using SpaceAce.Main;

public sealed class MasterAudioListenerHolder : MonoBehaviour
{
    [SerializeField] private AudioListener _masterListener;

    private Player _player = null;
    private LevelLoader _levelLoader = null;

    public AudioListener MasterListener => _masterListener;

    [Inject]
    private void Construct(Player player, LevelLoader levelLoader)
    {
        _player = player;
        _levelLoader = levelLoader;
    }

    private void OnEnable()
    {
        _player.SpaceshipSpawned += PlayerSpaceshipSpawnedEventHandler;
        _player.SpaceshipDefeated += PlayerSpaceshipDefeatedEventHandler;

        _levelLoader.MainMenuLoadingStarted += MainMenuLoadingStartedEventHandler;
    }

    private void OnDisable()
    {
        _player.SpaceshipSpawned -= PlayerSpaceshipSpawnedEventHandler;
        _player.SpaceshipDefeated -= PlayerSpaceshipDefeatedEventHandler;

        _levelLoader.MainMenuLoadingStarted -= MainMenuLoadingStartedEventHandler;
    }

    #region event handlers

    private void PlayerSpaceshipSpawnedEventHandler(object sender, EventArgs e)
    {
        _masterListener.enabled = false;
    }

    private void PlayerSpaceshipDefeatedEventHandler(object sender, EventArgs e)
    {
        _masterListener.enabled = true;
    }

    private void MainMenuLoadingStartedEventHandler(object sender, MainMenuLoadingStartedEventArgs e)
    {
        _masterListener.enabled = true;
    }

    #endregion
}
