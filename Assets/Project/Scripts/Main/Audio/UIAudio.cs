using UnityEngine;

namespace SpaceAce.Main.Audio
{
    [CreateAssetMenu(fileName = "UI audio",
                     menuName = "Space ace/Configs/Audio/UI audio")]
    public sealed class UIAudio : ScriptableObject
    {
        [SerializeField]
        private AudioCollection _forwardButtonClickAudio;

        [SerializeField]
        private AudioCollection _backwardButtonClickAudio;

        [SerializeField]
        private AudioCollection _hoverOverAudio;

        [SerializeField]
        private AudioCollection _switchAudio;

        [SerializeField]
        private AudioCollection _creditsOperationAudio;

        [SerializeField]
        private AudioCollection _notificationAudio;

        [SerializeField]
        private AudioCollection _errorAudio;

        public AudioCollection ForwardClick => _forwardButtonClickAudio;
        public AudioCollection BackwardClick => _backwardButtonClickAudio;
        public AudioCollection HoverOver => _hoverOverAudio;
        public AudioCollection Switch => _switchAudio;

        public AudioCollection CreditsOperation => _creditsOperationAudio;

        public AudioCollection Notification => _notificationAudio;
        public AudioCollection Error => _errorAudio;
    }
}