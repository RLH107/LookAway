using System.Collections;
using UnityEngine;

public class IAGround : MonoBehaviour
{
    private GameObject TargetObject;
    [SerializeField] private float Movementspeed = 1.0f;
    [SerializeField] private float turningSpeed = 5.0f;
    [SerializeField] private float AgroDistance = 10.0f;
    [SerializeField] private float AttackDistance = 1f;
    private float DistanceBeforeDistroy = 200f;

    SpawnAI SpawnAI_Script;

    public enum STATE
    {
        IDLE,
        MT_TARGET,
        ATTACK,
        DEAD,
    }

    public STATE state;

    void Start()
    {
        TargetObject = GameObject.FindGameObjectWithTag("Player");
        SpawnAI_Script = GameObject.FindGameObjectWithTag("SpawnSis").GetComponent<SpawnAI>();
        StateSwitch(STATE.IDLE);
    }

    private void Update()
    {
        if (AgroDistance < AttackDistance)
        {
            AgroDistance = 15.0f;
            AttackDistance = 0.8f;
        }
        if (DistanceBeforeDistroy <= AgroDistance)
        {
            DistanceBeforeDistroy = AgroDistance + 50f;
        }
        if (Vector3.Distance(transform.transform.position, TargetObject.transform.position) > DistanceBeforeDistroy)
        {
            Dead();
        }
    }

    public void Dead()
    {
        StopAllCoroutines();
        StateSwitch(STATE.DEAD);
    }

    void StateSwitch(STATE STate)
    {
        state = STate;
        switch (state)
        {
            case STATE.IDLE:
                StartCoroutine("IDLE");
                break;
            case STATE.MT_TARGET:
                StartCoroutine("MT_TARGET");
                break;
            case STATE.ATTACK:
                StartCoroutine("ATTACK");
                break;
            case STATE.DEAD:
                StartCoroutine("DEAD");
                break;
        }
    }

    private IEnumerator IDLE()
    {
        yield return null;
        if (Vector3.Distance(transform.transform.position, TargetObject.transform.position) < AgroDistance)
        {
            StateSwitch(STATE.MT_TARGET);
        }
        else
        {
            StateSwitch(STATE.IDLE);
        }

    }
    private IEnumerator MT_TARGET()
    {
        float distance = Vector3.Distance(transform.transform.position, TargetObject.transform.position);
        var step = Movementspeed * Time.deltaTime;
        Vector3 direction = TargetObject.transform.position - transform.position;
        Quaternion rotation = Quaternion.LookRotation(direction);
        transform.position = Vector3.MoveTowards(transform.position, TargetObject.transform.position, step);
        transform.rotation = Quaternion.Lerp(transform.rotation, rotation, turningSpeed * Time.deltaTime);

        yield return null;

        if (distance < AttackDistance)
        {
            StateSwitch(STATE.ATTACK);
        }
        else

        if (distance > AgroDistance)
        {
            StateSwitch(STATE.IDLE);
        }
        else
        {
            StateSwitch(STATE.MT_TARGET);
        }
    }

    private IEnumerator ATTACK()
    {
        yield return null;
        if (Vector3.Distance(transform.transform.position, TargetObject.transform.position) < AttackDistance)
        {
            StateSwitch(STATE.ATTACK);
        }
        else
        {
            StateSwitch(STATE.MT_TARGET);
        }

    }

    private IEnumerator DEAD()
    {
        SpawnAI_Script.AddToSpawner(1);
        Destroy(gameObject);
        yield return null;
    }
}
