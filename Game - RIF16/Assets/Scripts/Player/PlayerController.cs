﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class PlayerController : PhisicObject {

    public float maxSpeed = 7;

    [Header("Jumping")]
    public float jumpTakeOffSpeed = 7;

    [Header("Health")]
    public int maxHealth = 5;
    public static int curHealth;
    private DateTime lastRun;
    public bool playerHurt;
    public Transform spawningPoint;
    public GameObject healthUIParticles;
    //public Animation healthUIParticlesAnim;
    private ParticleSystem healthUIParticleSys;

    [Header("Effects")]
    public AudioSource runAudioSource;
    public AudioSource jumpAudioSource;
    public AudioSource hurtAudioSource;

    private SpriteRenderer spriteRenderer;
    private CapsuleCollider2D playerCollider;
    private Animator animator;
    private bool canMove = true;
    private bool jump;


    /* Initialization */
    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        playerCollider = GetComponent<CapsuleCollider2D>();
        healthUIParticleSys = healthUIParticles.GetComponent<ParticleSystem>();

        if (PlayerPrefs.HasKey("health")) {
            curHealth = PlayerPrefs.GetInt("health");
            if (curHealth == 0) {
                curHealth = maxHealth;
            }

            if (curHealth < 3) {
                var main = healthUIParticleSys.main;
                main.simulationSpeed = 1;
            }
        } else {
            curHealth = maxHealth;
        }
    }


    /* Player Movement */
    protected override void ComputeVelocity()
    {
        if (playerHurt)
        {
            canMove = false;
        }

        Vector2 move = Vector2.zero;

        move.x = canMove ? Input.GetAxisRaw("Horizontal") : 0;

        if (Input.GetButtonDown("Jump") && grounded && canMove)
        {
            velocity.y = jumpTakeOffSpeed;
            jump = true;
            playJumpSound();
        }
        else if (Input.GetButtonUp("Jump") && canMove)
        {
            if (velocity.y > 0)
            {
                velocity.y = velocity.y * 0.5f;
            }
            jump = false;
        }

        if (move.x > 0.01f)
        {   
            spriteRenderer.flipX = false;
        }
        else if (move.x < -0.01f)
        {   
            spriteRenderer.flipX = true;
        }

        animator.SetBool("grounded", grounded);
        animator.SetBool("playerHurt", playerHurt);
        animator.SetFloat("velocityX", Mathf.Abs(velocity.x) / maxSpeed);

        // Set Velocity Directly
        targetVelocity = move * maxSpeed;
    }


    /* Player Sounds */
    public void playJumpSound()
    {
        if (jump)
        {
            jumpAudioSource.Stop();
            jumpAudioSource.Play();
        }
    }

    public void playRunSound()
    {
        runAudioSource.pitch = 1 + UnityEngine.Random.Range(-0.2f, 0.2f);
        runAudioSource.Play();
    }

    public void playHurtSound()
    {
        hurtAudioSource.Play();
    }

    /* Player Collisions and Triggers*/
    void OnCollisionEnter2D(Collision2D other) {
        if (other.transform.tag == "MovingPlatform")
        {
            transform.parent = other.transform;
        }
    }

    void OnCollisionExit2D(Collision2D other) {
        if (other.transform.tag == "MovingPlatform")
        {
            transform.parent = null;
        }
    }

    void OnTriggerEnter2D(Collider2D other) {
        if (other.CompareTag("Enemy"))
        {
            playerCollider.enabled = false;
            playerHurt = true;
        }
        if (other.CompareTag("Portal"))
        {
            canMove = false;
        }
    }

    void OnTriggerExit2D(Collider2D other) {
        if (other.CompareTag("LevelBorder"))
        {
            playerHurt = true;
        }
    }

    public void TeleportAhead() {
        if(SceneManager.GetActiveScene().buildIndex == 7) {
            this.transform.position = new Vector2(123.76f, -9.02f);
        }
    }

    public void Die() {
        if (lastRun.AddSeconds(0.5) < DateTime.Now) {

            lastRun = DateTime.Now;

            curHealth--;
            PlayerPrefs.SetInt("health", curHealth);

            int ActiveSceneIndex = SceneManager.GetActiveScene().buildIndex;

            if (ActiveSceneIndex > 1) {
                if (curHealth == 0) {
                    PlayerPrefs.SetInt("Downgraded", 1);
                    PlayerPrefs.SetInt("sceneIndex", ActiveSceneIndex - 1);
                    SceneManager.LoadSceneAsync(ActiveSceneIndex - 1);
                } else {
                    SceneManager.LoadSceneAsync(ActiveSceneIndex);
                }
            } else {
                SceneManager.LoadSceneAsync(ActiveSceneIndex);
            }
        }
    }
}