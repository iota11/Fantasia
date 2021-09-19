using UnityEngine;

public class Enemy : GameBehavior {

	[SerializeField]
	EnemyAnimationConfig animationConfig = default;

	[SerializeField]
	Transform model = default;

	EnemyFactory originFactory;

	GameTile tileFrom, tileTo, tileAimed;
	Vector3 positionFrom, positionTo;
	Direction direction;
	DirectionChange directionChange;
	float directionAngleFrom, directionAngleTo;
	float progress, progressFactor;
	float pathOffset;
	float speed;

	EnemyAnimator animator;

	Collider targetPointCollider;

	public Collider TargetPointCollider {
		set {
			Debug.Assert(targetPointCollider == null, "Redefined collider!");
			targetPointCollider = value;
		}
	}

	public EnemyFactory OriginFactory {
		get => originFactory;
		set {
			Debug.Assert(originFactory == null, "Redefined origin factory!");
			originFactory = value;
		}
	}

	public bool IsValidTarget => animator.CurrentClip == EnemyAnimator.Clip.Move;

	public float Scale { get; private set; }

	float Health { get; set; }
    // prepare for detection
    public static int BufferedCount { get; private set; }
    const int towerLayerMask = 1 << 10;
    private Tower towerAimed;
    static Collider[] buffer = new Collider[100];

    //撒网抓鱼
    public static bool FillBuffer(Vector3 position, float range)
    {
        Vector3 top = position;
        top.y += 3f;
        //get how many towers in scope
        BufferedCount = Physics.OverlapCapsuleNonAlloc(
            position, top, range, buffer, towerLayerMask
        );
        return BufferedCount > 0;
    }

    public static Tower GetBuffered(int index)
    {
        //this target is the enemy targetPoint.
        var target = buffer[index].GetComponent<Tower>();
        Debug.Assert(target != null, "Targeted non-tower!", buffer[0]);
        return target;
    }

    public void ApplyDamage (float damage) {
		Debug.Assert(damage >= 0f, "Negative damage applied.");
		Health -= damage;
	}

	public override bool GameUpdate () {
#if UNITY_EDITOR
		if (!animator.IsValid) {
			animator.RestoreAfterHotReload(
				model.GetChild(0).GetComponent<Animator>(),
				animationConfig,
				animationConfig.MoveAnimationSpeed * speed / Scale
			);
		}
#endif
		animator.GameUpdate();

		if (animator.CurrentClip == EnemyAnimator.Clip.Intro) {
			if (!animator.IsDone) {
				return true;
			}
			animator.PlayMove(animationConfig.MoveAnimationSpeed * speed / Scale);
			targetPointCollider.enabled = true;
		}
		else if (animator.CurrentClip >= EnemyAnimator.Clip.Outro) {
			if (animator.IsDone) {
				Recycle();
				return false;
			}
			return true;
		}

		if (Health <= 0f) {
			animator.PlayDying();
			targetPointCollider.enabled = false;
			return true;
		}

        progress += 0;
        if (true)
        {
            if (FillBuffer(transform.position, 2))
            {
                progress += 0;
                //towerAimed = GetBuffered(0);
                //towerAimed.ApplyDamage();
            }
            else
            {
                progress += Time.deltaTime * progressFactor;
            }
        }
       
        //progress >=1 means "locomotion achieve current goal", prepare for next goal plz
        //progress is always ++
        while (progress >= 1f)
        {
            if (tileTo == null)
            {
                Game.EnemyReachedDestination();
                animator.PlayOutro();
                targetPointCollider.enabled = false;
                return true;
            }
            progress = (progress - 1f) / progressFactor;
            PrepareNextState();
            progress *= progressFactor;
        }

        //lerp the animation to move or to rotate
        if (directionChange == DirectionChange.None)
        {
            transform.localPosition =
                Vector3.LerpUnclamped(positionFrom, positionTo, progress);
        }
        else
        {
            float angle = Mathf.LerpUnclamped(
                directionAngleFrom, directionAngleTo, progress
            );
            transform.localRotation = Quaternion.Euler(0f, angle, 0f);
        }
        return true;
	}

