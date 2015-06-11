using UnityEditor;
using UnityEngine;
using System.IO;
using System.Text;
using System.Collections.Generic;

public class EditorTools 
{
	static string pathToShared = "Assets/Resources/Shared";
	static string pathToManifest = "Assets/Resources/Manifest.txt";
	static string resourcesFolderName = "Resources";

	/* Parses folders at pathToShared folder for available prefabs. */
	[MenuItem("Tools/Update Manifest File %m")]
	public static void UpdateManifest ()
	{
		if (pathToManifest == null)
		{
			UpdateManifestPath();
		}
		if (pathToShared == null)
		{
			ChangePathToResources();
		}
		Debug.Log ("Recreating the resource manifest file...");
		Debug.Log ("[Writing in " + pathToManifest + "]");

		List<DirectoryInfo> directoriesToCheck = new List<DirectoryInfo> ();
		List<FileInfo> prefabs = new List<FileInfo> ();

		directoriesToCheck.Add(new DirectoryInfo(pathToShared));

		while (directoriesToCheck.Count != 0)
		{
			DirectoryInfo dir = directoriesToCheck[0];
			directoriesToCheck.Remove(dir);

			prefabs.AddRange(dir.GetFiles("*.prefab"));
			directoriesToCheck.AddRange(dir.GetDirectories());
		}

		string[] prafabPaths = new string[prefabs.Count];

		for (int i = 0; i < prefabs.Count; i++)
		{
			prafabPaths[i] = ClipDirectoryPath(prefabs[i].DirectoryName) + Path.GetFileNameWithoutExtension(prefabs[i].Name);
		}

		// WriteAllLines
		System.IO.File.WriteAllLines(@pathToManifest, prafabPaths);
		Debug.Log ("...Finished creating resources manifest.");
		AssetDatabase.Refresh();
	}

	/* Clips directory paths in order to only keep path relative to the Resources folder. */
	static string ClipDirectoryPath (string path)
	{
		string[] splitPath = path.Split('\\');
		int index;
		for (index = 0; index < splitPath.Length; index++)
		{
			if (splitPath[index] == resourcesFolderName)
			{
				index++;
				break;
			}
		}

		if (index >= splitPath.Length)
		{
			return "";
		}

		StringBuilder sb = new StringBuilder ();
		for (;index < splitPath.Length; index++)
		{
			sb.Append(splitPath[index]);
			sb.Append('/');
		}

		return sb.ToString();
	}

	[MenuItem("Tools/Change Location of Manifest File %#m")]
	public static void UpdateManifestPath ()
	{
		pathToManifest = EditorUtility.SaveFilePanel("Select Manifest file location", "Assets/Resources", "Manifest", "txt");
		Debug.Log ("New manifest file location: " + pathToManifest);
		UpdateManifest ();
	}

	[MenuItem("Tools/Change Path to Shared Resources")]
	public static void ChangePathToResources ()
	{
		pathToShared = EditorUtility.OpenFolderPanel("Select path to shared resources", "Assets", "Resources");
		Debug.Log ("Path to resources is now " + pathToShared);
	}
}