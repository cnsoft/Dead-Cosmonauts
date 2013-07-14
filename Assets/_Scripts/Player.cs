using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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
	private bool dead;

	public float damageCooldown = 0.2f;
	private float damageCooldownTimer;

    private CameraFollow _cameraFollow;

	public tk2dTiledSprite healthBar;
	public tk2dTiledSprite  staminaBar;

	private Light2D flashlight;

	private StatsScreen statsScreen;

	private tk2dSprite spriteTorso;
	public tk2dSpriteAnimator torsoAnim;

	public Color runColor;

	private SpawnPoints spawnPoints;

	public string heroColorName = "Blue";

	public Transform bulletSpawnPoint;

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
		flashlight = GetComponentInChildren<Light2D>();

		UpdateAmmoHUD();
	}

    void uLink_OnNetworkInstantiate(uLink.NetworkMessageInfo info) {
        spriteTorso = GetComponentInChildren<tk2dSprite>();
        spawnPoints = (SpawnPoints)FindObjectOfType(typeof(SpawnPoints));
        statsScreen = (StatsScreen)FindObjectOfType(typeof(StatsScreen));

        if (!info.networkView.isOwner) {
            return;
        }
        SetName (info.networkView.owner.id, name);
        this.networkView.RPC ("SetName", uLink.RPCMode.Others, info.networkView.owner.id, name);

        _cameraFollow = GameObject.Find ("_Cam").GetComponent<CameraFollow> ();
        _cameraFollow.playerTransform = this.transform;
		healthBar = GameObject.Find("HealthBarSliced").GetComponent<tk2dTiledSprite>();
		staminaBar = GameObject.Find("StaminaBarSliced").GetComponent<tk2dTiledSprite>();
		statsScreen = (StatsScreen)FindObjectOfType(typeof(StatsScreen));
		spriteTorso = GetComponentInChildren<tk2dSprite>();
		spawnPoints = (SpawnPoints)FindObjectOfType(typeof(SpawnPoints));
		UpdateHealth();

		torsoAnim.Play(heroColorName+"_2");
    }

	void Update ()
	{
        if (!networkView.isOwner || dead) {
        	return;
        }

		if (Input.GetButtonDown("Flashlight"))
		{
			print ("FLASH");


			if (flashlight.LightRadius > 10)
				flashlight.LightRadius = 0;
			else
				flashlight.LightRadius = 16;
		}

		if (Input.GetButtonDown("Stats"))
		{
			statsScreen.ToggleStats();
		}

		currentMovement[0] = Mathf.Abs(Input.GetAxis(leftStickAxis[0])) > 0.1f ? Input.GetAxis(leftStickAxis[0]) : 0;
		currentMovement[1] = Mathf.Abs(Input.GetAxis(leftStickAxis[1])) > 0.1f ? Input.GetAxis(leftStickAxis[1]) : 0;

		if (Input.GetAxisRaw("LeftTrigger") > -0.5f || staminaEmpty){
	    	controller.Move(currentMovement*moveSpeed*Time.deltaTime);
			spriteTorso.color = Color.white;
		}else if (!staminaEmpty){
			controller.Move(currentMovement*moveSpeed*Time.deltaTime*1.4f);
			stamina -= Time.deltaTime * 2.5f;
			spriteTorso.color = runColor;
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

		damageCooldownTimer -= Time.deltaTime;

        meteorUpdateTimer -= Time.deltaTime;
        if (meteorUpdateTimer < 0f) {
            StartCoroutine (UpdateMeteor());

            meteorUpdateTimer = meteorUpdateSpeed;
        }
		UpdateStaminaHUD();
	}

    IEnumerator UpdateMeteor() {
        WWW www = new WWW(string.Format("{0}/update/{1}/{2}/{3}/{4}",meteorRoot,networkView.owner.id,transform.position.x,transform.position.y,transform.eulerAngles.z));
        yield return www;
    }

	void LateUpdate()
	{
		//torsoAnim.transform.eulerAngles = Vector3.zero;

		//AngleAnimations();
	}

	void AngleAnimations()
	{
		if (transform.eulerAngles.z > 0 && transform.eulerAngles.z <= 22.5f)
		{
			torsoAnim.Play(heroColorName+"_6");
		}else if (transform.eulerAngles.z > 22.5f && transform.eulerAngles.z <= 67.5f)
		{
			torsoAnim.Play(heroColorName+"_9");
		}else if (transform.eulerAngles.z > 67.5f && transform.eulerAngles.z <= 112.5f)
		{
			torsoAnim.Play(heroColorName+"_8");
		}else if (transform.eulerAngles.z > 112.5f && transform.eulerAngles.z <= 157.5f)
		{
			torsoAnim.Play(heroColorName+"_7");
		}else if (transform.eulerAngles.z > 157.5f && transform.eulerAngles.z <= 202.5f)
		{
			torsoAnim.Play(heroColorName+"_4");
		}else if (transform.eulerAngles.z > 202.5f && transform.eulerAngles.z <= 247.5f)
		{
			torsoAnim.Play(heroColorName+"_1");
		}else if (transform.eulerAngles.z > 247.5f && transform.eulerAngles.z <= 292.5f)
		{
			torsoAnim.Play(heroColorName+"_2");
		}else if (transform.eulerAngles.z > 292.5f && transform.eulerAngles.z <= 337.5f)
		{
			torsoAnim.Play(heroColorName+"_3");
		}else
		{
			torsoAnim.Play(heroColorName+"_6");
		}
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

			torsoAnim.Play(heroColorName+"_Shoot");

			//AudioSource.PlayClipAtPoint(PREFAB.audio.shootSound, transform.position);

            this.networkView.RPC ("SpawnBullets", uLink.RPCMode.All, weaponId);

        }

        UpdateAmmoHUD();
    }

    [RPC]
    void SpawnBullets(int _weaponId) {
        Transform instance = null;
        int owner = uLink.Network.player.id;
        switch (_weaponId) {
        case 0:
            instance = PREFAB.SpawnPrefab (PREFAB.BULLET, bulletSpawnPoint.position, transform.localEulerAngles - new Vector3 (0, 0, 90), "1");
            instance.GetComponent<Bullet> ().owner = owner ;
            break;
        case 1:
			instance = PREFAB.SpawnPrefab (PREFAB.BULLET, bulletSpawnPoint.position, transform.localEulerAngles - new Vector3 (0, 0, 90), "1");
            instance.GetComponent<Bullet> ().owner = owner ;
            break;
        case 2:
            for (int i = 0; i < 5; i++) {
				instance = PREFAB.SpawnPrefab (PREFAB.BULLET_SHOTGUN, bulletSpawnPoint.position, transform.localEulerAngles - new Vector3 (0, 0, 76 + (i * 7)), "1");
                instance.GetComponent<Bullet> ().owner = owner ;
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
		if (!dead && other.CompareTag("Bullet"))
		{
			Bullet bullet = other.GetComponent<Bullet>();


			int damage = bullet.damage;


            if (damageCooldownTimer <= 0){
                if (networkView.isOwner) {
                    int spawn = Random.Range(0, spawnPoints.spawnPoint.Length-1);
                    networkView.RPC ("SubtractHealth", uLink.RPCMode.All, damage, bullet.owner);
                    UpdateHealth();
                }

                StartCoroutine("Blink");
                TextPopup txtPop = PREFAB.SpawnPrefab(PREFAB.DAMAGE_TEXT, transform.position-new Vector3(0,0,5), "1").GetComponent<TextPopup>();
                txtPop.ChangeText(damage.ToString("f0"));

                damageCooldownTimer = damageCooldown;
                AudioSource.PlayClipAtPoint(PREFAB.audio.hitSound, transform.position);

            }

            networkView.RPC("HitCosmetics", uLink.RPCMode.All, damage, bullet.transform.position [0], bullet.transform.position [1]);
			PREFAB.DespawnPrefab(other.transform, "1");
		}
	}

    public void Healthpack ()
    {
        networkView.RPC ("FullHealth", uLink.RPCMode.All);
    }

    [RPC]
    void FullHealth() {
        health = maxHealth;
        if (networkView.isOwner) {
            UpdateHealth ();
        }
    }

    [RPC]
    void SetDead(bool isDead) {
        dead = isDead;
    }

    string name {
        get {
            return (string)(uLink.Network.loginData.Length != 0 ? uLink.Network.loginData [0] : uLink.Network.player);
        }
    }

    [RPC]
    void SubtractHealth(int damage, int bulletOwner) {
        health -= damage;

		if (health <= 0 && !dead)
		{
            DeathCosmetics ();

            if (networkView.isOwner) {
                dead = true;

                StartCoroutine (UnsetDead ());
                networkView.RPC ("AddDeath", uLink.RPCMode.All, uLink.Network.player.id);
                networkView.RPC ("AddKill", uLink.RPCMode.All, bulletOwner);
            }

            networkView.RPC ("SetDead", uLink.RPCMode.All, true);
		}
    }

    [RPC]
    void SetName(int id, string name) {
        Stat stat = stats.Find (s => s.id == id);
        if (stat == null) {
            stat = new Stat ();
            stats.Add (stat);
        }
        stat.name = name;
        stat.id = id;

        if (networkView.isOwner) {
            statsScreen.UpdateStats (stats);
        }
    }

    IEnumerator UnsetDead() {
        yield return new WaitForSeconds (5.0f);

        FullHealth();
        SetDead(false);
        networkView.RPC ("FullHealth",uLink.RPCMode.Others);
        networkView.RPC ("SetDead", uLink.RPCMode.Others, false);

        stamina = maxStamina;
        UpdateStaminaHUD();
        UpdateHealth();
    }

    void DeathCosmetics() {
        PREFAB.SpawnPrefab(PREFAB.EXPLOSION2, transform.position, "1");
        PREFAB.audio.PlayRandomKillSound();
        StopCoroutine("Blink");
        spriteTorso.color = Color.clear;
        flashlight.LightRadius = 0;

        StartCoroutine (DeathCosmeticsCoroutine ());
    }

    IEnumerator DeathCosmeticsCoroutine() {
        yield return new WaitForSeconds (5.0f);
        spriteTorso.color = Color.white;
    }

    [RPC]
    void HitCosmetics(int damage, float positionX, float positionY) {
        PREFAB.SpawnPrefab(PREFAB.HIT_IMPACT, new Vector3(positionX,positionY,0.0f) - new Vector3(Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f),5), "1");
    }

	IEnumerator Blink()
	{
		//renderer.material.color = Color.red;

		spriteTorso.color = Color.red;

		yield return new WaitForSeconds(0.1f);

		spriteTorso.color = Color.red;
		//renderer.material.color = Color.white;
	}


	void DeathEvent()
	{

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

    public class Stat {
        public int id;
        public string name;
        public int Kills;
        public int Deaths;
    }

    List<Stat> stats = new List<Stat>();

    [RPC]
    void AddKill (int id) {
        Stat stat = stats.Find (s => s.id == id);
        if (stat == null) {
//            return;
            stat = new Stat ();
            stats.Add (stat);
        }
      
        stat.Kills++;

        if (networkView.isOwner) {
            statsScreen.UpdateStats (stats);
        }
    }

    [RPC]
    void AddDeath (int id) {
        Stat stat = stats.Find (s => s.id == id);
        if (stat == null) {
//            return;
            stat = new Stat ();
            stats.Add (stat);
        }
        stat.Deaths++;

        if (networkView.isOwner) {
            statsScreen.UpdateStats (stats);
        }
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
     
		if (stamina > 0)
		{
			staminaBar.dimensions = new Vector2((stamina / maxStamina)*150,staminaBar.dimensions.y);
		}else
		{
			staminaBar.dimensions = new Vector2(0,staminaBar.dimensions.y);
		}
	}
}
