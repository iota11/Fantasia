using UnityEngine;

[CreateAssetMenu]
public class TurretFactory : GameObjectFactory {

	[System.Serializable]
	class TurretConfig {

		public Turret prefab = default;

		[FloatRangeSlider(0.5f, 2f)]
		public FloatRange scale = new FloatRange(1f);

		[FloatRangeSlider(0.2f, 5f)]
		public FloatRange speed = new FloatRange(1f);

		//[FloatRangeSlider(-0.4f, 0.4f)]
		//public FloatRange pathOffset = new FloatRange(0f);

		[FloatRangeSlider(10f, 1000f)]
		public FloatRange health = new FloatRange(100f);
	}

	[SerializeField]
    TurretConfig small = default, medium = default, large = default;

    TurretConfig GetConfig (TurretType type) {
		switch (type) {
			case TurretType.Small: return small;
			case TurretType.Medium: return medium;
			case TurretType.Large: return large;
		}
		Debug.Assert(false, "Unsupported enemy type!");
		return null;
	}

	public Turret Get (TurretType type = TurretType.Medium) {
        //read the type's config
        TurretConfig config = GetConfig(type);
		Turret instance = CreateGameObjectInstance(config.prefab);
		instance.OriginFactory = this;
		instance.Initialize(
			config.scale.RandomValueInRange,
			config.speed.RandomValueInRange,
			config.health.RandomValueInRange
		);
		return instance;
	}

	public void Reclaim (Turret turret) {
		Debug.Assert(turret.OriginFactory == this, "Wrong factory reclaimed!");
		Destroy(turret.gameObject);
	}
}