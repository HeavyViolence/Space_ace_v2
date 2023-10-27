using SpaceAce.Gameplay.Players;
using UnityEngine;
using System;
using SpaceAce.Main;
using SpaceAce.Architecture;

public sealed class MasterAudioListenerHolder : IInitializable, IDisposable
{
    private readonly AudioListener _masterAudioListener = null;
    private readonly Player _player = null;
    private readonly MainMenuLoader _mainMenuLoader = null;

    public AudioListener MasterAudioListener => _masterAudioListener;

    public MasterAudioListenerHolder(GameObject masterCameraObject, Player player, MainMenuLoader mainMenuLoader)
    {
        if (masterCameraObject == null)
            throw new ArgumentNullException(nameof(masterCameraObject),
                $"Attempted to pass an empty audio listener {typeof(GameObject)}!");

        AudioListener listener = masterCameraObject.GetComponentInChildren<AudioListener>();

        if (listener == null)
            throw new MissingComponentException($"Passed master camera object is missing {typeof(AudioListener)}!");

        _masterAudioListener = listener;

        _player = player ?? throw new ArgumentNullException(nameof(player),
            $"Attempted to pass an empty {typeof(Player)}!");

        _mainMenuLoader = mainMenuLoader ?? throw new ArgumentNullException(nameof(mainMenuLoader),
            $"Attempted to pass an empty {typeof(MainMenuLoader)}!");
    }

    public void Initialize()
    {
        _player.SpaceshipSpawned += PlayerSpaceshipSpawnedEventHandler;
        _player.SpaceshipDefeated += PlayerSpaceshipDefeatedEventHandler;

        _mainMenuLoader.MainMenuLoadingStarted += MainMenuLoadingStartedEventHandler;
    }

    public void Dispose()
    {
        _player.SpaceshipSpawned -= PlayerSpaceshipSpawnedEventHandler;
        _player.SpaceshipDefeated -= PlayerSpaceshipDefeatedEventHandler;

        _mainMenuLoader.MainMenuLoadingStarted -= MainMenuLoadingStartedEventHandler;
    }

    #region event handlers

    private void PlayerSpaceshipSpawnedEventHandler(object sender, EventArgs e)
    {
        _masterAudioListener.enabled = false;
    }

    private void PlayerSpaceshipDefeatedEventHandler(object sender, EventArgs e)
    {
        _masterAudioListener.enabled = true;
    }

    private void MainMenuLoadingStartedEventHandler(object sender, MainMenuLoadingStartedEventArgs e)
    {
        _masterAudioListener.enabled = true;
    }

    #endregion
}
