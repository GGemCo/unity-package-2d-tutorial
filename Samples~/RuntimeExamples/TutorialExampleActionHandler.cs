using GGemCo2DTutorial;
using UnityEngine;
using UnityEngine.Events;

namespace GGemCo2DTutorialSamples
{
    /// <summary>
    /// UID 기반 외부 튜토리얼 액션 처리 예제입니다.
    /// </summary>
    public sealed class TutorialExampleActionHandler : MonoBehaviour,
        ITutorialActionHandler
    {
        [SerializeField] private UnityEvent<int> onGuideShown;
        [SerializeField] private UnityEvent<int> onUiHighlighted;
        [SerializeField] private UnityEvent<int> onFeatureUnlocked;

        private void OnEnable()
        {
            TutorialActionHandlerRegistry.Register(this);
        }

        private void OnDisable()
        {
            TutorialActionHandlerRegistry.Unregister(this);
        }

        public bool TryExecute(in TutorialActionContext context)
        {
            TutorialActionDefinition action = context.Action;
            switch (action.type)
            {
                case TutorialActionType.ShowGuide:
                    onGuideShown?.Invoke(action.targetUid);
                    return true;
                case TutorialActionType.HighlightUi:
                    onUiHighlighted?.Invoke(action.targetUid);
                    return true;
                case TutorialActionType.UnlockFeature:
                    onFeatureUnlocked?.Invoke(action.targetUid);
                    return true;
                default:
                    return false;
            }
        }
    }
}
