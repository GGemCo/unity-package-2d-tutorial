using GGemCo2DCore;

namespace GGemCo2DTutorial
{
    /// <summary>
    /// Core의 공통 게임 이벤트를 Tutorial 표준 이벤트로 변환합니다.
    /// </summary>
    public sealed class TutorialCoreEventSubscriber
    {
        private bool _isSubscribed;

        /// <summary>
        /// Core 맵 입장과 몬스터 처치 이벤트를 구독합니다.
        /// </summary>
        public void Subscribe()
        {
            if (_isSubscribed)
            {
                return;
            }

            GameEventManager.MapEnteredEvent += HandleMapEntered;
            GameEventManager.MonsterKilledEvent += HandleMonsterKilled;
            _isSubscribed = true;
        }

        /// <summary>
        /// Core 이벤트 구독을 해제합니다.
        /// </summary>
        public void Unsubscribe()
        {
            if (!_isSubscribed)
            {
                return;
            }

            GameEventManager.MapEnteredEvent -= HandleMapEntered;
            GameEventManager.MonsterKilledEvent -= HandleMonsterKilled;
            _isSubscribed = false;
        }

        private static void HandleMapEntered(MapEnteredEventData eventData)
        {
            TutorialEventBus.Publish(new TutorialGameEvent(
                TutorialEventType.EnterMap,
                targetUid: eventData.MapUid));
        }

        private static void HandleMonsterKilled(MonsterKilledEventData eventData)
        {
            TutorialEventBus.Publish(new TutorialGameEvent(
                TutorialEventType.KillMonster,
                targetUid: eventData.monsterUid));
        }
    }
}
