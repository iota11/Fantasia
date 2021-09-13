using UnityEngine;

[CreateAssetMenu]
public class GameTileContentFactory : GameObjectFactory {

	[SerializeField]
	GameTileContent destinationPrefab = default;

	[SerializeField]
	GameTileContent emptyPrefab = default;

	[SerializeField]
	GameTileContent wallPrefab = default;

	[SerializeField]
	GameTileContent spawnPointPrefab = default;

	[SerializeField]
	Tower[] towerPrefabs = default;

    // tackle " tile content" and " tower"



    // this is to assign a content (actually a prefab) to a tile.
    //since it is called by a tile, the location will be centered as tile pivot.
	public GameTileContent Get (GameTileContentType type) {
		switch (type) {
			case GameTileContentType.Destination: return Get(destinationPrefab);
			case GameTileContentType.Empty: return Get(emptyPrefab);
			case GameTileContentType.Wall: return Get(wallPrefab);
			case GameTileContentType.SpawnPoint: return Get(spawnPointPrefab);
		}
		Debug.Assert(false, "Unsupported non-tower type: " + type);
		return null;
	}

    // if to get a tower, interpret "tower" to actual prefab, and get it
	public Tower Get (TowerType type) {
		Debug.Assert((int)type < towerPrefabs.Length, "Unsupported tower type!");
		Tower prefab = towerPrefabs[(int)type];
		Debug.Assert(type == prefab.TowerType, "Tower prefab at wrong index!");
		return Get(prefab);
	}

	public void Reclaim (GameTileContent content) {
		Debug.Assert(content.OriginFactory == this, "Wrong factory reclaimed!");
		Destroy(content.gameObject);
	}

    // the real "get" method !!!
    // create a instance and point it to this factory 
	T Get<T> (T prefab) where T : GameTileContent {
		T instance = CreateGameObjectInstance(prefab);
		instance.OriginFactory = this;
		return instance;
	}
}