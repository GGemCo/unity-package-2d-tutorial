using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GGemCo2DCore;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace GGemCo2DTutorial
{
    /// <summary>
    /// 개별 Tutorial JSON을 Addressables에서 로드하고 정의를 캐시합니다.
    /// </summary>
    public sealed class TutorialAddressableRepository : IDisposable
    {
        private readonly Dictionary<int, TutorialDefinition> _cache =
            new Dictionary<int, TutorialDefinition>();
        private readonly Dictionary<int, Task<TutorialDefinition>> _loadingTasks =
            new Dictionary<int, Task<TutorialDefinition>>();
        private bool _isDisposed;

        /// <summary>
        /// 개별 Tutorial JSON을 중복 요청 없이 로드하고 캐시합니다.
        /// </summary>
        public Task<TutorialDefinition> GetAsync(TutorialCatalogEntry entry)
        {
            if (_isDisposed || entry == null || entry.uid <= 0 ||
                string.IsNullOrWhiteSpace(entry.addressableKey))
            {
                return Task.FromResult<TutorialDefinition>(null);
            }

            if (_cache.TryGetValue(entry.uid, out TutorialDefinition cached))
            {
                return Task.FromResult(cached);
            }

            if (_loadingTasks.TryGetValue(entry.uid, out Task<TutorialDefinition> loading))
            {
                return loading;
            }

            Task<TutorialDefinition> task = LoadDefinitionAsync(entry);
            _loadingTasks.Add(entry.uid, task);
            return task;
        }

        /// <summary>
        /// 캐시와 진행 중인 요청 추적을 정리합니다.
        /// TextAsset 핸들은 각 로드 메서드에서 즉시 해제됩니다.
        /// </summary>
        public void Dispose()
        {
            _isDisposed = true;
            _cache.Clear();
            _loadingTasks.Clear();
        }

        private async Task<TutorialDefinition> LoadDefinitionAsync(TutorialCatalogEntry entry)
        {
            try
            {
                TutorialDefinition definition =
                    await LoadTextAssetAsync<TutorialDefinition>(
                        entry.addressableKey,
                        $"Tutorial uid={entry.uid}");

                if (_isDisposed)
                {
                    return null;
                }

                if (!TutorialDefinitionValidator.ValidateRuntime(
                        entry.uid,
                        definition,
                        out string error))
                {
                    if (!string.IsNullOrWhiteSpace(error))
                    {
                        GcLogger.LogError(error);
                    }

                    return null;
                }

                _cache[entry.uid] = definition;
                return definition;
            }
            finally
            {
                _loadingTasks.Remove(entry.uid);
            }
        }

        /// <summary>
        /// Addressables TextAsset을 로드하여 지정한 DTO로 역직렬화합니다.
        /// 파싱이 끝나면 성공 여부와 관계없이 핸들을 해제합니다.
        /// </summary>
        /// <typeparam name="T">역직렬화할 참조 타입입니다.</typeparam>
        /// <param name="key">TextAsset Addressables 키입니다.</param>
        /// <param name="displayName">오류 로그에 표시할 데이터 이름입니다.</param>
        /// <returns>역직렬화된 객체이며 실패 시 null입니다.</returns>
        private static async Task<T> LoadTextAssetAsync<T>(string key, string displayName)
            where T : class
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return null;
            }

            AsyncOperationHandle<TextAsset> handle = default;
            try
            {
                handle = Addressables.LoadAssetAsync<TextAsset>(key);
                await handle.Task;
                if (handle.Status != AsyncOperationStatus.Succeeded || handle.Result == null)
                {
                    GcLogger.LogError(
                        $"{displayName} Addressables 로드에 실패했습니다. key: {key}");
                    return null;
                }

                return JsonConvert.DeserializeObject<T>(handle.Result.text);
            }
            catch (Exception exception)
            {
                GcLogger.LogError(
                    $"{displayName} JSON 파싱 중 오류가 발생했습니다. key: {key}, error: {exception.Message}");
                return null;
            }
            finally
            {
                // 파싱 이후에는 순수 C# 정의만 유지하므로 TextAsset 핸들을 즉시 반환합니다.
                if (handle.IsValid())
                {
                    Addressables.Release(handle);
                }
            }
        }
    }
}
