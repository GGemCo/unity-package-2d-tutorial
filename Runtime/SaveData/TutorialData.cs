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
        public int TutorialUid;
        public int StepIndex;
        public bool IsCompleted;
    }

    /// <summary>
    /// Tutorial 진행 상태를 Core 저장 확장 섹션에 기록하고 복원합니다.
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
                Progress = new Dictionary<int, TutorialProgressData>(Progress),
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

            Progress = new Dictionary<int, TutorialProgressData>(snapshot.Progress);
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
        /// 튜토리얼 진행 단계 또는 완료 상태를 저장하고 Core 저장을 요청합니다.
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
            SceneGame.Instance?.saveDataManager?.StartSaveData();
        }

        /// <summary>
        /// 저장 봉투에 기록되는 Tutorial 진행 데이터 DTO입니다.
        /// </summary>
        public sealed class TutorialProgressSnapshot
        {
            public Dictionary<int, TutorialProgressData> Progress;
        }
    }
}
