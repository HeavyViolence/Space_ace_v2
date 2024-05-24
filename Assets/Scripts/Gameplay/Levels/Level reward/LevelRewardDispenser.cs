using SpaceAce.Gameplay.Players;

using System;

using Zenject;

namespace SpaceAce.Gameplay.Levels
{
    public sealed class LevelRewardDispenser : IInitializable, IDisposable
    {
        private readonly LevelRewardCollector _levelRewardCollector;
        private readonly LevelCompleter _levelCompleter;
        private readonly Player _player;

        public LevelRewardDispenser(LevelRewardCollector levelRewardCollector,
                                    LevelCompleter levelCompleter,
                                    Player player)
        {
            _levelRewardCollector = levelRewardCollector ?? throw new ArgumentNullException();
            _levelCompleter = levelCompleter ?? throw new ArgumentNullException();
            _player = player ?? throw new ArgumentNullException();
        }

        #region interfaces

        public void Initialize()
        {
            _levelCompleter.LevelCompleted += LevelCompletedEventHandler;
        }

        public void Dispose()
        {
            _levelCompleter.LevelCompleted -= LevelCompletedEventHandler;
        }

        #endregion

        #region event handlers

        private void LevelCompletedEventHandler(object sender, LevelEndedEventArgs e)
        {
            _player.Wallet.AddCredits(_levelRewardCollector.CreditsReward);
            _player.Experience.Add(_levelRewardCollector.ExperienceReward);
        }

        #endregion
    }
}