	public override void Recycle () {
		animator.Stop();
		OriginFactory.Reclaim(this);
	}

	public void Initialize (
		float scale, float speed, float pathOffset, float health
	) {
		Scale = scale;
		model.localScale = new Vector3(scale, scale, scale);
		this.speed = speed;
		this.pathOffset = pathOffset;
		Health = health;
		animator.PlayIntro();
		targetPointCollider.enabled = false;
	}
 



    public void SpawnOn (GameTile tile) {
		tileFrom = tile;
		tileTo = tile.NextTileOnPath;
		progress = 0f;
		PrepareIntro();
	}

	void Awake () {
		animator.Configure(
			model.GetChild(0).gameObject.AddComponent<Animator>(),
			animationConfig
		);
	}

	void PrepareNextState () {
		tileFrom = tileTo;
		tileTo = tileTo.NextTileOnPath;
		positionFrom = positionTo;
		if (tileTo == null) {
			PrepareOutro();
			return;
		}
		positionTo = tileFrom.ExitPoint;
        //compare current direction and the next direction
		directionChange = direction.GetDirectionChangeTo(tileFrom.PathDirection);
        //get the next direction
		direction = tileFrom.PathDirection;
        //update the current direction
		directionAngleFrom = directionAngleTo;
		switch (directionChange) {
			case DirectionChange.None: PrepareForward(); break;
			case DirectionChange.TurnRight: PrepareTurnRight(); break;
			case DirectionChange.TurnLeft: PrepareTurnLeft(); break;
			default: PrepareTurnAround(); break;
		}
	}

	void PrepareForward () {
		transform.localRotation = direction.GetRotation();
		directionAngleTo = direction.GetAngle();
		model.localPosition = new Vector3(pathOffset, 0f);
		progressFactor = speed;
	}

	void PrepareTurnRight () {
		directionAngleTo = directionAngleFrom + 90f;
        // r = pathoffset - 0.5
		model.localPosition = new Vector3(pathOffset - 0.5f, 0f);
        //pivot location
		transform.localPosition = positionFrom + direction.GetHalfVector();
		progressFactor = speed / (Mathf.PI * 0.5f * (0.5f - pathOffset));
	}

	void PrepareTurnLeft () {
		directionAngleTo = directionAngleFrom - 90f;
        // r = pathoffset +0.5
        model.localPosition = new Vector3(pathOffset + 0.5f, 0f);
		transform.localPosition = positionFrom + direction.GetHalfVector();
		progressFactor = speed / (Mathf.PI * 0.5f * (0.5f + pathOffset));
	}

	void PrepareTurnAround () {
		directionAngleTo = directionAngleFrom + (pathOffset < 0f ? 180f : -180f);
		model.localPosition = new Vector3(pathOffset, 0f);
		transform.localPosition = positionFrom;
		progressFactor =
			speed / (Mathf.PI * Mathf.Max(Mathf.Abs(pathOffset), 0.2f));
	}

	void PrepareIntro () {
		positionFrom = tileFrom.transform.localPosition;
		transform.localPosition = positionFrom;
		positionTo = tileFrom.ExitPoint;
		direction = tileFrom.PathDirection;
		directionChange = DirectionChange.None;
		directionAngleFrom = directionAngleTo = direction.GetAngle();
		model.localPosition = new Vector3(pathOffset, 0f);
		transform.localRotation = direction.GetRotation();
		progressFactor = 2f * speed;
	}

	void PrepareOutro () {
		positionTo = tileFrom.transform.localPosition;
		directionChange = DirectionChange.None;
		directionAngleTo = direction.GetAngle();
		model.localPosition = new Vector3(pathOffset, 0f);
		transform.localRotation = direction.GetRotation();
		progressFactor = 2f * speed;
	}

	void OnDestroy () {
		animator.Destroy();
	}
}