using System;
using System.Collections.Generic;
using GGemCo2DTutorial;
using UnityEngine;

namespace GGemCo2DTutorialEditor
{
    /// <summary>
    /// 튜토리얼 제작 단계에서 사용하는 조건 데이터입니다.
    /// 런타임 JSON DTO보다 제작 메모와 기본값 보정에 집중합니다.
    /// </summary>
    [Serializable]
    public sealed class TutorialAuthoringCondition
    {
        [SerializeField]
        private TutorialEventType type;

        [SerializeField]
        private string key;

        [SerializeField]
        private int intValue;

        [SerializeField]
        private int requiredCount = 1;

        [SerializeField]
        [TextArea(1, 3)]
        private string memo;

        /// <summary>
        /// 조건으로 감지할 이벤트 종류입니다.
        /// </summary>
        public TutorialEventType Type
        {
            get => type;
            set => type = value;
        }

        /// <summary>
        /// 입력 액션, UI 대상, 사용자 정의 이벤트 등 문자열 식별자입니다.
        /// </summary>
        public string Key
        {
            get => key;
            set => key = value;
        }

        /// <summary>
        /// 맵, 몬스터, Quest 등 정수 식별자입니다.
        /// </summary>
        public int IntValue
        {
            get => intValue;
            set => intValue = value;
        }

        /// <summary>
        /// 조건이 완료되기 위해 필요한 누적 횟수입니다.
        /// </summary>
        public int RequiredCount
        {
            get => requiredCount;
            set => requiredCount = Mathf.Max(1, value);
        }

        /// <summary>
        /// 제작자가 남기는 조건 설명 메모입니다.
        /// 런타임 JSON으로는 내보내지 않습니다.
        /// </summary>
        public string Memo
        {
            get => memo;
            set => memo = value;
        }

        /// <summary>
        /// 기본 조건 데이터를 생성합니다.
        /// </summary>
        /// <param name="eventType">초기 이벤트 종류입니다.</param>
        /// <returns>기본값이 보정된 제작용 조건 데이터입니다.</returns>
        public static TutorialAuthoringCondition CreateDefault(TutorialEventType eventType = TutorialEventType.None)
        {
            return new TutorialAuthoringCondition
            {
                type = eventType,
                requiredCount = 1,
            };
        }

        /// <summary>
        /// 런타임 Tutorial 조건 DTO로 변환합니다.
        /// </summary>
        /// <returns>런타임 JSON에 저장할 조건 정의입니다.</returns>
        public TutorialConditionDefinition ToRuntimeDefinition()
        {
            return new TutorialConditionDefinition
            {
                type = type,
                key = string.IsNullOrWhiteSpace(key) ? null : key.Trim(),
                intValue = intValue,
                requiredCount = requiredCount > 0 ? requiredCount : 1,
            };
        }

        /// <summary>
        /// 제작 중 잘못 입력될 수 있는 값을 안전한 기본값으로 보정합니다.
        /// </summary>
        public void EnsureDefaults()
        {
            if (requiredCount <= 0)
            {
                requiredCount = 1;
            }
        }
    }

    /// <summary>
    /// 튜토리얼 제작 단계에서 사용하는 액션 데이터입니다.
    /// 런타임 액션 인자 외에 제작 메모와 리스트형 문자열 인자를 보관합니다.
    /// </summary>
    [Serializable]
    public sealed class TutorialAuthoringAction
    {
        [SerializeField]
        private TutorialActionType type;

        [SerializeField]
        private string key;

        [SerializeField]
        private int intValue;

        [SerializeField]
        private List<string> stringValues = new List<string>();

        [SerializeField]
        [TextArea(1, 3)]
        private string memo;

        /// <summary>
        /// 단계 진입 또는 종료 시 실행할 액션 종류입니다.
        /// </summary>
        public TutorialActionType Type
        {
            get => type;
            set => type = value;
        }

        /// <summary>
        /// UI 대상, 기능 키, 사용자 정의 액션 등 문자열 식별자입니다.
        /// </summary>
        public string Key
        {
            get => key;
            set => key = value;
        }

        /// <summary>
        /// Quest UID, 기능 UID 등 정수 인자입니다.
        /// </summary>
        public int IntValue
        {
            get => intValue;
            set => intValue = value;
        }

        /// <summary>
        /// 입력 허용 목록처럼 여러 문자열이 필요한 액션 인자입니다.
        /// </summary>
        public List<string> StringValues => stringValues;

        /// <summary>
        /// 제작자가 남기는 액션 설명 메모입니다.
        /// 런타임 JSON으로는 내보내지 않습니다.
        /// </summary>
        public string Memo
        {
            get => memo;
            set => memo = value;
        }

