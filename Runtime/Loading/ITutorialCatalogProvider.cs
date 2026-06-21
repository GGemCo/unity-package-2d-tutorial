using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GGemCo2DCore;

namespace GGemCo2DTutorial
{
    /// <summary>
    /// TutorialManager가 사용할 Catalog 공급원을 추상화합니다.
    /// </summary>
    public interface ITutorialCatalogProvider
    {
        /// <summary>
        /// Tutorial Catalog를 로드합니다.
        /// </summary>
        /// <returns>로드된 Catalog입니다. 실패 시 null입니다.</returns>
        Task<TutorialCatalog> LoadCatalogAsync();
    }

    /// <summary>
    /// 기존 Catalog JSON Addressables 키를 통해 Catalog를 공급합니다.
    /// </summary>
    public sealed class AddressableTutorialCatalogProvider : ITutorialCatalogProvider
    {
        private readonly TutorialAddressableRepository _repository;
        private readonly string _catalogKey;

        /// <summary>
        /// Addressables 기반 Catalog 공급자를 생성합니다.
        /// </summary>
        /// <param name="repository">Catalog JSON 로드에 사용할 저장소입니다.</param>
        /// <param name="catalogKey">Catalog TextAsset Addressables 키입니다.</param>
        public AddressableTutorialCatalogProvider(
            TutorialAddressableRepository repository,
            string catalogKey)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _catalogKey = string.IsNullOrWhiteSpace(catalogKey)
                ? TutorialConstants.DefaultCatalogKey
                : catalogKey.Trim();
        }

        /// <summary>
        /// Addressables에서 Catalog JSON을 로드합니다.
        /// </summary>
        /// <returns>로드된 Catalog입니다.</returns>
        public Task<TutorialCatalog> LoadCatalogAsync()
        {
            return _repository.LoadCatalogAsync(_catalogKey);
        }
    }

    /// <summary>
    /// TableTutorial 행을 런타임 Catalog로 변환하여 공급합니다.
    /// </summary>
    public sealed class TableTutorialCatalogProvider : ITutorialCatalogProvider
    {
        private readonly TableTutorial _tableTutorial;

        /// <summary>
        /// TableTutorial 기반 Catalog 공급자를 생성합니다.
        /// </summary>
        /// <param name="tableTutorial">Catalog Entry로 변환할 Tutorial 테이블입니다.</param>
        public TableTutorialCatalogProvider(TableTutorial tableTutorial)
        {
            _tableTutorial = tableTutorial;
        }

        /// <summary>
        /// 로드된 TableTutorial 캐시를 Catalog DTO로 변환합니다.
        /// </summary>
        /// <returns>테이블 기반 Catalog입니다. 유효한 행이 없으면 null입니다.</returns>
        public Task<TutorialCatalog> LoadCatalogAsync()
        {
            TutorialCatalog catalog = BuildCatalog();
            return Task.FromResult(catalog);
        }

        /// <summary>
        /// TableTutorial 행 목록에서 활성화된 Tutorial Catalog를 생성합니다.
        /// </summary>
        /// <returns>생성된 Catalog입니다.</returns>
        private TutorialCatalog BuildCatalog()
        {
            if (_tableTutorial == null)
            {
                return null;
            }

            IReadOnlyList<StruckTableTutorial> rows = _tableTutorial.GetEnabledRows();
            if (rows == null || rows.Count <= 0)
            {
                return null;
            }

            TutorialCatalog catalog = new TutorialCatalog();
            HashSet<int> usedUids = new HashSet<int>();
            for (int i = 0; i < rows.Count; i++)
            {
                StruckTableTutorial row = rows[i];
                if (row == null || !row.Enabled || !usedUids.Add(row.Uid))
                {
                    continue;
                }

                TutorialCatalogEntry entry = TableTutorial.ToCatalogEntry(row);
                if (entry == null)
                {
                    GcLogger.LogWarning(
                        $"[Tutorial] TableTutorial 행을 Catalog Entry로 변환하지 못했습니다. uid={row.Uid}");
                    continue;
                }

                catalog.tutorials.Add(entry);
            }

            return catalog.tutorials.Count > 0 ? catalog : null;
        }
    }
}
