using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;
using UnityEngine.UI;
public class NavMeshAgentBrain : MonoBehaviourPun
{
    public bool ShouldIMove;
    public bool Attacking;
    public GameObject Point;
    public Animator animator;
    public NavMeshAgent navMeshAgent;


    public bool death = false;
    public float health = 1.00f;
    public float damage = 0.01f;
    public float points = 10;


    public string lastHitPlayer;
    // Start is called before the first frame update
    void Awake()
    {
        animator = GetComponent<Animator>();
        navMeshAgent = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {

        
        if (GameObject.Find("GameManager").GetComponent<GameManager>().running)
        {
            if (animator.GetCurrentAnimatorStateInfo(0).IsName("Base Layer.Z_FallingBack") && animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f)
            {
                GetComponent<PhotonView>().RPC("DestroyZombie", RpcTarget.All);

            }

            if (ShouldIMove && !death)
            { 
                if (Point.transform.gameObject.activeSelf)
                {

                    GetComponent<NavMeshAgent>().SetDestination(Point.transform.position);
                }
                else
                {
                    stop();
                }
                
            }

        }

     
    }

    [PunRPC]
    public void ReBirth(Vector3 newPosition)
    {
        transform.position = newPosition;
        health = 1;
        death = false;
        ShouldIMove = false;
        transform.gameObject.SetActive(true);
    }

 
    public void move()
    {
            if (animator == null)
            {
                animator = GetComponent<Animator>();
            }

            if (navMeshAgent == null)
            {
                navMeshAgent = GetComponent<NavMeshAgent>();
            }
            animator.SetBool("run", true);
            animator.SetBool("attack", false);
            ShouldIMove = true;
            navMeshAgent.isStopped = false;
            Attacking = false;
        
    }


    
    public void stop()
    {
            print("STOPED");
            if (animator == null)
            {
                animator = GetComponent<Animator>();
            }

            if (navMeshAgent == null)
            {
                navMeshAgent = GetComponent<NavMeshAgent>();
            }
            animator.SetBool("run", false);
            animator.SetBool("attack", false);
            ShouldIMove = false;
            navMeshAgent.isStopped = true;
            navMeshAgent.velocity = Vector3.zero;
            Attacking = false;
        
    }


    [PunRPC]
    public void GetDamageZombie(float amount, string username)
    {
        if (GetComponent<PhotonView>().IsMine)
        {
            if (!death)
            {
                health -= amount;


                if (health <= 0)
                {
                    GetPlayer(username);
                    Death();
                }
            }

        }


        /*if (photonView.IsMine)
        {
            fillImage.fillAmount = playerHealth;
        }*/
    }

    public void Death()
    {
        navMeshAgent.velocity = Vector3.zero;
        navMeshAgent.isStopped = true;
        ShouldIMove = false;
        death = true;
        animator.SetBool("run", false);
        animator.SetBool("attack", false);
        animator.SetTrigger("dead");

    }


    [PunRPC]
    public void DestroyZombie()
    {
        GetComponent<PhotonView>().transform.gameObject.SetActive(false);
        if (PhotonNetwork.IsMasterClient)
        {
            GameObject.Find("PoolManager").GetComponent<PoolManager>().zombieList.Enqueue(transform.gameObject);
        }
           
    }

    GameObject GetPlayer(string namePlayer)
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        
        foreach (GameObject player in players)
        {
            print(namePlayer);
            print(points);
            player.GetComponentInParent<MyPlayer>().teamPoints += this.points;
            if (player.GetComponentInParent<PhotonView>().Owner.NickName == namePlayer)
            { 
                player.GetComponentInParent<MyPlayer>().points += this.points;
            }
        }
        return null;
    }


    public void DamagePlayer()
    {
        if (GameObject.Find("GameManager").GetComponent<GameManager>().running)
        {
            Point.GetComponentInParent<PhotonView>().RPC("GetDamage", RpcTarget.All, damage, photonView.ViewID);
        }
        
    }

    
    public void ReachTarget()
    {
        
            if (animator == null)
            {
                animator = GetComponent<Animator>();
            }

            if (navMeshAgent == null)
            {
                navMeshAgent = GetComponent<NavMeshAgent>();
            }
            transform.LookAt(Point.transform);
            ShouldIMove = false;
            navMeshAgent.isStopped = true;
            navMeshAgent.velocity = Vector3.zero;
            animator.SetBool("run", false);
            animator.SetBool("attack", true);
            Attacking = true;
        
     
    }

    [PunRPC]
    public void StopZombieFromPlayer()
    {
        stop();
    }



}