        /// <summary>
        /// 기본 액션 데이터를 생성합니다.
        /// </summary>
        /// <param name="actionType">초기 액션 종류입니다.</param>
        /// <returns>기본값이 보정된 제작용 액션 데이터입니다.</returns>
        public static TutorialAuthoringAction CreateDefault(TutorialActionType actionType = TutorialActionType.None)
        {
            return new TutorialAuthoringAction
            {
                type = actionType,
                stringValues = new List<string>(),
            };
        }

        /// <summary>
        /// 런타임 Tutorial 액션 DTO로 변환합니다.
        /// </summary>
        /// <returns>런타임 JSON에 저장할 액션 정의입니다.</returns>
        public TutorialActionDefinition ToRuntimeDefinition()
        {
            return new TutorialActionDefinition
            {
                type = type,
                key = string.IsNullOrWhiteSpace(key) ? null : key.Trim(),
                intValue = intValue,
                stringValues = BuildRuntimeStringValues(),
            };
        }

        /// <summary>
        /// 제작 중 잘못 입력될 수 있는 값을 안전한 기본값으로 보정합니다.
        /// </summary>
        public void EnsureDefaults()
        {
            if (stringValues == null)
            {
                stringValues = new List<string>();
            }
        }

        /// <summary>
        /// 빈 문자열을 제외하고 런타임에 전달할 문자열 배열을 생성합니다.
        /// </summary>
        /// <returns>정리된 문자열 배열입니다. 유효한 값이 없으면 null입니다.</returns>
        private string[] BuildRuntimeStringValues()
        {
            if (stringValues == null || stringValues.Count <= 0)
            {
                return null;
            }

            List<string> values = new List<string>(stringValues.Count);
            for (int i = 0; i < stringValues.Count; i++)
            {
                string value = stringValues[i];
                if (!string.IsNullOrWhiteSpace(value))
                {
                    values.Add(value.Trim());
                }
            }

            return values.Count > 0 ? values.ToArray() : null;
        }
    }

    /// <summary>
    /// 튜토리얼 제작 단계에서 사용하는 Step 데이터입니다.
    /// 런타임 Step DTO에 제작용 표시명과 미리보기 문구를 추가로 보관합니다.
    /// </summary>
    [Serializable]
    public sealed class TutorialAuthoringStep
    {
        [SerializeField]
        private int uid = 1;

        [SerializeField]
        private string displayName;

        [SerializeField]
        private string messageKey;

        [SerializeField]
        [TextArea(2, 5)]
        private string guideTextPreview;

        [SerializeField]
        private List<TutorialAuthoringCondition> conditions = new List<TutorialAuthoringCondition>();

        [SerializeField]
        private List<TutorialAuthoringAction> actionsOnEnter = new List<TutorialAuthoringAction>();

        [SerializeField]
        private List<TutorialAuthoringAction> actionsOnExit = new List<TutorialAuthoringAction>();

        /// <summary>
        /// Step 고유 UID입니다.
        /// </summary>
        public int Uid
        {
            get => uid;
            set => uid = value;
        }

        /// <summary>
        /// 제작 툴 목록에 표시할 이름입니다.
        /// 런타임 JSON으로는 내보내지 않습니다.
        /// </summary>
        public string DisplayName
        {
            get => displayName;
            set => displayName = value;
        }

        /// <summary>
        /// 런타임에서 표시할 튜토리얼 안내 문구의 로컬라이즈 키입니다.
        /// </summary>
        public string MessageKey
        {
            get => messageKey;
            set => messageKey = value;
        }

        /// <summary>
        /// 제작자가 확인할 안내 문구 미리보기입니다.
        /// 런타임 JSON으로는 내보내지 않습니다.
        /// </summary>
        public string GuideTextPreview
        {
            get => guideTextPreview;
            set => guideTextPreview = value;
        }

        /// <summary>
        /// Step 완료 조건 목록입니다.
        /// </summary>
        public List<TutorialAuthoringCondition> Conditions => conditions;

        /// <summary>
        /// Step 진입 시 실행할 액션 목록입니다.
        /// </summary>
        public List<TutorialAuthoringAction> ActionsOnEnter => actionsOnEnter;

        /// <summary>
        /// Step 종료 시 실행할 액션 목록입니다.
        /// </summary>
        public List<TutorialAuthoringAction> ActionsOnExit => actionsOnExit;

