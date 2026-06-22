using System;
using System.Collections.Generic;
using GGemCo2DCore;

namespace GGemCo2DTutorial
{
    /// <summary>
    /// 개별 튜토리얼의 현재 단계와 완료 상태입니다.
    /// </summary>
    [Serializable]
    public sealed class TutorialProgressData
    {
        /// <summary>
        /// 진행 상태가 속한 Tutorial UID입니다.
        /// </summary>
        public int TutorialUid;

        /// <summary>
        /// 다음 실행 시 이어서 시작할 단계 인덱스입니다.
        /// </summary>
        public int StepIndex;

        /// <summary>
        /// Tutorial 완료 여부입니다.
        /// </summary>
        public bool IsCompleted;
    }

    /// <summary>
    /// Tutorial 진행 상태를 전용 저장 파일과 Core 저장 확장 섹션에 기록하고 복원합니다.
    /// </summary>
    public sealed class TutorialData : ISaveContributor
    {
        /// <summary>
        /// 튜토리얼 UID별 진행 상태입니다.
        /// </summary>
        public Dictionary<int, TutorialProgressData> Progress =
            new Dictionary<int, TutorialProgressData>();

        /// <inheritdoc />
        public string SectionKey => TutorialConstants.SaveSectionKey;

        /// <summary>
        /// Tutorial 전용 저장 파일에서 로드한 진행 데이터를 현재 런타임 데이터에 반영합니다.
        /// 전용 저장 데이터가 없으면 먼저 복원된 Core 확장 섹션 데이터를 유지합니다.
        /// </summary>
        /// <param name="saveDataContainer">로딩 씬에서 역직렬화한 Tutorial 저장 컨테이너입니다.</param>
        public void Initialize(SaveDataContainerTutorial saveDataContainer)
        {
            Dictionary<int, TutorialProgressData> loadedProgress =
                saveDataContainer?.TutorialData?.Progress;
            if (loadedProgress == null)
            {
                return;
            }

            Progress = CloneProgress(loadedProgress);
        }

        /// <summary>
        /// Tutorial 저장 기여자를 Core 저장 레지스트리에 등록합니다.
        /// </summary>
        public void Register()
        {
            SaveRegistry.Register(this);
        }

        /// <summary>
        /// Tutorial 저장 기여자 등록을 해제합니다.
        /// </summary>
        public void Unregister()
        {
            SaveRegistry.Unregister(this);
        }

        /// <inheritdoc />
        public void Capture(SaveEnvelope env)
        {
            env?.SetSection(SectionKey, new TutorialProgressSnapshot
            {
                Progress = CloneProgress(Progress),
            });
        }

        /// <inheritdoc />
        public void Restore(SaveEnvelope env)
        {
            Progress.Clear();
            if (env == null ||
                !env.TryGetSection(SectionKey, out TutorialProgressSnapshot snapshot) ||
                snapshot?.Progress == null)
            {
                return;
            }

            Progress = CloneProgress(snapshot.Progress);
        }

        /// <summary>
        /// 지정한 튜토리얼이 완료되었는지 확인합니다.
        /// </summary>
        public bool IsCompleted(int tutorialUid)
        {
            return Progress.TryGetValue(tutorialUid, out TutorialProgressData data) &&
                   data != null &&
                   data.IsCompleted;
        }

        /// <summary>
        /// 튜토리얼 진행 단계 또는 완료 상태를 저장하고 Tutorial 전용 저장을 요청합니다.
        /// </summary>
        public void SetProgress(int tutorialUid, int stepIndex, bool isCompleted)
        {
            if (tutorialUid <= 0)
            {
                return;
            }

            if (!Progress.TryGetValue(tutorialUid, out TutorialProgressData data) || data == null)
            {
                data = new TutorialProgressData { TutorialUid = tutorialUid };
                Progress[tutorialUid] = data;
            }

            data.StepIndex = stepIndex >= 0 ? stepIndex : 0;
            data.IsCompleted = isCompleted;
            TutorialPackageManager.Instance?.SaveDataManagerTutorial?.StartSaveData();
        }

        /// <summary>
        /// 저장 봉투에 기록되는 Tutorial 진행 데이터 DTO입니다.
        /// </summary>
        public sealed class TutorialProgressSnapshot
        {
            /// <summary>
            /// 튜토리얼 UID별 진행 상태입니다.
            /// </summary>
            public Dictionary<int, TutorialProgressData> Progress;
        }

        /// <summary>
        /// 외부 저장 데이터와 런타임 진행 데이터가 같은 참조를 공유하지 않도록 깊은 복사합니다.
        /// </summary>
        /// <param name="source">복사할 Tutorial 진행 상태 사전입니다.</param>
        /// <returns>개별 진행 데이터까지 복제된 새 사전입니다.</returns>
        private static Dictionary<int, TutorialProgressData> CloneProgress(
            IReadOnlyDictionary<int, TutorialProgressData> source)
        {
            var result = new Dictionary<int, TutorialProgressData>();
            if (source == null)
            {
                return result;
            }

            foreach (KeyValuePair<int, TutorialProgressData> pair in source)
            {
                TutorialProgressData data = pair.Value;
                if (data == null)
                {
                    continue;
                }

                result[pair.Key] = new TutorialProgressData
                {
                    TutorialUid = data.TutorialUid,
                    StepIndex = data.StepIndex,
                    IsCompleted = data.IsCompleted,
                };
            }

            return result;
        }
    }
}
