using UnityEngine;
using System.Collections;

public class Player : uLink.MonoBehaviour
{
    public Vector3 currentMovement;
    public float moveSpeed = 5;
    public CharacterController controller;

    public string meteorRoot = "http://cosmo.meteor.com";

    public float meteorUpdateSpeed = 0.2f;
    private float meteorUpdateTimer = 0f;

	public float weaponCooldown = 0.1f;
	private float weaponTimer;

	public float rotationSpeed = 10;

	private float noShootTimer;

	public string[] leftStickAxis = {"Horizontal", "Vertical"};
	public string[] rightStickAxis = {"RSHorizontal", "RSVertical"};
	
	public int weaponId;
	public int weaponAmmo;

	private tk2dTextMesh ammoText;
	private tk2dTextMesh weaponText;

	public float health = 10;
	public float maxHealth = 10;
	public float stamina = 10;
	public float maxStamina = 10;
	private bool staminaEmpty;

	public float damageCooldown = 0.2f;
	private float damageCooldownTimer;

    private CameraFollow _cameraFollow;

	public tk2dTiledSprite healthBar;
	public tk2dTiledSprite  staminaBar;

	void Awake ()
	{
        if (controller == null)
        {
            controller = GetComponent<CharacterController>();
        }

		ammoText = GameObject.Find("AmmoText").GetComponent<tk2dTextMesh>();
		weaponText = GameObject.Find("WeaponText").GetComponent<tk2dTextMesh>();

		health = maxHealth;
		stamina = maxStamina;

		UpdateAmmoHUD();
	}

    void uLink_OnNetworkInstantiate(uLink.NetworkMessageInfo info) {
        if (!info.networkView.isOwner) {
            return;
        }

        _cameraFollow = GameObject.Find ("_Cam").GetComponent<CameraFollow> ();
        _cameraFollow.playerTransform = this.transform;
		healthBar = GameObject.Find("HealthBarSliced").GetComponent<tk2dTiledSprite>();
		staminaBar = GameObject.Find("StaminaBarSliced").GetComponent<tk2dTiledSprite>();
		UpdateHealth();
    }

	void Update ()
	{
        if (!networkView.isOwner) {
        	return;
        }

		currentMovement[0] = Mathf.Abs(Input.GetAxis(leftStickAxis[0])) > 0.1f ? Input.GetAxis(leftStickAxis[0]) : 0;
		currentMovement[1] = Mathf.Abs(Input.GetAxis(leftStickAxis[1])) > 0.1f ? Input.GetAxis(leftStickAxis[1]) : 0;

		if (Input.GetAxisRaw("LeftTrigger") > -0.5f || staminaEmpty){
	    	controller.Move(currentMovement*moveSpeed*Time.deltaTime);

		}else if (!staminaEmpty){
			controller.Move(currentMovement*moveSpeed*Time.deltaTime*1.4f);
			stamina -= Time.deltaTime * 4;
		}

		float y = Input.GetAxis(rightStickAxis[1]);
		float x = Input.GetAxis(rightStickAxis[0]);

		if (Mathf.Abs(Input.GetAxis(rightStickAxis[0])) <= 0.1f && Mathf.Abs(Input.GetAxis(rightStickAxis[1])) <= 0.1f && Input.GetAxisRaw("RightTrigger") > -0.5f)
		{
			noShootTimer -= Time.deltaTime;
		}else
		{
			noShootTimer = 1.5f;
		}

		if (noShootTimer > 0){
			if (Mathf.Abs(y) > 0.1f || Mathf.Abs(x) > 0.1f ){
				Vector3 targetEuler = new Vector3(0,0, (Mathf.Atan2(y, x))*180/(Mathf.PI));

				transform.localEulerAngles = new Vector3(0,0,Mathf.LerpAngle(transform.localEulerAngles.z, targetEuler.z, Time.deltaTime * rotationSpeed));

				if (Mathf.Abs(y) > 0.6f || Mathf.Abs(x) > 0.6f ){
					//WeaponShoot();
				}
			}
		}else
		{
			y = Input.GetAxis(leftStickAxis[1]);
			x = Input.GetAxis(leftStickAxis[0]);

			if (Mathf.Abs(y) > 0.1f || Mathf.Abs(x) > 0.1f ){
				Vector3 targetEuler = new Vector3(0,0, (Mathf.Atan2(y, x))*180/(Mathf.PI));
				transform.localEulerAngles = new Vector3(0,0,Mathf.LerpAngle(transform.localEulerAngles.z, targetEuler.z, Time.deltaTime * 7));
			}
		}

		if (Input.GetAxisRaw("RightTrigger") < -0.5f)
			WeaponShoot();

		weaponTimer -= Time.deltaTime;

		if (Input.GetKeyDown(KeyCode.A))
		{
			TextPopup dmgTxt = PREFAB.SpawnPrefab(PREFAB.DAMAGE_TEXT, transform.position - new Vector3(0,0,5), "1").GetComponent<TextPopup>();
			dmgTxt.ChangeText("DEAD COSMONAUGHTS", Color.red);
		}

		damageCooldownTimer -= Time.deltaTime;

        meteorUpdateTimer -= Time.deltaTime;
        if (meteorUpdateTimer < 0f) {
            StartCoroutine (UpdateMeteor());

            meteorUpdateTimer = meteorUpdateSpeed;
        }
		UpdateStaminaHUD();
	}

