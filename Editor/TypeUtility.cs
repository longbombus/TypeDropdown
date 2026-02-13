using System;
using System.IO;
using System.Text.RegularExpressions;
using Unity.CodeEditor;
using UnityEditor;
using UnityEngine;

namespace TypeDropdown.Editor
{
	public static class TypeUtility
	{
		public static bool TryPickTypeScript(Type type)
		{
			if (type == null)
				return false;

			var scriptGUIDs = AssetDatabase.FindAssetGUIDs($"t:script {type.Name}");
			if (scriptGUIDs.Length > 0)
			{
				CodeEditor.CurrentEditor.OpenProject(AssetDatabase.GUIDToAssetPath(scriptGUIDs[0]));
				return true;
			}

			if (TryFindScriptByContent(type, out var scriptPath))
			{
				CodeEditor.CurrentEditor.OpenProject(scriptPath);
				return true;
			}

			return false;
		}

		private static bool TryFindScriptByContent(Type type, out string scriptPath)
		{
			var typeName = type.Name;

			var pattern = new Regex(
				$@"(class|struct|interface|record)\s+{Regex.Escape(typeName)}\b",
				RegexOptions.Compiled | RegexOptions.CultureInvariant
			);

			var guids = AssetDatabase.FindAssetGUIDs("t:script");
			foreach (var guid in guids)
			{
				var path = AssetDatabase.GUIDToAssetPath(guid);
				if (string.IsNullOrEmpty(path))
					continue;

				if (!path.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
					continue;
				if (path.Contains("/Library/", StringComparison.OrdinalIgnoreCase))
					continue;

				try
				{
					var fullPath = Path.GetFullPath(path);
					if (!File.Exists(fullPath))
						continue;

					foreach (var line in File.ReadLines(fullPath))
					{
						if (string.IsNullOrWhiteSpace(line))
							continue;

						if (line.Length > 255)
							continue;

						if (!pattern.IsMatch(line))
							continue;

						scriptPath = path;
						return true;
					}
				}
				catch (Exception e)
				{
					Debug.LogException(e);
				}
			}

			scriptPath = null;
			return false;
		}
	}
}