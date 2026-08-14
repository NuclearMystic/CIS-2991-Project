using CIS2991Project.Enemies;
using UnityEngine;
using UnityEngine.Audio;

namespace CIS2991Project.Player
{
    public class PlayerShoot : MonoBehaviour
    {
        [Header("Ammo Items — matched against the equipped weapon's ammo type when reloading")]
        [SerializeField] private global::Item pistolAmmoItem;
        [SerializeField] private global::Item shotgunAmmoItem;
        [SerializeField] private global::Item rifleAmmoItem;

        [Header("Projectile Visuals — matched against the equipped weapon's ammo type when firing")]
        [SerializeField] private Sprite pistolProjectileSprite;
        [SerializeField] private Sprite shotgunProjectileSprite;
        [SerializeField] private Sprite rifleProjectileSprite;

        [Header("Reload Timers — how long the weapon is locked once it runs dry. Kept a bit slow at 0 skill " +
                "so leveling the matching weapon skill (up to -50% at level 100) actually feels like progress.")]
        [SerializeField] private float pistolReloadSeconds = 2.5f;
        [SerializeField] private float shotgunReloadSeconds = 5f;
        [SerializeField] private float rifleReloadSecondsPerBullet = 1.25f;

        [Header("Shotgun Spread — extra pellets fired alongside the center shot, one on each side")]
        [SerializeField] private float shotgunSpreadDegrees = 8f;

        [Header("Weapon Range — how long (seconds) a projectile travels before disappearing")]
        [SerializeField] private float pistolProjectileLifetime = 4f;
        [SerializeField] private float shotgunProjectileLifetime = 0.5f;
        [SerializeField] private float rifleProjectileLifetime = 4f;

        [Header("Fire Rate — delay (seconds) between shots, per weapon")]
        [SerializeField] private float pistolFireCooldown = 0.2f;
        [SerializeField] private float shotgunFireCooldown = 0.8f;
        [SerializeField] private float rifleFireCooldown = 0.15f;

        [Header("Weapon Sounds — matched against the equipped weapon's ammo type when firing")]
        [SerializeField] private AudioClip pistolFireSound;
        [SerializeField] private AudioClip shotgunFireSound;
        [SerializeField] private AudioClip rifleFireSound;
        [SerializeField] private AudioClip dryFireSound;
        [SerializeField] private AudioClip reloadCompleteSound;
        [SerializeField] private AudioMixerGroup sfxMixerGroup;

        private Vector2 _lastDirection = Vector2.right;
        private float _cooldown;

        private PlayerInventory _inventory;
        private CharacterSheet _characterSheet;
        private global::Item _equippedWeapon;
        private int _currentAmmo;
        private bool _isReloading;
        private float _reloadTimeRemaining;
        private float _reloadTotalDuration;

        public event System.Action<Vector2> Fired;
        public event System.Action<int, int, global::WeaponAmmoType> AmmoChanged;

        public int CurrentAmmo => _currentAmmo;
        public int MaxAmmo => _equippedWeapon != null ? Mathf.Max(0, _equippedWeapon.ammoCapacity) : 0;
        public global::WeaponAmmoType CurrentAmmoType => _equippedWeapon != null ? _equippedWeapon.ammoType : global::WeaponAmmoType.None;
        public bool IsReloading => _isReloading;
        public float ReloadTimeRemaining => _isReloading ? _reloadTimeRemaining : 0f;
        public float ReloadFractionRemaining => _isReloading && _reloadTotalDuration > 0f
            ? Mathf.Clamp01(_reloadTimeRemaining / _reloadTotalDuration)
            : 0f;

        private void Awake()
        {
            _inventory = GetComponent<PlayerInventory>();
            _characterSheet = GetComponent<CharacterSheet>();
        }

        private void OnEnable()
        {
            if (_inventory != null)
                _inventory.EquipmentChanged += HandleEquipmentChanged;
        }

        private void OnDisable()
        {
            if (_inventory != null)
                _inventory.EquipmentChanged -= HandleEquipmentChanged;
        }

        private void Start()
        {
            HandleEquipmentChanged();
        }

        private void HandleEquipmentChanged()
        {
            var newWeapon = _inventory != null ? _inventory.EquippedWeapon : null;
            if (newWeapon == _equippedWeapon)
                return;

            _equippedWeapon = newWeapon;
            _currentAmmo = MaxAmmo;
            _isReloading = false;
            _reloadTimeRemaining = 0f;
            _reloadTotalDuration = 0f;
            AmmoChanged?.Invoke(_currentAmmo, MaxAmmo, CurrentAmmoType);
        }

