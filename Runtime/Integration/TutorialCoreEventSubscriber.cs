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
        /// Core 맵 입장, 몬스터 처치, 캐릭터 사망 이벤트를 구독합니다.
        /// </summary>
        public void Subscribe()
        {
            if (_isSubscribed)
            {
                return;
            }

            GameEventManager.MapEnteredEvent += HandleMapEntered;
            GameEventManager.MonsterKilledEvent += HandleMonsterKilled;
            GameEventManager.CharacterDiedEvent += HandleCharacterDied;
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
            GameEventManager.CharacterDiedEvent -= HandleCharacterDied;
            _isSubscribed = false;
        }

        /// <summary>
        /// Core 맵 입장 이벤트를 Tutorial 맵 입장 이벤트로 변환합니다.
        /// </summary>
        /// <param name="eventData">입장한 맵 정보입니다.</param>
        private static void HandleMapEntered(MapEnteredEventData eventData)
        {
            TutorialEventBus.Publish(new TutorialGameEvent(
                TutorialEventType.EnterMap,
                targetUid: eventData.MapUid));
        }

        /// <summary>
        /// Core 몬스터 처치 이벤트를 Tutorial 몬스터 처치 이벤트로 변환합니다.
        /// </summary>
        /// <param name="eventData">처치한 몬스터 정보입니다.</param>
        private static void HandleMonsterKilled(MonsterKilledEventData eventData)
        {
            TutorialEventBus.Publish(new TutorialGameEvent(
                TutorialEventType.KillMonster,
                targetUid: eventData.monsterUid));
        }

        /// <summary>
        /// Core 캐릭터 사망 이벤트 중 플레이어 사망만 Tutorial 이벤트로 변환합니다.
        /// </summary>
        /// <param name="eventData">사망한 캐릭터와 사망 원인 정보입니다.</param>
        private static void HandleCharacterDied(CharacterDiedEventData eventData)
        {
            // CharacterDiedEvent는 모든 캐릭터에 대해 발행되므로 플레이어만 전달합니다.
            if (eventData.Character == null || !eventData.Character.IsPlayer())
            {
                return;
            }

            TutorialEventBus.PublishPlayerDied();
        }
    }
}
