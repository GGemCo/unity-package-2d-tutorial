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
        GuideClosed = 11,
        PlayerDied = 12,
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
    /// 여러 자동 시작 조건을 결합하는 논리 방식을 정의합니다.
    /// </summary>
    public enum TutorialStartConditionMatchMode
    {
        All = 0,
        Any = 1,
    }

    /// <summary>
    /// 자동 시작 조건이 일회성 이벤트인지 현재 상태 판정인지 구분합니다.
    /// </summary>
    public enum TutorialStartConditionSource
    {
        Event = 0,
        State = 1,
    }

    /// <summary>
    /// 튜토리얼 자동 시작 시 현재 값을 조회할 수 있는 표준 상태 종류입니다.
    /// </summary>
    public enum TutorialStartStateType
    {
        None = 0,
        WindowVisible = 1,
        MapCleared = 2,
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

        /// <summary>
        /// 기존 단일 자동 시작 조건 데이터와의 하위 호환을 위해 유지합니다.
        /// 신규 Catalog는 <see cref="startConditions"/>를 우선 사용합니다.
        /// </summary>
        public TutorialConditionDefinition startCondition;

        /// <summary>
        /// 신규 복합 자동 시작 조건 그룹입니다.
        /// </summary>
        public TutorialStartConditionGroup startConditions;
    }

    /// <summary>
    /// 하나 이상의 자동 시작 조건과 결합 방식을 보관합니다.
    /// </summary>
    [Serializable]
    public sealed class TutorialStartConditionGroup
    {
        public TutorialStartConditionMatchMode matchMode =
            TutorialStartConditionMatchMode.All;
        public List<TutorialStartConditionDefinition> conditions =
            new List<TutorialStartConditionDefinition>();
    }

    /// <summary>
    /// 이벤트 발생 이력 또는 현재 런타임 상태를 기준으로 자동 시작 조건을 정의합니다.
    /// </summary>
    [Serializable]
    public sealed class TutorialStartConditionDefinition
    {
        public TutorialStartConditionSource source =
            TutorialStartConditionSource.Event;
        public TutorialEventType eventType;
        public TutorialStartStateType stateType;
        public int targetUid;
        public TutorialInputActionType inputAction;
        public int intValue;
        public float floatValue;
        public int requiredCount = 1;
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
    /// 가이드 한 페이지에서 함께 표시할 Sprite 주소와 현지화 설명 키를 정의합니다.
    /// </summary>
    [Serializable]
    public sealed class TutorialGuidePageDefinition
    {
        public string spriteAddress;
        public string descriptionLocalizationKey;
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
        /// 기존 단일 페이지 JSON과의 하위 호환성을 위해 유지합니다.
        /// </summary>
        public string guideSpriteAddress;

        /// <summary>
        /// ShowGuide 액션에서 페이지 순서대로 표시할 Sprite Addressables 런타임 주소 목록입니다.
        /// 기존 다중 페이지 JSON과의 하위 호환성을 위해 유지합니다.
        /// </summary>
        public List<string> guideSpriteAddresses = new List<string>();

        /// <summary>
        /// ShowGuide 액션에서 페이지 순서대로 표시할 이미지와 현지화 설명 키 목록입니다.
        /// </summary>
        public List<TutorialGuidePageDefinition> guidePages =
            new List<TutorialGuidePageDefinition>();

        /// <summary>
        /// 새 페이지 데이터를 우선하고, 없으면 기존 다중·단일 주소를 설명 없는 페이지로 변환합니다.
        /// </summary>
        /// <param name="result">정규화한 페이지 정의를 저장할 재사용 목록입니다.</param>
        public void CollectGuidePages(List<TutorialGuidePageDefinition> result)
        {
            if (result == null)
            {
                return;
            }

            result.Clear();
            if (guidePages != null && guidePages.Count > 0)
            {
                for (int i = 0; i < guidePages.Count; i++)
                {
                    TutorialGuidePageDefinition page = guidePages[i];
                    if (page == null || string.IsNullOrWhiteSpace(page.spriteAddress))
                    {
                        continue;
                    }

                    result.Add(page);
                }

                if (result.Count > 0)
                {
                    return;
                }
            }

            // 기존 JSON은 설명 키가 없으므로 빈 키를 가진 새 페이지 정의로 런타임에서만 변환합니다.
            if (guideSpriteAddresses != null && guideSpriteAddresses.Count > 0)
            {
                for (int i = 0; i < guideSpriteAddresses.Count; i++)
                {
                    string address = guideSpriteAddresses[i];
                    if (!string.IsNullOrWhiteSpace(address))
                    {
                        result.Add(new TutorialGuidePageDefinition
                        {
                            spriteAddress = address.Trim(),
                        });
                    }
                }

                if (result.Count > 0)
                {
                    return;
                }
            }

            if (!string.IsNullOrWhiteSpace(guideSpriteAddress))
            {
                result.Add(new TutorialGuidePageDefinition
                {
                    spriteAddress = guideSpriteAddress.Trim(),
                });
            }
        }

        /// <summary>
        /// 다중 페이지 주소를 우선하고, 없으면 기존 단일 페이지 주소를 결과 목록에 추가합니다.
        /// </summary>
        /// <param name="result">정규화한 페이지 주소를 저장할 재사용 목록입니다.</param>
        public void CollectGuideSpriteAddresses(List<string> result)
        {
            if (result == null)
            {
                return;
            }

            result.Clear();
            if (guidePages != null && guidePages.Count > 0)
            {
                for (int i = 0; i < guidePages.Count; i++)
                {
                    string address = guidePages[i]?.spriteAddress;
                    if (!string.IsNullOrWhiteSpace(address))
                    {
                        result.Add(address.Trim());
                    }
                }

                if (result.Count > 0)
                {
                    return;
                }
            }

            if (guideSpriteAddresses != null && guideSpriteAddresses.Count > 0)
            {
                for (int i = 0; i < guideSpriteAddresses.Count; i++)
                {
                    string address = guideSpriteAddresses[i];
                    if (!string.IsNullOrWhiteSpace(address))
                    {
                        result.Add(address.Trim());
                    }
                }

                if (result.Count > 0)
                {
                    return;
                }
            }

            if (!string.IsNullOrWhiteSpace(guideSpriteAddress))
            {
                result.Add(guideSpriteAddress.Trim());
            }
        }
    }
}
