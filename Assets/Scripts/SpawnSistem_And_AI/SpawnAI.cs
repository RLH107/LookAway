using System.Collections;
using UnityEngine;

public class SpawnAI : MonoBehaviour
{
    [SerializeField] private GameObject to_spawn;
    [SerializeField] private LayerMask MTerrain;
    [SerializeField] private int SAreaRadMax;
    [SerializeField] private int SAreaRadMin;
    [SerializeField] private int YAxisSArea;
    [SerializeField] private int MaxNumOfEntetys;
    [SerializeField] private float DistanceOfGround;
    [SerializeField] private float SCooldown;

    //[SerializeField] int N_of_Enemys_to_Spawn = 10;

    private Transform TPlayer;
    private Vector3 SP;
    private LayerMask Default;
    private float SC;
    private int Xpos;
    private int Ypos;
    private int Zpos;

    public bool Night = false;
    [HideInInspector] public int PublicNumberOfEntetys;

    void Start()
    {
        SC = SCooldown;
        PublicNumberOfEntetys = MaxNumOfEntetys;
        TPlayer = GameObject.FindGameObjectWithTag("Player").transform;
        Default = LayerMask.GetMask("Default");
    }

    private void Update()
    {
        transform.position = TPlayer.position;

        if (Night == true && SC > 0)
        {
            SC -= Time.deltaTime;
        }
        if (SC <= 0 && PublicNumberOfEntetys > 0)
        {
            StartCoroutine("Spawn");
        }
    }

    public void AddToSpawner(int NumberToAdd)
    {
        PublicNumberOfEntetys += NumberToAdd;
    }


    private IEnumerator Spawn()
    {
        Xpos = Random.Range(-SAreaRadMax, SAreaRadMax);
        Ypos = Random.Range(-YAxisSArea, YAxisSArea);
        Zpos = Random.Range(-SAreaRadMax, SAreaRadMax);
        SP = new Vector3(transform.position.x + Xpos, transform.position.y + Ypos, transform.position.z + Zpos);

        if (Physics.Raycast(SP, -transform.up, DistanceOfGround, MTerrain) && !Physics.Raycast(SP, -transform.up, DistanceOfGround, Default) && Vector3.Distance(transform.position, SP) >= SAreaRadMin && Vector3.Distance(transform.position, SP) <= SAreaRadMax)
        {
            Instantiate(to_spawn, SP, Quaternion.identity);
            PublicNumberOfEntetys -= 1;
            SC = SCooldown;
        }
        yield return null;

    }

}
