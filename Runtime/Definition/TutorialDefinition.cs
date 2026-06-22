using System;
using System.Collections.Generic;

namespace GGemCo2DTutorial
{
    /// <summary>
    /// 튜토리얼 조건에서 감지할 수 있는 표준 게임 이벤트 종류입니다.
    /// </summary>
    public enum TutorialEventType
    {
        None = 0,
        EnterMap = 1,
        KillMonster = 2,
        InputAction = 3,
        OpenWindow = 4,
        UiClicked = 5,
        QuestStarted = 6,
        QuestCompleted = 7,
        AutoMoveDistanceReached = 8,
        GuideClicked = 9,
        CombatStarted = 10,
    }

    /// <summary>
    /// 튜토리얼 단계 진입 또는 종료 시 실행할 표준 액션 종류입니다.
    /// </summary>
    public enum TutorialActionType
    {
        None = 0,
        ShowGuide = 1,
        HideGuide = 2,
        HighlightUi = 3,
        ClearHighlight = 4,
        BlockInputExcept = 5,
        ClearInputBlock = 6,
        UnlockFeature = 7,
        StartQuest = 8,
        SetGameplayState = 9,
    }

    /// <summary>
    /// 튜토리얼 조건과 입력 차단 정책에서 사용하는 표준 입력 종류입니다.
    /// </summary>
    public enum TutorialInputActionType
    {
        None = 0,
        Move = 1,
        Jump = 2,
        Guard = 3,
        Attack = 4,
        Dash = 5,
        Interaction = 6,
    }

    /// <summary>
    /// 튜토리얼에서 허용할 입력 종류를 비트 마스크로 정의합니다.
    /// </summary>
    [Flags]
    public enum TutorialInputActionMask
    {
        None = 0,
        Move = 1 << 0,
        Jump = 1 << 1,
        Guard = 1 << 2,
        Attack = 1 << 3,
        Dash = 1 << 4,
        Interaction = 1 << 5,
        All = Move | Jump | Guard | Attack | Dash | Interaction,
    }

    /// <summary>
    /// 튜토리얼 액션으로 변경할 게임 진행 상태입니다.
    /// </summary>
    public enum TutorialGameplayState
    {
        Running = 0,
        Paused = 1,
    }

    /// <summary>
    /// 로드 가능한 튜토리얼 목록입니다.
    /// </summary>
    [Serializable]
    public sealed class TutorialCatalog
    {
        public List<TutorialCatalogEntry> tutorials = new List<TutorialCatalogEntry>();
    }

    /// <summary>
    /// 개별 튜토리얼의 로드 주소와 자동 시작 조건입니다.
    /// </summary>
    [Serializable]
    public sealed class TutorialCatalogEntry
    {
        public int uid;
        public string addressableKey;
        public bool repeatable;
        public TutorialConditionDefinition startCondition;
    }

    /// <summary>
    /// 하나의 튜토리얼과 순차 실행 단계를 정의합니다.
    /// </summary>
    [Serializable]
    public sealed class TutorialDefinition
    {
        public int uid;
        public string title;
        public List<TutorialStepDefinition> steps = new List<TutorialStepDefinition>();
    }

    /// <summary>
    /// 튜토리얼 한 단계의 완료 조건과 진입·종료 액션을 정의합니다.
    /// </summary>
    [Serializable]
    public sealed class TutorialStepDefinition
    {
        public int uid;
        public int messageUid;
        public List<TutorialConditionDefinition> conditions = new List<TutorialConditionDefinition>();
        public List<TutorialActionDefinition> actionsOnEnter = new List<TutorialActionDefinition>();
        public List<TutorialActionDefinition> actionsOnExit = new List<TutorialActionDefinition>();
    }

    /// <summary>
    /// 이벤트 종류와 UID, 입력 enum, 수치 기준으로 단계 완료 조건을 정의합니다.
    /// </summary>
    [Serializable]
    public sealed class TutorialConditionDefinition
    {
        public TutorialEventType type;
        public int targetUid;
        public TutorialInputActionType inputAction;
        public int intValue;
        public float floatValue;
        public int requiredCount = 1;
    }

    /// <summary>
    /// 표준 액션 처리기에 전달할 UID, enum, Addressables 주소 기반 인자를 정의합니다.
    /// </summary>
    [Serializable]
    public sealed class TutorialActionDefinition
    {
        public TutorialActionType type;
        public int targetUid;
        public int intValue;
        public TutorialInputActionMask inputMask;
        public TutorialGameplayState gameplayState;

        /// <summary>
        /// ShowGuide 액션에서 표시할 Sprite의 Addressables 런타임 주소입니다.
        /// </summary>
        public string guideSpriteAddress;
    }
}
