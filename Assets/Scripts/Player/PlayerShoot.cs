using System.Collections.Generic;
using CIS2991Project.Core;
using CIS2991Project.Enemies;
using UnityEngine;
using UnityEngine.Audio;

namespace CIS2991Project.Player
{
    public class PlayerShoot : MonoBehaviour
    {
        // Groups the six per-WeaponAmmoType fields below (item/sprite/lifetime/cooldown/sound/reload)
        // so every accessor is one dictionary lookup instead of its own switch statement. Built once
        // in Awake from the existing flat [SerializeField] fields, which stay as they are rather than
        // being reshaped into per-ammo-type Inspector groups - that would need every value re-entered
        // by hand in the Editor for no behavioral gain.
        private readonly struct AmmoProfile
        {
            public readonly global::Item Item;
            public readonly Sprite ProjectileSprite;
            public readonly float ProjectileLifetime;
            public readonly float FireCooldown;
            public readonly AudioClip FireSound;
            public readonly float ReloadSeconds;
            public readonly bool ReloadScalesWithMagazineSize;

            public AmmoProfile(global::Item item, Sprite projectileSprite, float projectileLifetime, float fireCooldown,
                AudioClip fireSound, float reloadSeconds, bool reloadScalesWithMagazineSize)
            {
                Item = item;
                ProjectileSprite = projectileSprite;
                ProjectileLifetime = projectileLifetime;
                FireCooldown = fireCooldown;
                FireSound = fireSound;
                ReloadSeconds = reloadSeconds;
                ReloadScalesWithMagazineSize = reloadScalesWithMagazineSize;
            }
        }

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

        [Header("Melee — used when the equipped weapon's category is Melee")]
        [SerializeField] private AudioClip meleeSwingSound;

        private Vector2 _lastDirection = Vector2.right;
        private float _cooldown;

        private PlayerInventory _inventory;
        private CharacterSheet _characterSheet;
        private global::Item _equippedWeapon;
        private int _currentAmmo;
        private bool _isReloading;
        private float _reloadTimeRemaining;
        private float _reloadTotalDuration;

        // Items aren't unique instances in this game (a "Makeshift Pistol" reference is shared by every
        // copy of it), so remembering ammo per weapon *type* here is exactly the right granularity -
        // this is what keeps switching weapons on the hotbar from refilling whatever you switch back to.
        private readonly Dictionary<global::Item, int> _savedAmmoByWeapon = new();
        private Dictionary<global::WeaponAmmoType, AmmoProfile> _ammoProfiles;

        public event System.Action<Vector2> Fired;
        public event System.Action<int, int, global::WeaponAmmoType> AmmoChanged;
        public event System.Action MeleeAttacked;

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

            _ammoProfiles = new Dictionary<global::WeaponAmmoType, AmmoProfile>
            {
                [global::WeaponAmmoType.Pistol] = new AmmoProfile(
                    pistolAmmoItem, pistolProjectileSprite, pistolProjectileLifetime, pistolFireCooldown,
                    pistolFireSound, pistolReloadSeconds, reloadScalesWithMagazineSize: false),
                [global::WeaponAmmoType.Shotgun] = new AmmoProfile(
                    shotgunAmmoItem, shotgunProjectileSprite, shotgunProjectileLifetime, shotgunFireCooldown,
                    shotgunFireSound, shotgunReloadSeconds, reloadScalesWithMagazineSize: false),
                [global::WeaponAmmoType.Rifle] = new AmmoProfile(
                    rifleAmmoItem, rifleProjectileSprite, rifleProjectileLifetime, rifleFireCooldown,
                    rifleFireSound, rifleReloadSecondsPerBullet, reloadScalesWithMagazineSize: true),
            };
        }

        private AmmoProfile? GetAmmoProfile(global::WeaponAmmoType ammoType) =>
            _ammoProfiles.TryGetValue(ammoType, out var profile) ? profile : null;

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

            // Remember how much ammo is left in the weapon we're switching away from, so switching
            // back later picks up where it left off instead of refilling.
            if (_equippedWeapon != null)
                _savedAmmoByWeapon[_equippedWeapon] = _currentAmmo;

