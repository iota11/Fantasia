using UnityEngine;

public class Turret : GameBehavior {

	[SerializeField]
	Transform model = default;

	TurretFactory originFactory;
    GameTile tileFrom;
	Vector3 position;
	float speed;

	Collider targetPointCollider;
    //the way to set collider to be detected
	public Collider TargetPointCollider {
		set {
			Debug.Assert(targetPointCollider == null, "Redefined collider!");
			targetPointCollider = value;
		}
	}
    //assign and read origin factory.
	public TurretFactory OriginFactory {
		get => originFactory;
		set {
			Debug.Assert(originFactory == null, "Redefined origin factory!");
			originFactory = value;
		}
	}

	public float Scale { get; private set; }

	float Health { get; set; }

	public void ApplyDamage (float damage) {
		Debug.Assert(damage >= 0f, "Negative damage applied.");
		Health -= damage;
	}

	public override bool GameUpdate () {
		if (Health <= 0f) {

			return true;
		}
        return true;
	}

	public override void Recycle () {
		OriginFactory.Reclaim(this);
	}

	public void Initialize (
		float scale, float speed, float health
	) {
		Scale = scale;
		model.localScale = new Vector3(scale, scale, scale);
		this.speed = speed;
		Health = health;
	}

	public void SpawnOn (GameTile tile) {
        Debug.Log("Hello");
        tileFrom = tile;
        PrepareIntro();
	}

	void Awake () {
	}
	void PrepareIntro () {
        Debug.Log(tileFrom.transform.position);
		transform.position = tileFrom.transform.position;
	}
	void OnDestroy () {
	}
}