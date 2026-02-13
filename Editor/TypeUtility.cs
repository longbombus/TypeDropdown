using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Unity.CodeEditor;
using UnityEditor;
using UnityEngine;

namespace TypeDropdown.Editor
{
	public static class TypeUtility
	{
		/// <summary> Tries to get a <see cref="Type"/> by its assembly qualified name. </summary>
		/// <returns> True if type was found </returns>
		public static bool TryGetType(string typeName, out Type type)
		{
			try
			{
				type = Type.GetType(typeName);
				return type != null;
			}
			catch (ArgumentNullException) { /* ok */ }
			catch (Exception e)
			{
				Debug.LogException(e);
			}

			type = null;
			return false;
		}

		/// <summary>
		/// Tries to get a <see cref="Type"/> from a Unity style type name.
		/// <code>
		/// Src: https://docs.unity3d.com/ScriptReference/SerializedProperty-managedReferenceFieldTypename.html
		/// Unity style type name format: "[assembly-name] [namespace.][parent-class-names][classname]" where:
		/// - [assembly-name] is the name of the assembly that contains the target type
		/// - [namespace.] is an optional (if empty) namespace followed by a '.'
		/// - [parent-class-names] is a '/' separated list of optional parent class names (in the case of nested class definitions)
		/// - [classname] is the managed reference field class name.
		/// </code>
		/// </summary>
		/// <returns> True if type was found </returns>
		public static bool TryGetTypeUnityStyle(string unityStyleTypeName, out Type type)
		{
			do
			{
				if (string.IsNullOrEmpty(unityStyleTypeName))
					break;

				// Unity format: "[assembly] [namespace.Type/Inner]"
				int spaceIndex = unityStyleTypeName.IndexOf(' ');
				if (spaceIndex < 0)
					break;

				var dotNetTypeName = new StringBuilder(unityStyleTypeName.Length + 2);

				var typeNameLength = unityStyleTypeName.Length - spaceIndex - 1;
				dotNetTypeName.Append(unityStyleTypeName, spaceIndex + 1, typeNameLength);
				dotNetTypeName.Replace('/', '+');
				dotNetTypeName.Append(", ");
				dotNetTypeName.Append(unityStyleTypeName, 0, spaceIndex);

				return TryGetType(dotNetTypeName.ToString(), out type);
			} while (false);

			type = null;
			return false;
		}

		/// <returns> Assembly qualified name </returns>
		public static string GetTypeName(Type type)
			=> $"{type.FullName}, {type.Assembly.GetName().Name}";

		/// <summary>
		/// Opens the script file that defines the specified type in the code editor.
		/// </summary>
		/// <param name="type"> Type that should be defined in script </param>
		/// <returns> Returns true if a script was successfully opened; otherwise, false. </returns>
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