        private void Update()
        {
            var input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            if (input != Vector2.zero && !Input.GetButton("Fire2"))
                _lastDirection = input.normalized;

            if (_cooldown > 0f)
                _cooldown -= Time.deltaTime;

            if (_isReloading)
            {
                _reloadTimeRemaining -= Time.deltaTime;
                if (_reloadTimeRemaining <= 0f)
                    FinishReload();
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                StartReload();
            }

            if (Input.GetKey(KeyCode.Space) && _cooldown <= 0f)
            {
                if (CanFire())
                {
                    SpawnProjectile();
                    _cooldown = GetFireCooldown(CurrentAmmoType);
                }
                else
                {
                    PlayDryFireSound();
                    StartReload();
                    _cooldown = GetFireCooldown(CurrentAmmoType);
                }
            }
        }

        private bool CanFire()
        {
            if (_isReloading)
                return false;

            return CurrentAmmoType == global::WeaponAmmoType.None || _currentAmmo > 0;
        }

        private bool StartReload()
        {
            if (_isReloading || _inventory == null || CurrentAmmoType == global::WeaponAmmoType.None)
                return false;

            var needed = MaxAmmo - _currentAmmo;
            var ammoItem = GetAmmoItem(CurrentAmmoType);
            if (needed <= 0 || ammoItem == null || _inventory.GetItemCount(ammoItem) <= 0)
                return false;

            _isReloading = true;
            _reloadTotalDuration = GetReloadDuration();
            _reloadTimeRemaining = _reloadTotalDuration;
            return true;
        }

        private float GetReloadDuration()
        {
            var baseDuration = CurrentAmmoType switch
            {
                global::WeaponAmmoType.Pistol => pistolReloadSeconds,
                global::WeaponAmmoType.Shotgun => shotgunReloadSeconds,
                global::WeaponAmmoType.Rifle => MaxAmmo * rifleReloadSecondsPerBullet,
                _ => 0f,
            };

            var weaponSkill = GetWeaponSkill(CurrentAmmoType);
            if (!weaponSkill.HasValue || _characterSheet == null)
                return baseDuration;

            return baseDuration * _characterSheet.GetWeaponReloadMultiplier(weaponSkill.Value);
        }

        private static SkillType? GetWeaponSkill(global::WeaponAmmoType ammoType)
        {
            return ammoType switch
            {
                global::WeaponAmmoType.Pistol => SkillType.Pistol,
                global::WeaponAmmoType.Shotgun => SkillType.Shotgun,
                global::WeaponAmmoType.Rifle => SkillType.Rifle,
                _ => null,
            };
        }

        private int GetWeaponDamage()
        {
            var baseDamage = _equippedWeapon != null ? Mathf.Max(1, _equippedWeapon.damage) : 1;

            var weaponSkill = GetWeaponSkill(CurrentAmmoType);
            if (!weaponSkill.HasValue || _characterSheet == null)
                return baseDamage;

            var multiplier = _characterSheet.GetWeaponDamageMultiplier(weaponSkill.Value);
            return Mathf.Max(1, Mathf.RoundToInt(baseDamage * multiplier));
        }

        private void FinishReload()
        {
            _isReloading = false;
            _reloadTimeRemaining = 0f;
            PlaySfx(reloadCompleteSound, GetMuzzlePosition());

            var needed = MaxAmmo - _currentAmmo;
            var ammoItem = GetAmmoItem(CurrentAmmoType);
            if (needed <= 0 || ammoItem == null)
                return;

            var amountToLoad = Mathf.Min(needed, _inventory.GetItemCount(ammoItem));
            if (amountToLoad <= 0)
                return;

            _inventory.TryRemove(ammoItem, amountToLoad);
            _currentAmmo += amountToLoad;
            AmmoChanged?.Invoke(_currentAmmo, MaxAmmo, CurrentAmmoType);
        }

        private global::Item GetAmmoItem(global::WeaponAmmoType ammoType)
        {
            switch (ammoType)
            {
                case global::WeaponAmmoType.Pistol:
                    return pistolAmmoItem;
                case global::WeaponAmmoType.Shotgun:
                    return shotgunAmmoItem;
                case global::WeaponAmmoType.Rifle:
                    return rifleAmmoItem;
                default:
                    return null;
            }
        }

        private Sprite GetProjectileSprite(global::WeaponAmmoType ammoType)
        {
            switch (ammoType)
            {
                case global::WeaponAmmoType.Pistol:
                    return pistolProjectileSprite;
                case global::WeaponAmmoType.Shotgun:
                    return shotgunProjectileSprite;
                case global::WeaponAmmoType.Rifle:
                    return rifleProjectileSprite;
                default:
                    return null;
            }
        }

        private Vector2 GetMuzzlePosition()
        {
            return (Vector2)transform.position + _lastDirection * 0.6f;
        }

