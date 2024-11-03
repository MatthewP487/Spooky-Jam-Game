using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class plyrMov : MonoBehaviour
{
    // Start is called before the first frame update

    [Header("Rigidbody")]
    public Rigidbody2D theRB;

    [Header("Editable-Player-Stuff")]
    public float plyrMovSpd;
    public float accelRate;
    public float plyrAccel;
    public float accelMax;
    public float jumpForce;
    public float fallForce;
    public float distanceToWall;
    float horizontalInput;
    float verticalInput;
    public int atBase;

    [Header("Grounded-Stuff")]
    public bool isGrounded;
    public Transform groundCheck;
    public Transform playerSprite;
    public float groundCheckRadius;
    public LayerMask groundLayer;

    [Header("Line Stuff")]
    public bool isSliding, canSlide;
    public LayerMask iceLayer;
    public LineRenderer lineRenderer;
    public int currentPointIndex = 0;
    public int iceSpeed;

    [Header("Animation Stuff")]
    public Animator animator;
    public SpriteRenderer spriteRenderer;
    public bool left = false;


    void Start()
    {
        theRB = GetComponent<Rigidbody2D>();
        theRB.freezeRotation = true;
        plyrAccel = 1f;
        atBase = 0;
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        //Setup Stuff.
        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");
        theRB.velocity = new Vector2(horizontalInput * (plyrMovSpd * plyrAccel), theRB.velocity.y);
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (horizontalInput > 0 && plyrAccel < accelMax)
        {
            plyrAccel += accelRate * Time.deltaTime;
        }
        if (horizontalInput <=0 && plyrAccel >= 1f)
        {
            plyrAccel -= accelRate * Time.deltaTime;
        }

        // Make sure grounded and not sliding to jump
        if (Input.GetKeyDown(KeyCode.Space) && (isGrounded || isSliding))
        {
            Debug.Log("This is working");
            Jump();
            animator.SetBool("isJumping", true);
        }

        // Check for fast fall
        if (verticalInput < 0  && (!isGrounded || !isSliding))
        {
            FastFall();
        }

        // Changes the sprite from left to right
        if (theRB.velocity.x < 0)
            left = true;
        else if (theRB.velocity.x > 0)
            left = false;

        spriteRenderer.flipX = left;

        // If sliding activate the animation
        if (isSliding)
            animator.SetBool("isSliding", true);

        animator.SetFloat("xVelocity", Mathf.Abs(theRB.velocity.x));
        animator.SetFloat("yVelocity", theRB.velocity.y);

    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Ice"))
        {
            Debug.Log("You're on Ice!");
            lineRenderer = other.GetComponent<LineRenderer>();
            currentPointIndex = GetClosestIndex();

            if (lineRenderer != null)
            {
                Debug.Log("Found a LineRenderer on" + other.gameObject.name);
            }
        }

        isGrounded = true;
        animator.SetBool("isJumping", false);
        animator.SetBool("isSliding", false);

    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Ice"))
        {
            if (currentPointIndex >= lineRenderer.positionCount)
            {
                return;
            }
            Vector2 targetPosition = lineRenderer.GetPosition(currentPointIndex);
            Debug.Log("Target position is " + targetPosition);
            Vector2 direction = (targetPosition - theRB.position).normalized;
            Debug.Log("Direction is " + direction);
            theRB.velocity = new Vector2(theRB.velocity.x + (direction.x * iceSpeed), theRB.velocity.y + (direction.y * iceSpeed));
            float distance = Vector2.Distance(theRB.position, targetPosition);
            if (distance < 0.1f || (distance > - 0.1f))
            {
                currentPointIndex++;
            }

            
        }
    }

    public void FastFall()
    {
        Debug.Log("Fall!");
        theRB.velocity = new Vector2(theRB.velocity.x, theRB.velocity.y - fallForce);
    }
    public void Jump()
    {
        theRB.velocity = new Vector2(theRB.velocity.x, theRB.velocity.y + jumpForce);
    }

    public int GetClosestIndex()
    {
        int closestIndex = -1;
        float closestDistance = Mathf.Infinity;

        for (int i = 0; i < lineRenderer.positionCount; i++)
        {
            Vector2 pointPosition = lineRenderer.GetPosition(i);
            float distance = Vector2.Distance(playerSprite.position, pointPosition);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestIndex = i;
            }
        }

        Debug.Log("Closest index is " + closestIndex);

        return closestIndex;
    }

}
