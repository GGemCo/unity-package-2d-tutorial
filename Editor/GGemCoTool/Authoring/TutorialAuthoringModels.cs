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
    /// 생성툴에서 편집하는 가이드 이미지, Addressables 주소, 현지화 설명 키입니다.
    /// </summary>
    [Serializable]
    public sealed class TutorialAuthoringGuidePage
    {
        [SerializeField] private Sprite guideSprite;
        [SerializeField, TextArea(2, 5)] private string descriptionSourceKo;
        [SerializeField, HideInInspector] private int localizationUid;
        [SerializeField, HideInInspector] private string descriptionLocalizationKey;
        [SerializeField, HideInInspector] private string guideSpriteAddress;

        public Sprite GuideSprite { get => guideSprite; set => guideSprite = value; }
        public string DescriptionSourceKo
        {
            get => descriptionSourceKo;
            set => descriptionSourceKo = Normalize(value);
        }
        public int LocalizationUid => localizationUid;
        public string DescriptionLocalizationKey
        {
            get => descriptionLocalizationKey;
            set => descriptionLocalizationKey = Normalize(value);
        }
        public string GuideSpriteAddress => guideSpriteAddress;

        /// <summary>
        /// 이미지와 선택적인 기존 주소를 사용하여 새 제작 페이지를 생성합니다.
        /// </summary>
        /// <param name="sprite">표시할 가이드 Sprite입니다.</param>
        /// <param name="address">기존 JSON 또는 Addressables에서 복원한 주소입니다.</param>
        /// <param name="localizationKey">설명 문자열을 조회할 Localization 키입니다.</param>
        /// <returns>초기화된 제작 페이지입니다.</returns>
        public static TutorialAuthoringGuidePage Create(
            Sprite sprite = null,
            string address = null,
            string localizationKey = null)
        {
            return new TutorialAuthoringGuidePage
            {
                guideSprite = sprite,
                guideSpriteAddress = Normalize(address),
                descriptionLocalizationKey = Normalize(localizationKey),
            };
        }

        /// <summary>
        /// Localization Export에서 사용할 영구 UID를 설정합니다.
        /// 페이지 순서가 변경되어도 한번 발급된 UID는 유지합니다.
        /// </summary>
        /// <param name="value">튜토리얼 내부에서 고유한 양수 UID입니다.</param>
        /// <returns>기존 값과 달라졌으면 true입니다.</returns>
        public bool SetLocalizationUid(int value)
        {
            int normalized = Mathf.Max(0, value);
            if (localizationUid == normalized)
            {
                return false;
            }

            localizationUid = normalized;
            return true;
        }

        /// <summary>
        /// 자동 생성하거나 기존 데이터에서 복원한 Localization 키를 저장합니다.
        /// </summary>
        /// <param name="value">문자열 테이블에서 사용할 엔트리 키입니다.</param>
        /// <returns>기존 값과 달라졌으면 true입니다.</returns>
        public bool SetDescriptionLocalizationKey(string value)
        {
            string normalized = Normalize(value);
            if (string.Equals(
                    descriptionLocalizationKey,
                    normalized,
                    StringComparison.Ordinal))
            {
                return false;
            }

            descriptionLocalizationKey = normalized;
            return true;
        }

        /// <summary>
        /// 같은 JSON 페이지에서 복원된 기존 Editor 전용 Localization 제작 값을 보존합니다.
        /// 런타임 키가 다르면 서로 다른 페이지로 간주하여 원문과 UID를 복사하지 않습니다.
        /// </summary>
        /// <param name="source">기존 TutorialAuthoringAsset에 저장된 페이지입니다.</param>
        public void RestoreLocalizationAuthoring(TutorialAuthoringGuidePage source)
        {
            if (source == null ||
                !string.Equals(
                    descriptionLocalizationKey,
                    source.descriptionLocalizationKey,
                    StringComparison.Ordinal))
            {
                return;
            }

            localizationUid = Mathf.Max(0, source.localizationUid);
            descriptionSourceKo = Normalize(source.descriptionSourceKo);
        }

        /// <summary>
        /// Addressables 동기화 결과로 계산된 Sprite 런타임 주소를 저장합니다.
        /// </summary>
        /// <param name="address">새 런타임 주소입니다.</param>
        /// <returns>기존 값과 달라졌으면 true입니다.</returns>
        public bool SetGuideSpriteAddress(string address)
        {
            string normalized = Normalize(address);
            if (string.Equals(guideSpriteAddress, normalized, StringComparison.Ordinal))
            {
                return false;
            }

            guideSpriteAddress = normalized;
            return true;
        }

        /// <summary>
        /// 제작 페이지를 런타임 JSON 페이지 정의로 변환합니다.
        /// </summary>
        /// <returns>정규화된 런타임 페이지 정의입니다.</returns>
        public TutorialGuidePageDefinition ToRuntimeDefinition()
        {
            return new TutorialGuidePageDefinition
            {
                spriteAddress = Normalize(guideSpriteAddress),
                descriptionLocalizationKey = Normalize(descriptionLocalizationKey),
            };
        }

        /// <summary>
        /// 직렬화된 문자열 값의 앞뒤 공백을 제거하고 빈 값은 null로 정규화합니다.
        /// </summary>
        public void EnsureDefaults()
        {
            guideSpriteAddress = Normalize(guideSpriteAddress);
            descriptionSourceKo = Normalize(descriptionSourceKo);
            localizationUid = Mathf.Max(0, localizationUid);
            descriptionLocalizationKey = Normalize(descriptionLocalizationKey);
        }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
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
        [SerializeField] private List<TutorialAuthoringGuidePage> guidePages =
            new List<TutorialAuthoringGuidePage>();
        [SerializeField, HideInInspector] private List<Sprite> guideSprites = new List<Sprite>();
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
        public List<TutorialAuthoringGuidePage> GuidePages =>
            guidePages ??= new List<TutorialAuthoringGuidePage>();

        /// <summary>
        /// 기존 제작 API 호환용 Sprite 목록입니다. 새 코드에서는 <see cref="GuidePages"/>를 사용합니다.
        /// </summary>
        [Obsolete("GuidePages를 사용하십시오.")]
        public List<Sprite> GuideSprites => guideSprites ??= new List<Sprite>();

        /// <summary>
        /// 기존 제작 API 호환용 Addressables 주소 목록입니다.
        /// </summary>
        [Obsolete("GuidePages의 GuideSpriteAddress를 사용하십시오.")]
        public IReadOnlyList<string> GuideSpriteAddresses =>
            guideSpriteAddresses ??= new List<string>();

        public string Memo { get => memo; set => memo = value; }

        /// <summary>
        /// 기존 단일 페이지 제작 API와의 호환성을 위해 첫 번째 가이드 Sprite를 제공합니다.
        /// </summary>
        public Sprite GuideSprite
        {
            get => GuidePages.Count > 0 ? GuidePages[0]?.GuideSprite : guideSprite;
            set
            {
                if (value == null)
                {
                    GuidePages.Clear();
                    guideSprite = null;
                    return;
                }

                if (GuidePages.Count == 0)
                {
                    GuidePages.Add(TutorialAuthoringGuidePage.Create(value));
                }
                else
                {
                    GuidePages[0] ??= TutorialAuthoringGuidePage.Create();
                    GuidePages[0].GuideSprite = value;
                }
            }
        }

        /// <summary>
        /// 기존 단일 페이지 제작 API와의 호환성을 위해 첫 번째 가이드 주소를 제공합니다.
        /// </summary>
        public string GuideSpriteAddress =>
            GuidePages.Count > 0
                ? GuidePages[0]?.GuideSpriteAddress
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
            List<TutorialGuidePageDefinition> runtimePages =
                new List<TutorialGuidePageDefinition>(GuidePages.Count);
            if (type == TutorialActionType.ShowGuide)
            {
                for (int i = 0; i < GuidePages.Count; i++)
                {
                    if (GuidePages[i] != null)
                    {
                        runtimePages.Add(GuidePages[i].ToRuntimeDefinition());
                    }
                }
            }

            return new TutorialActionDefinition
            {
                type = type,
                targetUid = targetUid,
                intValue = intValue,
                inputMask = inputMask,
                gameplayState = gameplayState,
                guidePages = runtimePages,
                guideSpriteAddresses = new List<string>(),
            };
        }

        /// <summary>
        /// Addressables 동기화 결과로 계산된 가이드 페이지 주소 목록을 저장합니다.
        /// </summary>
        /// <param name="addresses">페이지 순서대로 정렬된 Sprite 런타임 주소 목록입니다.</param>
        /// <returns>기존 주소 목록과 달라졌으면 true입니다.</returns>
        public bool SetGuideSpriteAddresses(IReadOnlyList<string> addresses)
        {
            int sourceCount = addresses?.Count ?? 0;
            if (GuidePages.Count == 0)
            {
                guideSpriteAddresses ??= new List<string>();
                guideSpriteAddresses.Clear();
                for (int i = 0; i < sourceCount; i++)
                {
                    guideSpriteAddresses.Add(addresses[i]);
                }

                return sourceCount > 0;
            }

            bool changed = false;
            for (int i = 0; i < GuidePages.Count; i++)
            {
                string address = i < sourceCount ? addresses[i] : null;
                if (GuidePages[i] != null)
                {
                    changed |= GuidePages[i].SetGuideSpriteAddress(address);
                }
            }

            return changed;
        }

        /// <summary>
        /// 기존 단일 페이지 제작 API와의 호환성을 위해 첫 번째 가이드 주소를 설정합니다.
        /// </summary>
        /// <param name="address">첫 번째 페이지의 Sprite 런타임 주소입니다.</param>
        public void SetGuideSpriteAddress(string address)
        {
            if (GuidePages.Count == 0)
            {
                guideSpriteAddress = address;
                return;
            }

            GuidePages[0] ??= TutorialAuthoringGuidePage.Create();
            GuidePages[0].SetGuideSpriteAddress(address);
        }

        /// <summary>
        /// 기존 Guide UID 기반 제작 데이터를 직접 페이지 참조 방식으로 변환합니다.
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

            if (GuidePages.Count > 0 ||
                (guideSprites != null && guideSprites.Count > 0))
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

                GuidePages.Add(TutorialAuthoringGuidePage.Create(guide.Sprite));
                targetUid = 0;
                return true;
            }

            return false;
        }

        /// <summary>
        /// 액션 값을 보정하고 기존 Sprite 목록을 새 페이지 데이터로 이전합니다.
        /// </summary>
        public void EnsureDefaults()
        {
            targetUid = Mathf.Max(0, targetUid);
            guidePages ??= new List<TutorialAuthoringGuidePage>();
            guideSprites ??= new List<Sprite>();
            guideSpriteAddresses ??= new List<string>();
            if (type == TutorialActionType.ShowGuide)
            {
                targetUid = 0;
                MigrateLegacyGuidePages();
                for (int i = guidePages.Count - 1; i >= 0; i--)
                {
                    if (guidePages[i] == null)
                    {
                        guidePages.RemoveAt(i);
                    }
                    else
                    {
                        guidePages[i].EnsureDefaults();
                    }
                }
            }
            else
            {
                guidePages.Clear();
            }

            // 이전 필드는 변환 뒤 비워 중복 데이터가 다시 JSON으로 유입되지 않게 합니다.
            guideSprites.Clear();
            guideSpriteAddresses.Clear();
            guideSprite = null;
            guideSpriteAddress = null;
        }

        /// <summary>
        /// 기존 단일·다중 Sprite와 주소 목록을 같은 순서의 새 페이지 객체로 이전합니다.
        /// </summary>
        private void MigrateLegacyGuidePages()
        {
            if (guidePages.Count > 0)
            {
                return;
            }

            if (guideSprites.Count == 0 && guideSprite != null)
            {
                guideSprites.Add(guideSprite);
            }

            if (guideSpriteAddresses.Count == 0 &&
                !string.IsNullOrWhiteSpace(guideSpriteAddress))
            {
                guideSpriteAddresses.Add(guideSpriteAddress);
            }

            int pageCount = Mathf.Max(guideSprites.Count, guideSpriteAddresses.Count);
            for (int i = 0; i < pageCount; i++)
            {
                Sprite sprite = i < guideSprites.Count ? guideSprites[i] : null;
                string address =
                    i < guideSpriteAddresses.Count ? guideSpriteAddresses[i] : null;
                guidePages.Add(TutorialAuthoringGuidePage.Create(sprite, address));
            }
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
