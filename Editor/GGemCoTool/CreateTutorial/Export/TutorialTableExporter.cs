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
        private static readonly string[] Headers =
        {
            "Uid", "Name", "Enabled", "Repeatable", "Priority",
            "StartEventType", "StartTargetUid", "StartInputAction",
            "StartIntValue", "StartFloatValue", "StartRequiredCount",
            "PreloadPolicy", "Memo",
        };

        /// <summary>
        /// 같은 UID의 테이블 행을 교체하고 UTF-8 BOM 없는 TSV로 저장합니다.
        /// </summary>
        public static TutorialExportResult Export(TutorialAuthoringAsset asset)
        {
            if (asset == null || asset.Uid <= 0)
            {
                return TutorialExportResult.Failure("테이블에 동기화할 Tutorial UID가 없습니다.");
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

                Directory.CreateDirectory(Path.GetDirectoryName(absolutePath));
                File.WriteAllText(
                    absolutePath,
                    builder.ToString(),
                    new UTF8Encoding(false));
                AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
                return TutorialExportResult.Success(
                    $"Tutorial 테이블 행을 동기화했습니다. uid: {asset.Uid}",
                    assetPath,
                    AssetDatabase.LoadAssetAtPath<UnityEngine.TextAsset>(assetPath));
            }
            catch (Exception exception)
            {
                return TutorialExportResult.Failure(
                    $"Tutorial 테이블 동기화에 실패했습니다. {exception.Message}");
            }
        }

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

        private static string Sanitize(string value)
        {
            return string.IsNullOrEmpty(value)
                ? string.Empty
                : value.Replace('\t', ' ').Replace('\r', ' ').Replace('\n', ' ');
        }
    }
}
