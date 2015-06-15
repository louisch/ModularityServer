using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PrefabTracker : MonoBehaviour {
	public TextAsset manifest;
	public List<string> all;
	public List<string> players = new List<string>();
	public Dictionary<string, GameObject> loadedPlayerModules = new Dictionary<string, GameObject>();

	public List<string> projectiles = new List<string>();
	public Dictionary<string, GameObject> loadedProjectiles = new Dictionary<string, GameObject>();

	public List<string> modules = new List<string>();
	public Dictionary<string, GameObject> loadedModules = new Dictionary<string, GameObject>();

	public string playersPath = "Players";
	public string projectilesPath = "Projectiles";

	public GameObject defaultPrefab;

	bool loading = true;

	void Awake ()
	{
		StartCoroutine(readManifest());
	}

	// Reads in the manifest and calls the line parser.
	public IEnumerator readManifest()
	{
		if (manifest == null)
		{
			Debug.LogError ("No manifest file not found.");
			yield break;
		}
		all.AddRange(manifest.text.Split('\n'));
		yield return StartCoroutine(parsePaths(all));
		loading = false;
	}

	// Parses lines retrieved from manifest
	IEnumerator parsePaths (IEnumerable<string> paths)
	{
		foreach (string path in paths)
		{
			yield return null;
			if (path == "")
			{
				continue;
			}

			string p = path.Trim();

			if (path.Contains(playersPath))
			{
   				players.Add(p);
			}
			else if (path.Contains(projectilesPath))
			{
				projectiles.Add(p);
			}
			else
			{
   				modules.Add(p);
			}
		}

		Debug.Log("Found players: " + players.Count);
		Debug.Log (players[0] + " " + players[0].Length);
		Debug.Log("Found projectiles: " + projectiles.Count);
		Debug.Log("Found generic modules: " + modules.Count);
	}

	public void GetRandomPlayerModel (out GameObject playerPrefab, out string prefabPath)
	{
		prefabPath = players[Random.Range(0,players.Count)];
		playerPrefab = GetAsset(prefabPath, loadedPlayerModules);
		Debug.Log ("Got player model at path: " + prefabPath);
	}

	public void GetRandomModel (out GameObject playerPrefab, out string prefabPath)
	{
		prefabPath = modules[Random.Range(0,modules.Count)];
		playerPrefab = GetAsset(prefabPath, loadedModules);
		Debug.Log ("Got player model at path: " + prefabPath);
	}

	public GameObject GetModule (string path)
	{
		return GetAsset(path, loadedModules);
	}

	public GameObject GetAsset (string path, Dictionary<string,GameObject> loadedModules)
	{

		GameObject modulePrefab;
		if (!loadedModules.TryGetValue(path, out modulePrefab))
		{
			modulePrefab = Resources.Load<GameObject> (path);
			if (modulePrefab == null)
			{
				Debug.LogWarningFormat ("Could not load asset at path '{0}'. Loading default asset instead.", path);
				modulePrefab = defaultPrefab;
			}
			else
			{
				loadedModules.Add(path,modulePrefab);
			}
		}
		else
		{
			Debug.Log ("Loaded module!");
		}

		return modulePrefab;
	}
}