        /// <summary>
        /// 기본 Step 데이터를 생성합니다.
        /// </summary>
        /// <param name="stepUid">초기 Step UID입니다.</param>
        /// <returns>기본값이 보정된 제작용 Step 데이터입니다.</returns>
        public static TutorialAuthoringStep CreateDefault(int stepUid)
        {
            return new TutorialAuthoringStep
            {
                uid = stepUid > 0 ? stepUid : 1,
                displayName = $"Step {Mathf.Max(1, stepUid)}",
                conditions = new List<TutorialAuthoringCondition>
                {
                    TutorialAuthoringCondition.CreateDefault(),
                },
                actionsOnEnter = new List<TutorialAuthoringAction>(),
                actionsOnExit = new List<TutorialAuthoringAction>(),
            };
        }

        /// <summary>
        /// 런타임 Tutorial Step DTO로 변환합니다.
        /// </summary>
        /// <returns>런타임 JSON에 저장할 Step 정의입니다.</returns>
        public TutorialStepDefinition ToRuntimeDefinition()
        {
            EnsureDefaults(uid);
            return new TutorialStepDefinition
            {
                uid = uid,
                messageKey = string.IsNullOrWhiteSpace(messageKey) ? null : messageKey.Trim(),
                conditions = ConvertConditions(),
                actionsOnEnter = ConvertActions(actionsOnEnter),
                actionsOnExit = ConvertActions(actionsOnExit),
            };
        }

        /// <summary>
        /// 제작 중 잘못 입력될 수 있는 값을 안전한 기본값으로 보정합니다.
        /// </summary>
        /// <param name="fallbackUid">UID가 비어 있을 때 사용할 대체 UID입니다.</param>
        public void EnsureDefaults(int fallbackUid)
        {
            if (uid <= 0)
            {
                uid = fallbackUid > 0 ? fallbackUid : 1;
            }

            if (string.IsNullOrWhiteSpace(displayName))
            {
                displayName = $"Step {uid}";
            }

            if (conditions == null)
            {
                conditions = new List<TutorialAuthoringCondition>();
            }

            if (actionsOnEnter == null)
            {
                actionsOnEnter = new List<TutorialAuthoringAction>();
            }

            if (actionsOnExit == null)
            {
                actionsOnExit = new List<TutorialAuthoringAction>();
            }

            EnsureConditionDefaults(conditions);
            EnsureActionDefaults(actionsOnEnter);
            EnsureActionDefaults(actionsOnExit);
        }

        /// <summary>
        /// 제작용 조건 목록을 런타임 조건 목록으로 변환합니다.
        /// </summary>
        /// <returns>런타임 조건 목록입니다.</returns>
        private List<TutorialConditionDefinition> ConvertConditions()
        {
            List<TutorialConditionDefinition> result = new List<TutorialConditionDefinition>();
            if (conditions == null)
            {
                return result;
            }

            for (int i = 0; i < conditions.Count; i++)
            {
                TutorialAuthoringCondition condition = conditions[i];
                if (condition == null || condition.Type == TutorialEventType.None)
                {
                    continue;
                }

                result.Add(condition.ToRuntimeDefinition());
            }

            return result;
        }

        /// <summary>
        /// 제작용 액션 목록을 런타임 액션 목록으로 변환합니다.
        /// </summary>
        /// <param name="source">변환할 제작용 액션 목록입니다.</param>
        /// <returns>런타임 액션 목록입니다.</returns>
        private static List<TutorialActionDefinition> ConvertActions(List<TutorialAuthoringAction> source)
        {
            List<TutorialActionDefinition> result = new List<TutorialActionDefinition>();
            if (source == null)
            {
                return result;
            }

            for (int i = 0; i < source.Count; i++)
            {
                TutorialAuthoringAction action = source[i];
                if (action == null || action.Type == TutorialActionType.None)
                {
                    continue;
                }

                result.Add(action.ToRuntimeDefinition());
            }

            return result;
        }

        /// <summary>
        /// 조건 목록의 null 항목과 기본값을 정리합니다.
        /// </summary>
        /// <param name="source">정리할 조건 목록입니다.</param>
        private static void EnsureConditionDefaults(List<TutorialAuthoringCondition> source)
        {
            if (source == null)
            {
                return;
            }

            for (int i = source.Count - 1; i >= 0; i--)
            {
                if (source[i] == null)
                {
                    source.RemoveAt(i);
                    continue;
                }

                source[i].EnsureDefaults();
            }
        }

        /// <summary>
        /// 액션 목록의 null 항목과 기본값을 정리합니다.
        /// </summary>
        /// <param name="source">정리할 액션 목록입니다.</param>
        private static void EnsureActionDefaults(List<TutorialAuthoringAction> source)
        {
            if (source == null)
            {
                return;
            }

            for (int i = source.Count - 1; i >= 0; i--)
            {
                if (source[i] == null)
                {
                    source.RemoveAt(i);
                    continue;
                }

                source[i].EnsureDefaults();
            }
        }
    }
}
