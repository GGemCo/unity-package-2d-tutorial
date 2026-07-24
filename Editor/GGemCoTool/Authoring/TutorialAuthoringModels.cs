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

        /// <summary>
        /// 조건 값을 유효 범위로 보정하고 가이드 완료 조건 정책을 적용합니다.
        /// </summary>
        public void EnsureDefaults()
        {
            targetUid = Mathf.Max(0, targetUid);
            if (type == TutorialEventType.GuideClicked ||
                type == TutorialEventType.GuideClosed)
            {
                targetUid = 0;
            }
            else if (type == TutorialEventType.CombatStarted)
            {
                targetUid = 0;
            }

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
        [SerializeField] private List<Sprite> guideSprites = new List<Sprite>();
        [SerializeField, HideInInspector] private Sprite guideSprite;
        [SerializeField, HideInInspector] private List<string> guideSpriteAddresses =
            new List<string>();
        [SerializeField, HideInInspector] private string guideSpriteAddress;
        [SerializeField, TextArea(1, 3)] private string memo;

        public TutorialActionType Type { get => type; set => type = value; }
        public int TargetUid { get => targetUid; set => targetUid = Mathf.Max(0, value); }
        public int IntValue { get => intValue; set => intValue = value; }
        public TutorialInputActionMask InputMask { get => inputMask; set => inputMask = value; }
        public TutorialGameplayState GameplayState { get => gameplayState; set => gameplayState = value; }
        public List<Sprite> GuideSprites => guideSprites ??= new List<Sprite>();
        public IReadOnlyList<string> GuideSpriteAddresses => guideSpriteAddresses;
        public string Memo { get => memo; set => memo = value; }

        /// <summary>
        /// 기존 단일 페이지 제작 API와의 호환성을 위해 첫 번째 가이드 Sprite를 제공합니다.
        /// </summary>
        public Sprite GuideSprite
        {
            get => GuideSprites.Count > 0 ? GuideSprites[0] : guideSprite;
            set
            {
                if (value == null)
                {
                    GuideSprites.Clear();
                    guideSprite = null;
                    return;
                }

                if (GuideSprites.Count == 0)
                {
                    GuideSprites.Add(value);
                }
                else
                {
                    GuideSprites[0] = value;
                }
            }
        }

        /// <summary>
        /// 기존 단일 페이지 제작 API와의 호환성을 위해 첫 번째 가이드 주소를 제공합니다.
        /// </summary>
        public string GuideSpriteAddress =>
            guideSpriteAddresses != null && guideSpriteAddresses.Count > 0
                ? guideSpriteAddresses[0]
                : guideSpriteAddress;

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
            EnsureDefaults();
            return new TutorialActionDefinition
            {
                type = type,
                targetUid = targetUid,
                intValue = intValue,
                inputMask = inputMask,
                gameplayState = gameplayState,
                guideSpriteAddress = type == TutorialActionType.ShowGuide
                    ? guideSpriteAddress
                    : null,
                guideSpriteAddresses = type == TutorialActionType.ShowGuide
                    ? new List<string>(guideSpriteAddresses)
                    : new List<string>(),
            };
        }

        /// <summary>
        /// Addressables 동기화 결과로 계산된 가이드 페이지 주소 목록을 저장합니다.
        /// </summary>
        /// <param name="addresses">페이지 순서대로 정렬된 Sprite 런타임 주소 목록입니다.</param>
        /// <returns>기존 주소 목록과 달라졌으면 true입니다.</returns>
        public bool SetGuideSpriteAddresses(IReadOnlyList<string> addresses)
        {
            guideSpriteAddresses ??= new List<string>();
            int sourceCount = addresses?.Count ?? 0;
            bool changed = guideSpriteAddresses.Count != sourceCount;
            if (!changed)
            {
                for (int i = 0; i < sourceCount; i++)
                {
                    string normalized = NormalizeAddress(addresses[i]);
                    if (!string.Equals(
                            guideSpriteAddresses[i],
                            normalized,
                            StringComparison.Ordinal))
                    {
                        changed = true;
                        break;
                    }
                }
            }

            if (!changed)
            {
                return false;
            }

            guideSpriteAddresses.Clear();
            for (int i = 0; i < sourceCount; i++)
            {
                guideSpriteAddresses.Add(NormalizeAddress(addresses[i]));
            }

            guideSpriteAddress = null;
            return true;
        }

        /// <summary>
        /// 기존 단일 페이지 제작 API와의 호환성을 위해 첫 번째 가이드 주소를 설정합니다.
        /// </summary>
        /// <param name="address">첫 번째 페이지의 Sprite 런타임 주소입니다.</param>
        public void SetGuideSpriteAddress(string address)
        {
            string normalized = NormalizeAddress(address);
            guideSpriteAddresses ??= new List<string>();
            if (normalized == null)
            {
                guideSpriteAddresses.Clear();
                guideSpriteAddress = null;
                return;
            }

            if (guideSpriteAddresses.Count == 0)
            {
                guideSpriteAddresses.Add(normalized);
            }
            else
            {
                guideSpriteAddresses[0] = normalized;
            }

            guideSpriteAddress = null;
        }

        /// <summary>
        /// 기존 Guide UID 기반 제작 데이터를 직접 Sprite 참조 방식으로 변환합니다.
        /// </summary>
        /// <param name="legacyGuides">이전 제작 에셋에 저장된 가이드 목록입니다.</param>
        /// <returns>변환할 데이터가 없거나 Sprite 변환에 성공하면 true입니다.</returns>
        public bool TryMigrateLegacyGuide(
            IReadOnlyList<TutorialAuthoringGuide> legacyGuides)
        {
            if (type != TutorialActionType.ShowGuide || targetUid <= 0)
            {
                return true;
            }

            if (guideSprites != null && guideSprites.Count > 0)
            {
                targetUid = 0;
                return true;
            }

            if (legacyGuides == null)
            {
                return false;
            }

            for (int i = 0; i < legacyGuides.Count; i++)
            {
                TutorialAuthoringGuide guide = legacyGuides[i];
                if (guide == null || guide.Uid != targetUid || guide.Sprite == null)
                {
                    continue;
                }

                guideSprites ??= new List<Sprite>();
                guideSprites.Add(guide.Sprite);
                targetUid = 0;
                return true;
            }

            return false;
        }

        /// <summary>
        /// 액션 값을 보정하고 현재 액션 타입에서 사용하지 않는 가이드 참조를 정리합니다.
        /// </summary>
        public void EnsureDefaults()
        {
            targetUid = Mathf.Max(0, targetUid);
            guideSprites ??= new List<Sprite>();
            guideSpriteAddresses ??= new List<string>();
            if (type == TutorialActionType.ShowGuide)
            {
                targetUid = 0;
                MigrateSingleGuidePage();
            }

            if (type != TutorialActionType.ShowGuide)
            {
                guideSprites.Clear();
                guideSpriteAddresses.Clear();
                guideSprite = null;
                guideSpriteAddress = null;
            }
            else if (guideSprites.Count == 0)
            {
                guideSpriteAddresses.Clear();
                guideSpriteAddress = null;
            }
        }

        /// <summary>
        /// 기존 단일 Sprite와 단일 주소를 새 페이지 목록으로 이전합니다.
        /// </summary>
        private void MigrateSingleGuidePage()
        {
            if (guideSprites.Count == 0 && guideSprite != null)
            {
                guideSprites.Add(guideSprite);
            }

            if (guideSpriteAddresses.Count == 0 &&
                !string.IsNullOrWhiteSpace(guideSpriteAddress))
            {
                guideSpriteAddresses.Add(guideSpriteAddress.Trim());
            }

            guideSprite = null;
            guideSpriteAddress = null;
        }

        /// <summary>
        /// Addressables 주소의 공백을 제거하고 빈 값은 null로 정규화합니다.
        /// </summary>
        /// <param name="address">정규화할 주소입니다.</param>
        /// <returns>정규화된 주소 또는 null입니다.</returns>
        private static string NormalizeAddress(string address)
        {
            return string.IsNullOrWhiteSpace(address)
                ? null
                : address.Trim();
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

        /// <summary>
        /// 단계 UID와 내부 목록을 유효한 제작 상태로 보정합니다.
        /// </summary>
        /// <param name="fallbackUid">UID가 없을 때 사용할 기본 UID입니다.</param>
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

        /// <summary>
        /// 유효한 제작 조건만 런타임 조건 목록으로 변환합니다.
        /// </summary>
        /// <returns>런타임 조건 목록입니다.</returns>
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

        /// <summary>
        /// 유효한 제작 액션만 런타임 액션 목록으로 변환합니다.
        /// </summary>
        /// <param name="source">변환할 제작 액션 목록입니다.</param>
        /// <returns>런타임 액션 목록입니다.</returns>
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

        /// <summary>
        /// null 조건을 제거하고 남은 조건 값을 보정합니다.
        /// </summary>
        /// <param name="source">보정할 조건 목록입니다.</param>
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

        /// <summary>
        /// null 액션을 제거하고 남은 액션 값을 보정합니다.
        /// </summary>
        /// <param name="source">보정할 액션 목록입니다.</param>
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

        /// <summary>
        /// 이 단계에 포함된 모든 액션을 지정 목록에 추가합니다.
        /// </summary>
        /// <param name="result">액션을 누적할 목록입니다.</param>
        public void CollectActions(List<TutorialAuthoringAction> result)
        {
            if (result == null)
            {
                return;
            }

            CollectActions(actionsOnEnter, result);
            CollectActions(actionsOnExit, result);
        }

        /// <summary>
        /// 지정 액션 목록의 유효한 항목을 결과 목록에 추가합니다.
        /// </summary>
        /// <param name="source">수집할 원본 액션 목록입니다.</param>
        /// <param name="result">액션을 누적할 결과 목록입니다.</param>
        private static void CollectActions(
            IReadOnlyList<TutorialAuthoringAction> source,
            List<TutorialAuthoringAction> result)
        {
            for (int i = 0; i < source.Count; i++)
            {
                if (source[i] != null)
                {
                    result.Add(source[i]);
                }
            }
        }
    }
}
