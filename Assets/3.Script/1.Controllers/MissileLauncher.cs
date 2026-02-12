using UnityEngine;
using UnityEngine.Pool;

public class MissileLauncher : MonoBehaviour
{
    [SerializeField] private MissileController missilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private Transform target;

    private IObjectPool<MissileController> _pool;

    private void Awake()
    {
        _pool = new ObjectPool<MissileController>(
            CreateMissile,
            OnGetMissile,
            OnReleaseMissile,
            OnDestroyMissile,
            maxSize: 20
        );
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            _pool.Get();
        }
    }

    private MissileController CreateMissile()
    {
        MissileController instance = Instantiate(missilePrefab);
        instance.SetPool(_pool);
        return instance;
    }

    private void OnGetMissile(MissileController missile)
    {
        missile.gameObject.SetActive(true);
        missile.transform.position = firePoint.position;
        missile.transform.rotation = firePoint.rotation;
        missile.SetTarget(target);
    }

    private void OnReleaseMissile(MissileController missile)
    {
        missile.gameObject.SetActive(false);
    }

    private void OnDestroyMissile(MissileController missile)
    {
        Destroy(missile.gameObject);
    }
}
