using System;
using System.Collections.Generic;
using GGemCo2DTutorial;
using UnityEngine;

namespace GGemCo2DTutorialEditor
{
    /// <summary>
    /// 생성툴에서 편집하는 UID·enum 기반 튜토리얼 조건입니다.
    /// </summary>
    [Serializable]
    public sealed class TutorialAuthoringCondition
    {
        [SerializeField] private TutorialEventType type;
        [SerializeField] private int targetUid;
        [SerializeField] private TutorialInputActionType inputAction;
        [SerializeField] private int intValue;
        [SerializeField] private float floatValue;
        [SerializeField] private int requiredCount = 1;
        [SerializeField, TextArea(1, 3)] private string memo;

        public TutorialEventType Type { get => type; set => type = value; }
        public int TargetUid { get => targetUid; set => targetUid = Mathf.Max(0, value); }
        public TutorialInputActionType InputAction { get => inputAction; set => inputAction = value; }
        public int IntValue { get => intValue; set => intValue = value; }
        public float FloatValue { get => floatValue; set => floatValue = Mathf.Max(0f, value); }
        public int RequiredCount { get => requiredCount; set => requiredCount = Mathf.Max(1, value); }
        public string Memo { get => memo; set => memo = value; }

        /// <summary>
        /// 지정한 이벤트 종류의 기본 조건을 생성합니다.
        /// </summary>
        public static TutorialAuthoringCondition CreateDefault(
            TutorialEventType eventType = TutorialEventType.None)
        {
            return new TutorialAuthoringCondition
            {
                type = eventType,
                requiredCount = 1,
            };
        }

        /// <summary>
        /// 생성툴 조건을 런타임 DTO로 변환합니다.
        /// </summary>
        public TutorialConditionDefinition ToRuntimeDefinition()
        {
            EnsureDefaults();
            return new TutorialConditionDefinition
            {
                type = type,
                targetUid = targetUid,
                inputAction = inputAction,
                intValue = intValue,
                floatValue = floatValue,
                requiredCount = requiredCount,
            };
        }

        public void EnsureDefaults()
        {
            targetUid = Mathf.Max(0, targetUid);
            floatValue = Mathf.Max(0f, floatValue);
            requiredCount = Mathf.Max(1, requiredCount);
        }
    }

    /// <summary>
    /// 생성툴에서 편집하는 UID·enum 기반 튜토리얼 액션입니다.
    /// </summary>
    [Serializable]
    public sealed class TutorialAuthoringAction
    {
        [SerializeField] private TutorialActionType type;
        [SerializeField] private int targetUid;
        [SerializeField] private int intValue;
        [SerializeField] private TutorialInputActionMask inputMask;
        [SerializeField] private TutorialGameplayState gameplayState;
        [SerializeField, TextArea(1, 3)] private string memo;

        public TutorialActionType Type { get => type; set => type = value; }
        public int TargetUid { get => targetUid; set => targetUid = Mathf.Max(0, value); }
        public int IntValue { get => intValue; set => intValue = value; }
        public TutorialInputActionMask InputMask { get => inputMask; set => inputMask = value; }
        public TutorialGameplayState GameplayState { get => gameplayState; set => gameplayState = value; }
        public string Memo { get => memo; set => memo = value; }

        /// <summary>
        /// 지정한 액션 종류의 기본 데이터를 생성합니다.
        /// </summary>
        public static TutorialAuthoringAction CreateDefault(
            TutorialActionType actionType = TutorialActionType.None)
        {
            return new TutorialAuthoringAction { type = actionType };
        }

        /// <summary>
        /// 생성툴 액션을 런타임 DTO로 변환합니다.
        /// </summary>
        public TutorialActionDefinition ToRuntimeDefinition()
        {
            return new TutorialActionDefinition
            {
                type = type,
                targetUid = targetUid,
                intValue = intValue,
                inputMask = inputMask,
                gameplayState = gameplayState,
            };
        }

        public void EnsureDefaults()
        {
            targetUid = Mathf.Max(0, targetUid);
        }
    }

    /// <summary>
    /// 생성툴에서 Project 창의 Sprite를 직접 연결하는 가이드 이미지 항목입니다.
    /// </summary>
    [Serializable]
    public sealed class TutorialAuthoringGuide
    {
        [SerializeField] private int uid = 1;
        [SerializeField] private string displayName;
        [SerializeField] private Sprite sprite;

        public int Uid { get => uid; set => uid = Mathf.Max(1, value); }
        public string DisplayName { get => displayName; set => displayName = value; }
        public Sprite Sprite { get => sprite; set => sprite = value; }
    }