        private void SpawnProjectile()
        {
            var muzzlePosition = GetMuzzlePosition();
            var weaponDamage = GetWeaponDamage();

            if (CurrentAmmoType == global::WeaponAmmoType.Shotgun)
            {
                SpawnPellet(muzzlePosition, _lastDirection, weaponDamage);
                SpawnPellet(muzzlePosition, Rotate(_lastDirection, -shotgunSpreadDegrees), weaponDamage);
                SpawnPellet(muzzlePosition, Rotate(_lastDirection, shotgunSpreadDegrees), weaponDamage);
            }
            else
            {
                SpawnPellet(muzzlePosition, _lastDirection, weaponDamage);
            }

            if (CurrentAmmoType != global::WeaponAmmoType.None)
            {
                _currentAmmo = Mathf.Max(0, _currentAmmo - 1);
                AmmoChanged?.Invoke(_currentAmmo, MaxAmmo, CurrentAmmoType);
            }

            PlaySfx(GetFireSound(CurrentAmmoType), muzzlePosition);

            Fired?.Invoke(_lastDirection);
        }

        private void SpawnPellet(Vector2 position, Vector2 direction, int damage)
        {
            var go = new GameObject("Projectile");
            go.transform.position = position;

            var projectileVisual = GetProjectileSprite(CurrentAmmoType);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = projectileVisual != null ? projectileVisual : CreateCircleSprite(Color.white);
            sr.sortingOrder = 2;
            go.transform.localScale = projectileVisual != null ? Vector3.one : new Vector3(0.3f, 0.3f, 1f);

            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.freezeRotation = true;

            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;

            go.AddComponent<Projectile>().Initialize(direction, damage, GetProjectileLifetime(CurrentAmmoType));
        }

        private float GetProjectileLifetime(global::WeaponAmmoType ammoType)
        {
            return ammoType switch
            {
                global::WeaponAmmoType.Pistol => pistolProjectileLifetime,
                global::WeaponAmmoType.Shotgun => shotgunProjectileLifetime,
                global::WeaponAmmoType.Rifle => rifleProjectileLifetime,
                _ => pistolProjectileLifetime,
            };
        }

        private float GetFireCooldown(global::WeaponAmmoType ammoType)
        {
            return ammoType switch
            {
                global::WeaponAmmoType.Pistol => pistolFireCooldown,
                global::WeaponAmmoType.Shotgun => shotgunFireCooldown,
                global::WeaponAmmoType.Rifle => rifleFireCooldown,
                _ => pistolFireCooldown,
            };
        }

        private static Vector2 Rotate(Vector2 vector, float degrees)
        {
            var radians = degrees * Mathf.Deg2Rad;
            var sin = Mathf.Sin(radians);
            var cos = Mathf.Cos(radians);
            return new Vector2(vector.x * cos - vector.y * sin, vector.x * sin + vector.y * cos);
        }

        private void PlayDryFireSound()
        {
            PlaySfx(dryFireSound, GetMuzzlePosition());
        }

        private void PlaySfx(AudioClip clip, Vector2 position)
        {
            if (clip == null)
                return;

            var go = new GameObject("SFX_" + clip.name);
            go.transform.position = position;

            var source = go.AddComponent<AudioSource>();
            source.clip = clip;
            source.outputAudioMixerGroup = sfxMixerGroup;
            source.spatialBlend = 0f;
            source.Play();

            Object.Destroy(go, clip.length);
        }

        private AudioClip GetFireSound(global::WeaponAmmoType ammoType)
        {
            switch (ammoType)
            {
                case global::WeaponAmmoType.Pistol:
                    return pistolFireSound;
                case global::WeaponAmmoType.Shotgun:
                    return shotgunFireSound;
                case global::WeaponAmmoType.Rifle:
                    return rifleFireSound;
                default:
                    return null;
            }
        }

        private static Sprite CreateCircleSprite(Color color)
        {
            const int size = 16;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var pixels = new Color32[size * size];
            var center = (size - 1) / 2f;
            var radius = size / 2f - 0.5f;

            for (var i = 0; i < pixels.Length; i++)
            {
                float x = i % size;
                float y = i / size;
                pixels[i] = Vector2.Distance(new Vector2(x, y), new Vector2(center, center)) <= radius
                    ? (Color32)color
                    : new Color32(0, 0, 0, 0);
            }

            texture.SetPixels32(pixels);
            texture.Apply();
            return Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size);
        }

        private class Projectile : MonoBehaviour
        {
            private const float Speed = 24f;

            private float _remaining;
            private int _damage;

            public void Initialize(Vector2 direction, int damage, float lifetime)
            {
                _remaining = lifetime;
                _damage = Mathf.Max(1, damage);
                GetComponent<Rigidbody2D>().linearVelocity = direction * Speed;
            }

            private void Update()
            {
                _remaining -= Time.deltaTime;
                if (_remaining <= 0f)
                    Destroy(gameObject);
            }

            private void OnTriggerEnter2D(Collider2D other)
            {
                if (other.GetComponent<Projectile>() != null)
                    return;

                var enemy = other.GetComponent<Enemy>();
                if (enemy != null)
                {
                    enemy.TakeHit(GetComponent<Rigidbody2D>().linearVelocity.normalized, _damage);
                    Destroy(gameObject);
                    return;
                }

                if (other.GetComponentInParent<PlayerHealth>() == null)
                    Destroy(gameObject);
            }
        }
    }
}
