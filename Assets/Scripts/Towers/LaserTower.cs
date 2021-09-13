using UnityEngine;

public class LaserTower : Tower {

	[SerializeField, Range(1f, 100f)]
	float damagePerSecond = 10f;

	[SerializeField]
	Transform turret = default, laserBeam = default;

	TargetPoint target;

	Vector3 laserBeamScale;

	void Awake () {
		laserBeamScale = laserBeam.localScale;
	}

	public override TowerType TowerType => TowerType.Laser;

	public override void GameUpdate () {
        //if could find or reach a target then shoot
		if (TrackTarget(ref target) || AcquireTarget(out target)) {
			Shoot();
		}
		else {
            //or scale it down to invisible
			laserBeam.localScale = Vector3.zero;
		}
	}

	void Shoot () {
		Vector3 point = target.Position;
		turret.LookAt(point);
		laserBeam.localRotation = turret.localRotation;

		float d = Vector3.Distance(turret.position, point);
		laserBeamScale.z = d;
		laserBeam.localScale = laserBeamScale;
		laserBeam.localPosition =
			turret.localPosition + 0.5f * d * laserBeam.forward;
        // cause enemy to damage
		target.Enemy.ApplyDamage(damagePerSecond * Time.deltaTime);
	}
}