    /// <summary>
    /// 생성툴에서 편집하는 튜토리얼 단계입니다.
    /// </summary>
    [Serializable]
    public sealed class TutorialAuthoringStep
    {
        [SerializeField] private int uid = 1;
        [SerializeField] private string displayName;
        [SerializeField] private int messageUid;
        [SerializeField, TextArea(2, 5)] private string guideTextPreview;
        [SerializeField] private List<TutorialAuthoringCondition> conditions =
            new List<TutorialAuthoringCondition>();
        [SerializeField] private List<TutorialAuthoringAction> actionsOnEnter =
            new List<TutorialAuthoringAction>();
        [SerializeField] private List<TutorialAuthoringAction> actionsOnExit =
            new List<TutorialAuthoringAction>();

        public int Uid { get => uid; set => uid = Mathf.Max(1, value); }
        public string DisplayName { get => displayName; set => displayName = value; }
        public int MessageUid { get => messageUid; set => messageUid = Mathf.Max(0, value); }
        public string GuideTextPreview { get => guideTextPreview; set => guideTextPreview = value; }
        public List<TutorialAuthoringCondition> Conditions => conditions;
        public List<TutorialAuthoringAction> ActionsOnEnter => actionsOnEnter;
        public List<TutorialAuthoringAction> ActionsOnExit => actionsOnExit;

        /// <summary>
        /// 기본 완료 조건을 포함한 새 단계를 생성합니다.
        /// </summary>
        public static TutorialAuthoringStep CreateDefault(int stepUid)
        {
            int validUid = Mathf.Max(1, stepUid);
            return new TutorialAuthoringStep
            {
                uid = validUid,
                displayName = $"Step {validUid}",
                conditions = new List<TutorialAuthoringCondition>
                {
                    TutorialAuthoringCondition.CreateDefault(),
                },
            };
        }

        /// <summary>
        /// 생성툴 단계를 런타임 DTO로 변환합니다.
        /// </summary>
        public TutorialStepDefinition ToRuntimeDefinition()
        {
            EnsureDefaults(uid);
            return new TutorialStepDefinition
            {
                uid = uid,
                messageUid = messageUid,
                conditions = ConvertConditions(),
                actionsOnEnter = ConvertActions(actionsOnEnter),
                actionsOnExit = ConvertActions(actionsOnExit),
            };
        }

        public void EnsureDefaults(int fallbackUid)
        {
            uid = uid > 0 ? uid : Mathf.Max(1, fallbackUid);
            messageUid = Mathf.Max(0, messageUid);
            displayName = string.IsNullOrWhiteSpace(displayName) ? $"Step {uid}" : displayName;
            conditions ??= new List<TutorialAuthoringCondition>();
            actionsOnEnter ??= new List<TutorialAuthoringAction>();
            actionsOnExit ??= new List<TutorialAuthoringAction>();
            EnsureConditions(conditions);
            EnsureActions(actionsOnEnter);
            EnsureActions(actionsOnExit);
        }

        private List<TutorialConditionDefinition> ConvertConditions()
        {
            List<TutorialConditionDefinition> result = new List<TutorialConditionDefinition>();
            for (int i = 0; i < conditions.Count; i++)
            {
                TutorialAuthoringCondition condition = conditions[i];
                if (condition != null && condition.Type != TutorialEventType.None)
                {
                    result.Add(condition.ToRuntimeDefinition());
                }
            }

            return result;
        }

        private static List<TutorialActionDefinition> ConvertActions(
            List<TutorialAuthoringAction> source)
        {
            List<TutorialActionDefinition> result = new List<TutorialActionDefinition>();
            for (int i = 0; i < source.Count; i++)
            {
                TutorialAuthoringAction action = source[i];
                if (action != null && action.Type != TutorialActionType.None)
                {
                    result.Add(action.ToRuntimeDefinition());
                }
            }

            return result;
        }

        private static void EnsureConditions(List<TutorialAuthoringCondition> source)
        {
            for (int i = source.Count - 1; i >= 0; i--)
            {
                if (source[i] == null)
                {
                    source.RemoveAt(i);
                }
                else
                {
                    source[i].EnsureDefaults();
                }
            }
        }

        private static void EnsureActions(List<TutorialAuthoringAction> source)
        {
            for (int i = source.Count - 1; i >= 0; i--)
            {
                if (source[i] == null)
                {
                    source.RemoveAt(i);
                }
                else
                {
                    source[i].EnsureDefaults();
                }
            }
        }
    }
}
