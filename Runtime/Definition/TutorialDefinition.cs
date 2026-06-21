using System;
using System.Collections.Generic;

namespace GGemCo2DTutorial
{
    /// <summary>
    /// 튜토리얼에서 감지할 수 있는 표준 이벤트 종류입니다.
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
        Custom = 100,
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
        Custom = 100,
    }

    /// <summary>
    /// Tutorial Catalog 전체 정의입니다.
    /// </summary>
    [Serializable]
    public sealed class TutorialCatalog
    {
        /// <summary>
        /// 카탈로그에 등록된 튜토리얼 항목입니다.
        /// </summary>
        public List<TutorialCatalogEntry> tutorials = new List<TutorialCatalogEntry>();
    }

    /// <summary>
    /// 개별 Tutorial JSON의 Addressables 키와 자동 시작 조건을 정의합니다.
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
    /// 하나의 튜토리얼과 순차 실행할 단계 목록을 정의합니다.
    /// </summary>
    [Serializable]
    public sealed class TutorialDefinition
    {
        public int uid;
        public string title;
        public List<TutorialStepDefinition> steps = new List<TutorialStepDefinition>();
    }

    /// <summary>
    /// 튜토리얼 한 단계의 완료 조건과 진입/종료 액션을 정의합니다.
    /// </summary>
    [Serializable]
    public sealed class TutorialStepDefinition
    {
        public int uid;
        public string messageKey;
        public List<TutorialConditionDefinition> conditions = new List<TutorialConditionDefinition>();
        public List<TutorialActionDefinition> actionsOnEnter = new List<TutorialActionDefinition>();
        public List<TutorialActionDefinition> actionsOnExit = new List<TutorialActionDefinition>();
    }

    /// <summary>
    /// 이벤트 종류와 비교 값, 필요 횟수로 단계 완료 조건을 정의합니다.
    /// </summary>
    [Serializable]
    public sealed class TutorialConditionDefinition
    {
        public TutorialEventType type;
        public string key;
        public int intValue;
        public int requiredCount = 1;
    }

    /// <summary>
    /// 외부 처리기에 전달할 튜토리얼 액션과 인자를 정의합니다.
    /// </summary>
    [Serializable]
    public sealed class TutorialActionDefinition
    {
        public TutorialActionType type;
        public string key;
        public int intValue;
        public string[] stringValues;
    }
}
