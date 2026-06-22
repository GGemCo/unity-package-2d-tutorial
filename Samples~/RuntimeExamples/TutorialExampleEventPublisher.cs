using GGemCo2DTutorial;
using UnityEngine;

namespace GGemCo2DTutorialSamples
{
    /// <summary>
    /// UID와 enum 기반 튜토리얼 이벤트 발행 예제입니다.
    /// </summary>
    public sealed class TutorialExampleEventPublisher : MonoBehaviour
    {
        public void PublishMove()
        {
            TutorialEventBus.PublishInputAction(TutorialInputActionType.Move);
        }

        public void PublishJump()
        {
            TutorialEventBus.PublishInputAction(TutorialInputActionType.Jump);
        }

        public void PublishWindowOpened(int windowUid)
        {
            TutorialEventBus.PublishWindowOpened(windowUid);
        }

        public void PublishUiClicked(int targetUid)
        {
            TutorialEventBus.Publish(new TutorialGameEvent(
                TutorialEventType.UiClicked,
                targetUid));
        }

        public void PublishQuestCompleted(int questUid)
        {
            TutorialEventBus.PublishQuestState(questUid, completed: true);
        }
    }
}
