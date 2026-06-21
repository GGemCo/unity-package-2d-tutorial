using System.IO;
using GGemCo2DCore;
using GGemCo2DCoreEditor;
using GGemCo2DTutorial;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace GGemCo2DTutorialEditor
{
    /// <summary>
    /// Tutorial 런타임 설정 ScriptableObject를 Common Addressables 그룹에 등록합니다.
    /// </summary>
    public sealed class SettingScriptableObjectTutorial : DefaultAddressable
    {
        private const string Title = "설정 ScriptableObject 추가하기";
        private readonly AddressableEditorTutorial _addressableEditor;

        /// <summary>
        /// Tutorial 설정 ScriptableObject Addressables 모듈을 생성합니다.
        /// </summary>
        /// <param name="addressableEditorWindow">버튼 레이아웃을 제공하는 부모 창입니다.</param>
        public SettingScriptableObjectTutorial(AddressableEditorTutorial addressableEditorWindow)
        {
            _addressableEditor = addressableEditorWindow;
            targetGroupName = ConfigAddressableGroupName.Common;
        }

        /// <summary>
        /// Tutorial 설정 ScriptableObject 등록 버튼을 그립니다.
        /// </summary>
        public void OnGUI()
        {
            if (!GUILayout.Button(
                    Title,
                    GUILayout.Width(_addressableEditor.ButtonWidth),
                    GUILayout.Height(_addressableEditor.ButtonHeight)))
            {
                return;
            }

            try
            {
                Setup();
            }
            catch (System.Exception exception)
            {
                Debug.LogException(exception);
                EditorUtility.DisplayDialog(
                    Title,
                    "Tutorial 설정 ScriptableObject Addressables 설정 중 오류가 발생했습니다.\n자세한 내용은 콘솔 로그를 확인해주세요.",
                    "OK");
            }
        }

        /// <summary>
        /// Tutorial 설정 에셋을 준비하고 Common Addressables 그룹에 등록합니다.
        /// </summary>
        /// <param name="ctx">
        /// 자동 설정 실행 컨텍스트입니다. null이면 완료 후 에셋을 저장하고 다이얼로그를 표시합니다.
        /// </param>
        public void Setup(EditorSetupContext ctx = null)
        {
            if (!EnsureTutorialSettingsAsset(ctx))
            {
                return;
            }

            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            if (!settings)
            {
                HelperLog.Warn("Addressable 설정을 찾을 수 없습니다. 새로 생성합니다.", ctx);
                settings = CreateAddressableSettings();
            }

            AddressableAssetGroup group = GetOrCreateGroup(settings, targetGroupName);
            if (!group)
            {
                HelperLog.Error($"'{targetGroupName}' 그룹을 설정할 수 없습니다.", ctx);
                return;
            }

            foreach (AddressableAssetInfo addressableAssetInfo in
                     ConfigAddressableSettingTutorial.NeedLoadInLoadingScene)
            {
                Add(
                    settings,
                    group,
                    addressableAssetInfo.Key,
                    addressableAssetInfo.Path,
                    addressableAssetInfo.Label);
            }

            settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, null, true);

            if (ctx != null)
            {
                HelperLog.Info("Tutorial 설정 ScriptableObject Addressables 설정 완료", ctx);
                return;
            }

            AssetDatabase.SaveAssets();
            EditorUtility.DisplayDialog(Title, "Addressables 설정 완료", "OK");
        }

        /// <summary>
        /// 기본 경로에 Tutorial 설정 에셋이 없으면 기본값으로 생성합니다.
        /// </summary>
        /// <param name="ctx">자동 설정 실행 컨텍스트입니다.</param>
        /// <returns>설정 에셋을 사용할 수 있으면 true입니다.</returns>
        private static bool EnsureTutorialSettingsAsset(EditorSetupContext ctx)
        {
            string assetPath = ConfigAddressableSettingTutorial.TutorialSettings.Path;
            GGemCoTutorialSettings existing =
                AssetDatabase.LoadAssetAtPath<GGemCoTutorialSettings>(assetPath);
            if (existing)
            {
                return true;
            }

            GGemCoTutorialSettings created = null;
            try
            {
                // ConfigAddressableSetting 규칙의 Settings 폴더가 없는 새 프로젝트도 지원합니다.
                string directoryPath = Path.GetDirectoryName(assetPath)?.Replace('\\', '/');
                if (!string.IsNullOrWhiteSpace(directoryPath) && !Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                    AssetDatabase.Refresh();
                }

                created = ScriptableObject.CreateInstance<GGemCoTutorialSettings>();
                AssetDatabase.CreateAsset(created, assetPath);
                EditorUtility.SetDirty(created);
                AssetDatabase.SaveAssets();
                HelperLog.Info($"Tutorial 설정 에셋을 생성했습니다. path: {assetPath}", ctx);
                return true;
            }
            catch (System.Exception exception)
            {
                if (created)
                {
                    Object.DestroyImmediate(created);
                }

                HelperLog.Error(
                    $"Tutorial 설정 에셋 생성에 실패했습니다. path: {assetPath}, error: {exception.Message}",
                    ctx);
                return false;
            }
        }
    }
}
