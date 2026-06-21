using GGemCo2DTutorial;
using UnityEditor;
using UnityEngine;

namespace GGemCo2DTutorialEditor
{
    /// <summary>
    /// CreateTutorialWindow의 Addressables 등록 보조 UI를 담당합니다.
    /// </summary>
    public sealed partial class CreateTutorialWindow
    {
        /// <summary>
        /// Export된 Tutorial JSON, Catalog JSON, TableTutorial txt를 Addressables에 등록하는 보조 UI를 그립니다.
        /// </summary>
        private void DrawAddressablesPanel()
        {
            EditorGUILayout.LabelField("Addressables 등록 보조", EditorStyles.boldLabel);
            _addressablesGroupName = EditorGUILayout.TextField("Group", _addressablesGroupName);
            _definitionLabel = EditorGUILayout.TextField("Tutorial Label", _definitionLabel);
            _catalogLabel = EditorGUILayout.TextField("Catalog Label", _catalogLabel);
            _tableLabel = EditorGUILayout.TextField("Table Label", _tableLabel);

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Tutorial JSON", EditorStyles.boldLabel);
                EditorGUILayout.LabelField("Address", GetCurrentTutorialAddress(), EditorStyles.wordWrappedMiniLabel);
                EditorGUILayout.LabelField("Last Export", string.IsNullOrWhiteSpace(_lastDefinitionJsonPath) ? "없음" : _lastDefinitionJsonPath, EditorStyles.wordWrappedMiniLabel);

                using (new EditorGUI.DisabledScope(_asset == null || string.IsNullOrWhiteSpace(_lastDefinitionJsonPath)))
                {
                    if (GUILayout.Button("마지막 Export JSON 등록"))
                    {
                        RegisterLastDefinitionJson();
                    }
                }

                using (new EditorGUI.DisabledScope(_asset == null))
                {
                    if (GUILayout.Button("선택 TextAsset을 Tutorial JSON으로 등록"))
                    {
                        RegisterSelectedDefinitionJson();
                    }
                }
            }

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Catalog JSON", EditorStyles.boldLabel);
                EditorGUILayout.LabelField("Address", TutorialConstants.DefaultCatalogKey, EditorStyles.wordWrappedMiniLabel);
                EditorGUILayout.LabelField("Last Export", string.IsNullOrWhiteSpace(_lastCatalogJsonPath) ? "없음" : _lastCatalogJsonPath, EditorStyles.wordWrappedMiniLabel);

                using (new EditorGUI.DisabledScope(string.IsNullOrWhiteSpace(_lastCatalogJsonPath)))
                {
                    if (GUILayout.Button("마지막 Catalog JSON 등록"))
                    {
                        RegisterLastCatalogJson();
                    }
                }

                if (GUILayout.Button("선택 TextAsset을 Catalog JSON으로 등록"))
                {
                    RegisterSelectedCatalogJson();
                }
            }

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("TableTutorial TXT", EditorStyles.boldLabel);
                EditorGUILayout.LabelField("Address", ConfigAddressableTableTutorial.TableTutorial.Key, EditorStyles.wordWrappedMiniLabel);
                EditorGUILayout.LabelField("Last Export", string.IsNullOrWhiteSpace(_lastTableTutorialPath) ? "없음" : _lastTableTutorialPath, EditorStyles.wordWrappedMiniLabel);

                using (new EditorGUI.DisabledScope(string.IsNullOrWhiteSpace(_lastTableTutorialPath)))
                {
                    if (GUILayout.Button("마지막 TableTutorial TXT 등록"))
                    {
                        RegisterLastTableTutorial();
                    }
                }

