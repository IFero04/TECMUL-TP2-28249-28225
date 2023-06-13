using UnityEngine;
using static scr_Models;
using UnityEngine.AI;
using System.Collections;

public class scr_Enemy : MonoBehaviour
{
    public EnemySettingsModel settings;
    public GameObject deadEffect;
    public NavMeshAgent navMeshAgent;
    public Animator animator;
    public AudioSource audioSource;
    public AudioClip zombieDamaged;
    public AudioClip zombieAttack;
    scr_WaveSpawner spawner;

    private Transform target;

    private void Start()
    {
        target = GameObject.FindGameObjectWithTag("Player").transform;
        
    }
    private void Update()
    {
        if (navMeshAgent)
        {
            NavMeshAgente();
        }
    }
    
    private void NavMeshAgente()
    {
        float _distance = Vector3.Distance(transform.position, target.position);


        if (_distance < 50)
        {
            animator.SetTrigger("Run");
            if (_distance > 2)
            {
                navMeshAgent.isStopped = false;
                navMeshAgent.SetDestination(target.position);
            }
            else
            {
                navMeshAgent.isStopped = true;
                //Attack
                if (settings.attackCoolDown <= 0)
                {
                    animator.SetTrigger("Attack");
                    audioSource.PlayOneShot(zombieAttack);
                    settings.attackCoolDown = 3;
                    StartCoroutine(CheckHit());
                }

                Vector3 _dir = (target.position - transform.position).normalized;
                Quaternion _lookAngle = Quaternion.LookRotation(new Vector3(_dir.x, 0, _dir.z));
                transform.rotation = Quaternion.Slerp(transform.rotation, _lookAngle, Time.deltaTime * 10);

            }
        }
        else
        {
            navMeshAgent.isStopped = true;
        }



        if (settings.attackCoolDown > 0)
            settings.attackCoolDown -= Time.deltaTime;
    }

    IEnumerator CheckHit()
    {
        yield return new WaitForSeconds(0.75f);
        float _distance = Vector3.Distance(transform.position, target.position);
        if (_distance <= 2)
        {
            target.GetComponent<scr_PlayerHealth>().CallTakeDamage(10);
        }
    }

    public void TakeDamege(float amount)
    {
        settings.health -= amount;

        if(settings.health <= 0f)
        {
            audioSource.PlayOneShot(zombieDamaged);
            Die();
        }
    }

    private void Die()
    {
        GameObject _effect = Instantiate(deadEffect, transform.position + new Vector3(0, 2, 0), Quaternion.identity);
        Destroy(_effect, 3f);

        scr_MenuHandler _menu = FindAnyObjectByType<scr_MenuHandler>();
        if (_menu)
        {
            _menu.score += 10;
        }

        if (spawner != null)
        {
            spawner.currentWave.Remove(this.gameObject);
        }
        
        Destroy(gameObject);
    }

    public void SetSpawner(scr_WaveSpawner _spawner)
    {
        spawner = _spawner;
    }
}
