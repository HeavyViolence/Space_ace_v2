using Cysharp.Threading.Tasks;

using UnityEngine;
using UnityEngine.Localization;

namespace SpaceAce.UI
{
    [CreateAssetMenu(fileName = "Entity view config",
                     menuName = "Space ace/Configs/UI/Entity view config")]
    public sealed class EntityViewConfig : ScriptableObject
    {
        #region icon

        [SerializeField]
        private Sprite _icon;

        public Sprite Icon => _icon;

        #endregion

        #region name

        [SerializeField]
        private LocalizedString _name;

        public async UniTask<string> GetLocalizedNameAsync()
        {
            var operation = _name.GetLocalizedStringAsync();

            await UniTask.WaitUntil(() => operation.IsDone == true);

            return operation.Result;
        }

        #endregion

        #region other

        [SerializeField]
        private bool _enableShootingView = false;

        public bool ShootingViewEnabled => _enableShootingView;

        #endregion
    }
}