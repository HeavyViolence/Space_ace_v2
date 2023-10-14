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

        public AudioCollection ForwardButtonClickAudio => _forwardButtonClickAudio;
        public AudioCollection BackwardButtonClickAudio => _backwardButtonClickAudio;
        public AudioCollection HoverOverAudio => _hoverOverAudio;
        public AudioCollection SwitchAudio => _switchAudio;

        public AudioCollection CreditsOperationAudio => _creditsOperationAudio;

        public AudioCollection NotificationAudio => _notificationAudio;
        public AudioCollection ErrorAudio => _errorAudio;
    }
}