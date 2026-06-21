using GGemCo2DTutorial;
using UnityEngine;
using UnityEngine.Events;

namespace GGemCo2DTutorial.Samples
{
    /// <summary>
    /// Tutorial 외부 액션을 프로젝트 UI와 게임 기능에 연결하는 예제 처리기입니다.
    /// </summary>
    public sealed class TutorialExampleActionHandler : MonoBehaviour, ITutorialActionHandler
    {
        [SerializeField]
        private UnityEvent<string> onGuideShown;

        [SerializeField]
        private UnityEvent<string> onGuideHidden;

        [SerializeField]
        private UnityEvent<string> onUiHighlighted;

        [SerializeField]
        private UnityEvent<string> onUiHighlightCleared;

        [SerializeField]
        private UnityEvent<string> onFeatureUnlocked;

        [SerializeField]
        private UnityEvent<int> onQuestStartRequested;

        [SerializeField]
        private UnityEvent<string> onCustomAction;

        /// <summary>
        /// 컴포넌트가 활성화되면 Tutorial 액션 처리기로 등록합니다.
        /// </summary>
        private void OnEnable()
        {
            TutorialActionHandlerRegistry.Register(this);
        }

        /// <summary>
        /// 컴포넌트가 비활성화되면 Tutorial 액션 처리기 등록을 해제합니다.
        /// </summary>
        private void OnDisable()
        {
            TutorialActionHandlerRegistry.Unregister(this);
        }

        /// <summary>
        /// Tutorial 액션을 종류별 UnityEvent로 전달합니다.
        /// 입력 제한 액션은 Tutorial Runtime 내부에서 처리하므로 이 예제에서는 다루지 않습니다.
        /// </summary>
        /// <param name="context">현재 Tutorial과 단계 정보가 포함된 액션 컨텍스트입니다.</param>
        /// <returns>이 컴포넌트가 액션을 처리했으면 true입니다.</returns>
        public bool TryExecute(in TutorialActionContext context)
        {
            TutorialActionDefinition action = context.Action;
            if (action == null)
            {
                return false;
            }

            switch (action.type)
            {
                case TutorialActionType.ShowGuide:
                    onGuideShown?.Invoke(action.key);
                    return true;
                case TutorialActionType.HideGuide:
                    onGuideHidden?.Invoke(action.key);
                    return true;
                case TutorialActionType.HighlightUi:
                    onUiHighlighted?.Invoke(action.key);
                    return true;
                case TutorialActionType.ClearHighlight:
                    onUiHighlightCleared?.Invoke(action.key);
                    return true;
                case TutorialActionType.UnlockFeature:
                    onFeatureUnlocked?.Invoke(action.key);
                    return true;
                case TutorialActionType.StartQuest:
                    onQuestStartRequested?.Invoke(action.intValue);
                    return true;
                case TutorialActionType.Custom:
                    onCustomAction?.Invoke(action.key);
                    return true;
                default:
                    return false;
            }
        }
    }
}
