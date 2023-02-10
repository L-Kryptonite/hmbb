using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    private Rigidbody2D rb;
    private Animator anim;
    public Collider2D coll;
    public Collider2D DisColl;
    public Transform CellingCheck,GroundCheck;
    //public AudioSource jumpAudio,hurtAudio,smAudio;
    [Space]
    public float speed;
    public float JumpForce;
    [Space]
    public float climbSpeed;
    public LayerMask ground;
    [SerializeField]
    public int sm;
    public Text BlueNum;
    private bool isHurt;//Ĭ����false
    private bool isGround;
    private int extraJump;
    private bool isLadder;
    private bool isClimbing;
    private bool isJumping;
    private bool isFalling;
    private float playerGravity;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        playerGravity = rb.gravityScale;
    }

    
    void FixedUpdate()
    {
        if (!isHurt)
        {
            Movement();
        }
       
        SwitchAnim();
        isGround = Physics2D.OverlapCircle(GroundCheck.position,0.2f,ground);
    }

    private void Update()
    {
        //Jump();
        Crouch();
        BlueNum.text = sm.ToString();
        newJump();
        CheckLadder();
        Climb();
        CheckAirStatus();
    }



    void Movement()//�ƶ�
    {
        float horizontalMove = Input.GetAxis("Horizontal");
        float facedircetion = Input.GetAxisRaw("Horizontal");

        //��ɫ�ƶ�
        if (horizontalMove !=0)
        {
            rb.velocity = new Vector2(horizontalMove * speed * Time.fixedDeltaTime, rb.velocity.y);
            anim.SetFloat("running", Mathf.Abs(facedircetion));
        }
        if (facedircetion != 0)
        {
            transform.localScale = new Vector3(facedircetion, 1, 1);
        }
        
        
       
        
    }
    //�л�����

    void SwitchAnim()
    {
        //anim.SetBool("idle", false);
        if(rb.velocity.y < 0.1f && !coll.IsTouchingLayers(ground))
        {
            anim.SetBool("falling", true);
        }
        if (anim.GetBool("jumping"))
        {
            if (rb.velocity.y<0)
            {
                anim.SetBool("jumping", false);
                anim.SetBool("falling", true);
            }
        }else if (isHurt)
        {
            anim.SetBool("hurt", true);
            anim.SetFloat("running", 0);
            if (Mathf.Abs(rb.velocity.x) < 0.1f)
            {
                anim.SetBool("hurt", false);
                //anim.SetBool("idle", true);
                isHurt = false;
            }
        }
        else if (coll.IsTouchingLayers(ground))
        {
            anim.SetBool("falling", false);
            //anim.SetBool("idle", true);
        }
    }
    
    //��ײ������
    private void OnTriggerEnter2D(Collider2D collision)
    {
        //�ռ���Ʒ
        if (collision.tag == "Collection")
        {
            SoundManager.instance.SmAudio();
            //Destroy(collision.gameObject);
            //sm += 1;
            collision.GetComponent<Animator>().Play("isGot");
            //BlueNum.text = sm.ToString();
        }
       if (collision.tag == "DeadLine")
        {

            Invoke("Restart",2f);
            
        }
        
    }
    //�������
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Enemy")
         {
            Enemy enemy = collision.gameObject.GetComponent<Enemy>();
               if (anim.GetBool("falling"))
            {
                enemy.JumpOn();
                
                rb.velocity = new Vector2(rb.velocity.x, JumpForce * Time.deltaTime);
                anim.SetBool("jumping", true);
            //����
            }else if (transform.position.x<collision.gameObject.transform.position.x)
            {
                rb.velocity = new Vector2(-10, rb.velocity.y);
                //hurtAudio.Play();
                SoundManager.instance.HurtAudio();
                isHurt = true;
            }
            else if (transform.position.x > collision.gameObject.transform.position.x)
            {
                rb.velocity = new Vector2(10, rb.velocity.y);
                SoundManager.instance.HurtAudio();
                isHurt = true;
            }
        }
   }

    void Crouch()
    {
        if (!Physics2D.OverlapCircle(CellingCheck.position,0.2f,ground)) { 
        if (Input.GetButtonDown("Crouch"))
        {
            anim.SetBool("crouching", true);
            DisColl.enabled = false;
        }
        else
        {
            anim.SetBool("crouching", false);
            DisColl.enabled = true;
        }
     }
    }

    /* void Jump()
     {
         if (Input.GetButtonDown("Jump") && coll.IsTouchingLayers(ground))
         {
             rb.velocity = new Vector2(rb.velocity.x, JumpForce * Time.fixedDeltaTime);
             jumpAudio.Play();
             anim.SetBool("jumping", true);
         }
     }*/

    //��ɫ��Ծ
    void newJump()
    {
        if (isGround)
        {
            extraJump = 1;
        }
        if (Input.GetButtonDown("Jump") && extraJump >0)
        {
            rb.velocity = Vector2.up * JumpForce;
            extraJump--;
            SoundManager.instance.JumpAudio();
            anim.SetBool("jumping", true);
        }
        if (Input.GetButtonDown("Jump") && extraJump == 0 && isGround)
        {
            rb.velocity = Vector2.up * JumpForce;
            SoundManager.instance.JumpAudio();
            anim.SetBool("jumping", true);
        }
    }


    void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
        
    

    public void SmCount()
    {
        sm += 1;
    }

    //������
    void CheckLadder()
    {
        isLadder = rb.IsTouchingLayers(LayerMask.GetMask("Ladder"));
    }

    void Climb()
    {
        if (isLadder)
        {
            float moveY = Input.GetAxis("Vertical");
            if (moveY >0.5f || moveY <-0.5f)
            {
                anim.SetBool("Climbing", true);
                rb.gravityScale = 0.0f;
                rb.velocity = new Vector2(rb.velocity.x, moveY * climbSpeed);
            }
            else
            {
                if (isJumping || isFalling)
                {
                    anim.SetBool("Climbing", false);
                }
                else
                {
                    anim.SetBool("Climbing", false);
                    rb.velocity = new Vector2(rb.velocity.x, 0.0f);
                }
            }
        }
        else
        {
            anim.SetBool("Climbing", false);
            rb.gravityScale = playerGravity;
        }
    }

    void CheckAirStatus()
    {
        isJumping = anim.GetBool("jumping");
        isFalling = anim.GetBool("falling");
        isClimbing = anim.GetBool("Climbing");
    }
}