            _equippedWeapon = newWeapon;
            _currentAmmo = newWeapon != null && _savedAmmoByWeapon.TryGetValue(newWeapon, out var savedAmmo)
                ? savedAmmo
                : MaxAmmo;
            _isReloading = false;
            _reloadTimeRemaining = 0f;
            _reloadTotalDuration = 0f;
            AmmoChanged?.Invoke(_currentAmmo, MaxAmmo, CurrentAmmoType);
        }

        private void Update()
        {
            // Without this, aiming/firing/reload input still reacts every frame while paused (only
            // Time.deltaTime-driven progress, like _cooldown counting down, is actually frozen).
            if (Time.timeScale == 0f)
            {
                return;
            }

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
                if (_equippedWeapon == null)
                {
                    // PlayerInventory auto-equips its fistsItem whenever nothing else is equipped, so
                    // this only happens if that fallback wasn't configured - nothing to attack with.
                }
                else if (IsMeleeMode)
                {
                    PerformMeleeAttack();
                    _cooldown = GetMeleeCooldown();
                }
                else if (CanFire())
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

        private bool IsMeleeMode => _equippedWeapon.weaponCategory == global::WeaponCategory.Melee;

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
            var profile = GetAmmoProfile(CurrentAmmoType);
            var baseDuration = profile == null
                ? 0f
                : profile.Value.ReloadScalesWithMagazineSize
                    ? profile.Value.ReloadSeconds * MaxAmmo
                    : profile.Value.ReloadSeconds;

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

        private void PerformMeleeAttack()
        {
            var range = Mathf.Max(0.05f, _equippedWeapon.meleeRange);
            var damage = GetMeleeDamage();
            var strikeCenter = (Vector2)transform.position + _lastDirection * (range * 0.5f);

            var hits = Physics2D.OverlapCircleAll(strikeCenter, range * 0.5f);
            var hitSomething = false;
            foreach (var hit in hits)
            {
                var enemy = hit.GetComponent<Enemy>();
                if (enemy != null)
                {
                    enemy.TakeHit(_lastDirection, damage);
                    hitSomething = true;
                }
            }

            PlaySfx(meleeSwingSound, GetMuzzlePosition());
            if (hitSomething)
                PlaySfx(_equippedWeapon.hitSound, strikeCenter);

            MeleeAttacked?.Invoke();
        }

        private int GetMeleeDamage()
        {
            var baseDamage = Mathf.Max(1, _equippedWeapon.damage);
            if (_characterSheet == null)
                return baseDamage;

            var multiplier = _characterSheet.GetWeaponDamageMultiplier(SkillType.Melee);
            return Mathf.Max(1, Mathf.RoundToInt(baseDamage * multiplier));
        }

        private float GetMeleeCooldown()
        {
            var baseCooldown = Mathf.Max(0.05f, _equippedWeapon.meleeAttackCooldown);
            if (_characterSheet == null)
                return baseCooldown;

            // Reuses the "reload speed" multiplier as swing speed - same "faster at higher skill" formula fits both.
            return baseCooldown * _characterSheet.GetWeaponReloadMultiplier(SkillType.Melee);
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

        private global::Item GetAmmoItem(global::WeaponAmmoType ammoType) => GetAmmoProfile(ammoType)?.Item;

        private Sprite GetProjectileSprite(global::WeaponAmmoType ammoType) => GetAmmoProfile(ammoType)?.ProjectileSprite;

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
            sr.sprite = projectileVisual != null ? projectileVisual : RuntimeSpriteUtils.CreateCircleSprite(Color.white);
            sr.sortingOrder = 2;
            go.transform.localScale = projectileVisual != null ? Vector3.one : new Vector3(0.3f, 0.3f, 1f);

            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.freezeRotation = true;

            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;

            var hitSound = _equippedWeapon != null ? _equippedWeapon.hitSound : null;
            go.AddComponent<PlayerProjectile>().Initialize(direction, damage, GetProjectileLifetime(CurrentAmmoType), hitSound, this);
        }

        // Both of these fall back to the pistol's value for WeaponAmmoType.None/unrecognized types,
        // matching the original per-type switch statements' behavior.
        private float GetProjectileLifetime(global::WeaponAmmoType ammoType) =>
            GetAmmoProfile(ammoType)?.ProjectileLifetime ?? pistolProjectileLifetime;

        private float GetFireCooldown(global::WeaponAmmoType ammoType) =>
            GetAmmoProfile(ammoType)?.FireCooldown ?? pistolFireCooldown;

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

        // Internal rather than private - PlayerProjectile (a standalone component, not nested in this
        // class) calls back into this on hit/impact.
        internal void PlaySfx(AudioClip clip, Vector2 position)
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

        private AudioClip GetFireSound(global::WeaponAmmoType ammoType) => GetAmmoProfile(ammoType)?.FireSound;
    }
}