                if (GUILayout.Button("선택 TextAsset을 TableTutorial TXT로 등록"))
                {
                    RegisterSelectedTableTutorial();
                }
            }
        }

        /// <summary>
        /// 현재 제작 데이터에 설정된 Tutorial JSON Addressables 주소를 반환합니다.
        /// </summary>
        /// <returns>현재 튜토리얼 Addressables 주소입니다.</returns>
        private string GetCurrentTutorialAddress()
        {
            if (_asset == null || string.IsNullOrWhiteSpace(_asset.AddressableKey))
            {
                return "Addressables Key가 비어 있습니다.";
            }

            return _asset.AddressableKey.Trim();
        }

        /// <summary>
        /// 마지막으로 Export한 Tutorial JSON을 Addressables에 등록합니다.
        /// </summary>
        private void RegisterLastDefinitionJson()
        {
            RegisterDefinitionJson(_lastDefinitionJsonPath);
        }

        /// <summary>
        /// 프로젝트 창에서 선택한 TextAsset을 현재 Tutorial JSON으로 Addressables에 등록합니다.
        /// </summary>
        private void RegisterSelectedDefinitionJson()
        {
            if (!TutorialAddressableRegisterUtility.TryGetSelectedTextAssetPath(out string assetPath, out string error))
            {
                ApplyAddressableRegistrationResult(TutorialAddressableRegistrationResult.Failure(error));
                return;
            }

            RegisterDefinitionJson(assetPath);
        }

        /// <summary>
        /// 마지막으로 Export한 Catalog JSON을 Addressables에 등록합니다.
        /// </summary>
        private void RegisterLastCatalogJson()
        {
            RegisterCatalogJson(_lastCatalogJsonPath);
        }

        /// <summary>
        /// 프로젝트 창에서 선택한 TextAsset을 Catalog JSON으로 Addressables에 등록합니다.
        /// </summary>
        private void RegisterSelectedCatalogJson()
        {
            if (!TutorialAddressableRegisterUtility.TryGetSelectedTextAssetPath(out string assetPath, out string error))
            {
                ApplyAddressableRegistrationResult(TutorialAddressableRegistrationResult.Failure(error));
                return;
            }

            RegisterCatalogJson(assetPath);
        }

        /// <summary>
        /// 마지막으로 Export한 TableTutorial txt를 Addressables 테이블로 등록합니다.
        /// </summary>
        private void RegisterLastTableTutorial()
        {
            RegisterTableTutorial(_lastTableTutorialPath);
        }

        /// <summary>
        /// 프로젝트 창에서 선택한 TextAsset을 TableTutorial txt로 Addressables에 등록합니다.
        /// </summary>
        private void RegisterSelectedTableTutorial()
        {
            if (!TutorialAddressableRegisterUtility.TryGetSelectedTextAssetPath(out string assetPath, out string error))
            {
                ApplyAddressableRegistrationResult(TutorialAddressableRegistrationResult.Failure(error));
                return;
            }

            RegisterTableTutorial(assetPath);
        }

        /// <summary>
        /// 지정한 Tutorial JSON 에셋 경로를 현재 제작 데이터의 Addressables Key로 등록합니다.
        /// </summary>
        /// <param name="assetPath">등록할 Tutorial JSON 에셋 경로입니다.</param>
        private void RegisterDefinitionJson(string assetPath)
        {
            if (_asset == null)
            {
                ApplyAddressableRegistrationResult(TutorialAddressableRegistrationResult.Failure("Tutorial JSON 주소를 가져올 제작 데이터를 먼저 선택하십시오."));
                return;
            }

            if (string.IsNullOrWhiteSpace(_asset.AddressableKey))
            {
                ApplyAddressableRegistrationResult(TutorialAddressableRegistrationResult.Failure("현재 제작 데이터의 Addressables Key가 비어 있습니다."));
                return;
            }

            TutorialAddressableRegistrationResult result = TutorialAddressableRegisterUtility.RegisterAsset(
                assetPath,
                _asset.AddressableKey,
                _addressablesGroupName,
                _definitionLabel);
            ApplyAddressableRegistrationResult(result);
        }

        /// <summary>
        /// 지정한 Catalog JSON 에셋 경로를 기본 Catalog Addressables Key로 등록합니다.
        /// </summary>
        /// <param name="assetPath">등록할 Catalog JSON 에셋 경로입니다.</param>
        private void RegisterCatalogJson(string assetPath)
        {
            TutorialAddressableRegistrationResult result = TutorialAddressableRegisterUtility.RegisterAsset(
                assetPath,
                TutorialConstants.DefaultCatalogKey,
                _addressablesGroupName,
                _catalogLabel);
            ApplyAddressableRegistrationResult(result);
        }

        /// <summary>
        /// 지정한 TableTutorial txt 에셋 경로를 Tutorial 테이블 Addressables Key로 등록합니다.
        /// </summary>
        /// <param name="assetPath">등록할 TableTutorial txt 에셋 경로입니다.</param>
        private void RegisterTableTutorial(string assetPath)
        {
            TutorialAddressableRegistrationResult result = TutorialAddressableRegisterUtility.RegisterAsset(
                assetPath,
                ConfigAddressableTableTutorial.TableTutorial.Key,
                _addressablesGroupName,
                _tableLabel);
            ApplyAddressableRegistrationResult(result);
        }

        /// <summary>
        /// Addressables 등록 결과를 상태 메시지와 프로젝트 창 선택 상태에 반영합니다.
        /// </summary>
        /// <param name="result">반영할 등록 결과입니다.</param>
        private void ApplyAddressableRegistrationResult(TutorialAddressableRegistrationResult result)
        {
            _statusMessage = result.Message;
            _statusType = result.Succeeded ? MessageType.Info : MessageType.Error;

            if (!result.Succeeded || result.Asset == null)
            {
                return;
            }

            EditorGUIUtility.PingObject(result.Asset);
            Selection.activeObject = result.Asset;
        }
    }
}
