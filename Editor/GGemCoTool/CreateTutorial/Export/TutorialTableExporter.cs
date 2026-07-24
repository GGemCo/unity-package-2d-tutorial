using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using GGemCo2DTutorial;
using UnityEditor;

namespace GGemCo2DTutorialEditor
{
    /// <summary>
    /// Authoring Asset의 카탈로그 설정을 tutorial TSV 테이블에 동기화합니다.
    /// </summary>
    internal static class TutorialTableExporter
    {
        private static readonly UTF8Encoding Utf8NoBom =
            new UTF8Encoding(false);

        private static readonly string[] Headers =
        {
            "Uid", "Name", "Enabled", "Repeatable", "Priority",
            "StartEventType", "StartTargetUid", "StartInputAction",
            "StartIntValue", "StartFloatValue", "StartRequiredCount",
            "PreloadPolicy", "Memo",
        };

        /// <summary>
        /// 같은 UID의 테이블 행을 교체하고 내용이 달라진 경우에만 UTF-8 BOM 없는 TSV로 저장합니다.
        /// </summary>
        /// <param name="asset">테이블에 동기화할 Tutorial 제작 에셋입니다.</param>
        /// <returns>동기화 성공 여부와 실제 파일 변경 여부입니다.</returns>
        public static TutorialTableExportResult Export(
            TutorialAuthoringAsset asset)
        {
            if (asset == null || asset.Uid <= 0)
            {
                return TutorialTableExportResult.Failure(
                    "테이블에 동기화할 Tutorial UID가 없습니다.");
            }

            try
            {
                string assetPath = ConfigAddressableTableTutorial.TableTutorial.Path;
                string absolutePath = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    assetPath);
                List<Dictionary<string, string>> rows = ReadRows(absolutePath);
                Dictionary<string, string> target = FindOrCreate(rows, asset.Uid);
                WriteAuthoringValues(target, asset);

                string content = BuildContent(rows);
                byte[] generatedBytes = Utf8NoBom.GetBytes(content);
                byte[] existingBytes = File.Exists(absolutePath)
                    ? File.ReadAllBytes(absolutePath)
                    : null;
                bool changed = !AreBytesEqual(existingBytes, generatedBytes);
                if (!changed)
                {
                    return TutorialTableExportResult.Success(
                        false,
                        $"Tutorial 테이블 변경사항이 없습니다. uid: {asset.Uid}",
                        assetPath,
                        AssetDatabase.LoadAssetAtPath<UnityEngine.TextAsset>(
                            assetPath));
                }

                string directoryPath = Path.GetDirectoryName(absolutePath);
                if (!string.IsNullOrWhiteSpace(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                File.WriteAllBytes(absolutePath, generatedBytes);
                AssetDatabase.ImportAsset(
                    assetPath,
                    ImportAssetOptions.ForceUpdate);
                return TutorialTableExportResult.Success(
                    true,
                    $"Tutorial 테이블 행을 변경했습니다. uid: {asset.Uid}",
                    assetPath,
                    AssetDatabase.LoadAssetAtPath<UnityEngine.TextAsset>(
                        assetPath));
            }
            catch (Exception exception)
            {
                return TutorialTableExportResult.Failure(
                    $"Tutorial 테이블 동기화에 실패했습니다. {exception.Message}");
            }
        }

        /// <summary>
        /// 정규화된 Header 순서로 전체 Tutorial TSV 문자열을 생성합니다.
        /// </summary>
        /// <param name="rows">출력할 테이블 행 목록입니다.</param>
        /// <returns>운영체제 기본 줄바꿈이 적용된 TSV 문자열입니다.</returns>
        private static string BuildContent(
            IReadOnlyList<Dictionary<string, string>> rows)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine(string.Join("\t", Headers));
            for (int i = 0; i < rows.Count; i++)
            {
                Dictionary<string, string> row = rows[i];
                for (int column = 0; column < Headers.Length; column++)
                {
                    if (column > 0)
                    {
                        builder.Append('\t');
                    }

                    row.TryGetValue(Headers[column], out string value);
                    builder.Append(Sanitize(value));
                }

                builder.AppendLine();
            }

            return builder.ToString();
        }