    IEnumerator UpdateMeteor() {
        WWW www = new WWW(string.Format("{0}/update/{1}/{2}/{3}/{4}/{5}",meteorRoot,networkView.owner.id,transform.position.x,transform.position.y,transform.eulerAngles.z,"white"));
        Debug.Log (www.url);
        yield return www;
    }

    void WeaponShoot() {
        if (weaponTimer <= 0) {
            switch (weaponId) {
            case 0:
                weaponTimer = weaponCooldown;
                break;
            case 1:
                weaponTimer = weaponCooldown * 0.8f;
                weaponAmmo--;
                break;
            case 2:
                weaponTimer = weaponCooldown * 4f;
                weaponAmmo--;
                break;
            }

            networkView.RPC ("SpawnBullets", uLink.RPCMode.All, weaponId);

        }

        UpdateAmmoHUD();
    }

    [RPC]
    void SpawnBullets(int _weaponId) {
        Transform instance = null;
        bool mine = networkView.isOwner;
        switch (_weaponId) {
        case 0:
            instance = PREFAB.SpawnPrefab (PREFAB.BULLET, transform.position, transform.localEulerAngles - new Vector3 (0, 0, 90), "1");
            instance.GetComponent<Bullet> ().mine = mine ;
            break;
        case 1:
            instance = PREFAB.SpawnPrefab (PREFAB.BULLET, transform.position, transform.localEulerAngles - new Vector3 (0, 0, 90), "1");
            instance.GetComponent<Bullet> ().mine = mine;
            break;
        case 2:
            for (int i = 0; i < 5; i++) {
                instance = PREFAB.SpawnPrefab (PREFAB.BULLET_SHOTGUN, transform.position, transform.localEulerAngles - new Vector3 (0, 0, 76 + (i * 7)), "1");
                instance.GetComponent<Bullet> ().mine = mine;
            }
            break;
        }
    }

	public void ChangeWeapon (int id)
	{
		weaponId = id;

		switch (id)
		{
			case(1): weaponAmmo = 40; break;
			case(2): weaponAmmo = 12; break;
		}
		UpdateAmmoHUD();
	}

	void OnTriggerEnter (Collider other)
	{
		if (other.CompareTag("Bullet"))
		{
			Bullet bullet = other.GetComponent<Bullet>();


			int damage = bullet.damage;

            if (damageCooldownTimer <= 0){
                if (networkView.isOwner) {
                    networkView.RPC ("SubtractHealth", uLink.RPCMode.All, damage);
                    UpdateHealth();
                }

                StartCoroutine(Blink());
                TextPopup txtPop = PREFAB.SpawnPrefab(PREFAB.DAMAGE_TEXT, transform.position-new Vector3(0,0,5), "1").GetComponent<TextPopup>();
                txtPop.ChangeText(damage.ToString("f0"));

                damageCooldownTimer = damageCooldown;
                AudioSource.PlayClipAtPoint(PREFAB.audio.hitSound, transform.position);

            }

            HitCosmetics(damage, bullet.transform.position [0], bullet.transform.position [1]);
			PREFAB.DespawnPrefab(other.transform, "1");
		}
	}

    [RPC]
    void SubtractHealth(int damage) {
        health -= damage;
    }

    [RPC]
    void HitCosmetics(int damage, float positionX, float positionY) {
        PREFAB.SpawnPrefab(PREFAB.HIT_IMPACT, new Vector3(positionX,positionY,0.0f) - new Vector3(Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f),5), "1");
    }

	IEnumerator Blink()
	{
		renderer.material.color = Color.red;
		yield return new WaitForSeconds(0.1f);
		renderer.material.color = Color.white;
	}

	void UpdateHealth()
	{
		if (health > 0)
		{
			healthBar.dimensions = new Vector2((health / maxHealth)*200,healthBar.dimensions.y);
		}else
		{
			healthBar.dimensions = new Vector2(0,healthBar.dimensions.y);
		}
	}

	void UpdateAmmoHUD()
	{
		if (weaponId > 0 && weaponAmmo <= 0)
		{
			weaponId = 0;
		}

		if (weaponId > 0)
		{
			ammoText.text = weaponAmmo.ToString("f0");
		}else
		{
			ammoText.text = "INFINITE";
		}

		switch (weaponId)
		{
		case(0): weaponText.text = "MG"; break;
		case(1): weaponText.text = "FAST MG"; break;
		case(2): weaponText.text = "SHOTGUN"; break;
		}

		ammoText.Commit();
		weaponText.Commit();
	}

	void UpdateStaminaHUD()
	{
		if (stamina > maxStamina)
		{
			stamina = maxStamina;
		}else if (stamina < maxStamina && Input.GetAxisRaw("LeftTrigger") > 0.5f)
			stamina += Time.deltaTime * 3;

		if (!staminaEmpty && stamina <= 0)
			staminaEmpty = true;
		else if (staminaEmpty && stamina > 3)
			staminaEmpty = false;

		print (stamina);

		if (stamina > 0)
		{
			staminaBar.dimensions = new Vector2((stamina / maxStamina)*150,staminaBar.dimensions.y);
		}else
		{
			staminaBar.dimensions = new Vector2(0,staminaBar.dimensions.y);
		}
	}
}
