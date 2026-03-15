using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class RepairEnvironmentMeshes
{
	private const string PrefabsFolder = "Assets/Prefabs/Enviroment/Hill_Zone";
	private const string MeshesFolder = "Assets/Meshes/Enviroment/Hill_Zone";

	[MenuItem("Tools/Environment/Repair Missing Mesh Filters")] 
	public static void Repair()
	{
		int repaired = 0;
		string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { PrefabsFolder });
		foreach (string guid in prefabGuids)
		{
			string prefabPath = AssetDatabase.GUIDToAssetPath(guid);
			GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
			if (prefab == null) continue;

			bool changed = false;
			foreach (MeshFilter mf in prefab.GetComponentsInChildren<MeshFilter>(true))
			{
				if (mf.sharedMesh != null) continue;

				// Try to find a mesh with the same name under MeshesFolder
				string meshName = mf.gameObject.name;
				string[] meshGuids = AssetDatabase.FindAssets($"{meshName} t:Mesh", new[] { MeshesFolder });
				Mesh found = null;
				foreach (string mg in meshGuids)
				{
					string path = AssetDatabase.GUIDToAssetPath(mg);
					found = AssetDatabase.LoadAssetAtPath<Mesh>(path);
					if (found != null) break;
				}

				if (found == null)
				{
					// Fallback: search any mesh in MeshesFolder
					meshGuids = AssetDatabase.FindAssets("t:Mesh", new[] { MeshesFolder });
					foreach (string mg in meshGuids)
					{
						string path = AssetDatabase.GUIDToAssetPath(mg);
						found = AssetDatabase.LoadAssetAtPath<Mesh>(path);
						if (found != null) break;
					}
				}

				if (found != null)
				{
					mf.sharedMesh = found;
					changed = true;
				}
			}

			if (changed)
			{
				EditorUtility.SetDirty(prefab);
				repaired++;
			}
		}

		if (repaired > 0)
		{
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
		}

		Debug.Log($"[Repair] Mesh Filters repaired in {repaired} prefab(s) under '{PrefabsFolder}'.");
	}

	[MenuItem("Tools/Environment/Assign Meshes By Name")] 
	public static void AssignByName()
	{
		int changedCount = 0;
		string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { PrefabsFolder });
		foreach (string guid in prefabGuids)
		{
			string prefabPath = AssetDatabase.GUIDToAssetPath(guid);
			GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
			if (prefab == null) continue;

			bool changed = false;

			// MeshFilter
			foreach (MeshFilter mf in prefab.GetComponentsInChildren<MeshFilter>(true))
			{
				string targetName = mf.gameObject.name;
				if (mf.sharedMesh != null && mf.sharedMesh.name == targetName) continue; // already correct

				Mesh found = FindMeshByName(targetName);
				if (found != null)
				{
					mf.sharedMesh = found;
					changed = true;
				}
			}

			// SkinnedMeshRenderer (just in case)
			foreach (SkinnedMeshRenderer smr in prefab.GetComponentsInChildren<SkinnedMeshRenderer>(true))
			{
				string targetName = smr.gameObject.name;
				if (smr.sharedMesh != null && smr.sharedMesh.name == targetName) continue;
				Mesh found = FindMeshByName(targetName);
				if (found != null)
				{
					smr.sharedMesh = found;
					changed = true;
				}
			}

			if (changed)
			{
				EditorUtility.SetDirty(prefab);
				PrefabUtility.SavePrefabAsset(prefab);
				changedCount++;
			}
		}

		if (changedCount > 0)
		{
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
		}

		Debug.Log($"[Repair] Assigned meshes by name in {changedCount} prefab(s) under '{PrefabsFolder}'.");
	}

	private static Mesh FindMeshByName(string meshName)
	{
		// Try exact name under meshes folder
		string[] meshGuids = AssetDatabase.FindAssets($"t:Mesh {meshName}", new[] { MeshesFolder });
		foreach (string mg in meshGuids)
		{
			string path = AssetDatabase.GUIDToAssetPath(mg);
			Mesh found = AssetDatabase.LoadAssetAtPath<Mesh>(path);
			if (found != null && found.name == meshName) return found;
		}

		// Try anywhere in Assets
		meshGuids = AssetDatabase.FindAssets($"t:Mesh {meshName}");
		foreach (string mg in meshGuids)
		{
			string path = AssetDatabase.GUIDToAssetPath(mg);
			Mesh found = AssetDatabase.LoadAssetAtPath<Mesh>(path);
			if (found != null && found.name == meshName) return found;
		}

		return null;
	}
}