        /// <summary>
        /// 기존 Tutorial TSV에서 Header와 행 값을 읽습니다.
        /// </summary>
        /// <param name="absolutePath">읽을 테이블의 절대 경로입니다.</param>
        /// <returns>Header 이름을 키로 사용하는 행 목록입니다.</returns>
        private static List<Dictionary<string, string>> ReadRows(string absolutePath)
        {
            List<Dictionary<string, string>> rows =
                new List<Dictionary<string, string>>();
            if (!File.Exists(absolutePath))
            {
                return rows;
            }

            string[] lines = File.ReadAllLines(absolutePath, Encoding.UTF8);
            if (lines.Length == 0)
            {
                return rows;
            }

            string[] sourceHeaders = lines[0].TrimStart('\uFEFF').Split('\t');
            for (int lineIndex = 1; lineIndex < lines.Length; lineIndex++)
            {
                if (string.IsNullOrWhiteSpace(lines[lineIndex]))
                {
                    continue;
                }

                string[] values = lines[lineIndex].Split('\t');
                Dictionary<string, string> row =
                    new Dictionary<string, string>(StringComparer.Ordinal);
                for (int i = 0; i < sourceHeaders.Length && i < values.Length; i++)
                {
                    row[sourceHeaders[i]] = values[i];
                }

                rows.Add(row);
            }

            return rows;
        }

        /// <summary>
        /// 지정 UID의 기존 행을 찾고 없으면 새 행을 목록 끝에 추가합니다.
        /// </summary>
        /// <param name="rows">검색할 테이블 행 목록입니다.</param>
        /// <param name="uid">찾거나 생성할 Tutorial UID입니다.</param>
        /// <returns>동기화 대상 행입니다.</returns>
        private static Dictionary<string, string> FindOrCreate(
            List<Dictionary<string, string>> rows,
            int uid)
        {
            string uidText = uid.ToString(CultureInfo.InvariantCulture);
            for (int i = 0; i < rows.Count; i++)
            {
                if (rows[i].TryGetValue("Uid", out string value) && value == uidText)
                {
                    return rows[i];
                }
            }

            Dictionary<string, string> created =
                new Dictionary<string, string>(StringComparer.Ordinal);
            rows.Add(created);
            return created;
        }

        /// <summary>
        /// TutorialAuthoringAsset의 카탈로그 값을 대상 테이블 행에 기록합니다.
        /// </summary>
        /// <param name="row">갱신할 테이블 행입니다.</param>
        /// <param name="asset">값을 제공하는 제작 에셋입니다.</param>
        private static void WriteAuthoringValues(
            Dictionary<string, string> row,
            TutorialAuthoringAsset asset)
        {
            TutorialAuthoringCondition condition = asset.StartCondition;
            row["Uid"] = asset.Uid.ToString(CultureInfo.InvariantCulture);
            row["Name"] = asset.Title;
            row["Enabled"] = asset.Enabled ? "Y" : "N";
            row["Repeatable"] = asset.Repeatable ? "Y" : "N";
            row["Priority"] = asset.Priority.ToString(CultureInfo.InvariantCulture);
            row["StartEventType"] = condition.Type.ToString();
            row["StartTargetUid"] = condition.TargetUid.ToString(CultureInfo.InvariantCulture);
            row["StartInputAction"] = condition.InputAction.ToString();
            row["StartIntValue"] = condition.IntValue.ToString(CultureInfo.InvariantCulture);
            row["StartFloatValue"] =
                condition.FloatValue.ToString(CultureInfo.InvariantCulture);
            row["StartRequiredCount"] =
                condition.RequiredCount.ToString(CultureInfo.InvariantCulture);
            row["PreloadPolicy"] = asset.PreloadPolicy.ToString();
            row["Memo"] = asset.Memo;
            row.Remove("StartKey");
        }

        /// <summary>
        /// TSV 셀을 깨뜨릴 수 있는 탭과 줄바꿈을 공백으로 치환합니다.
        /// </summary>
        /// <param name="value">정리할 원본 셀 값입니다.</param>
        /// <returns>한 줄 TSV 셀에 안전한 문자열입니다.</returns>
        private static string Sanitize(string value)
        {
            return string.IsNullOrEmpty(value)
                ? string.Empty
                : value.Replace('\t', ' ').Replace('\r', ' ').Replace('\n', ' ');
        }

        /// <summary>
        /// 두 바이트 배열의 길이와 내용을 순서대로 비교합니다.
        /// BOM 또는 줄바꿈 차이도 실제 파일 변경으로 처리합니다.
        /// </summary>
        /// <param name="left">기존 파일 바이트이며 파일이 없으면 null입니다.</param>
        /// <param name="right">새로 생성한 UTF-8 BOM 없는 바이트입니다.</param>
        /// <returns>두 배열의 전체 내용이 같으면 true입니다.</returns>
        private static bool AreBytesEqual(byte[] left, byte[] right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if (left == null || right == null || left.Length != right.Length)
            {
                return false;
            }

            for (int i = 0; i < left.Length; i++)
            {
                if (left[i] != right[i])
                {
                    return false;
                }
            }

            return true;
        }
    }
}
