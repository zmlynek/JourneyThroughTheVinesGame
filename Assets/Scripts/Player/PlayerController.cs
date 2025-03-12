using System;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    private bool isMoving;
    public Transform movePoint;
    public Transform playerCheckPoint;
    public LayerMask whatStopsMovement;
    public LayerMask enemies;
    public LayerMask npc;
    public LayerMask cutscene;

    public event Action<Collider2D> OnEncounter;
    public event Action<Collider2D> OnCutscene;

    private Animator animator;

    public Vector3 LastTileVisited {  get; set; }

    [SerializeField] Character playerChar;

    public Character PlayerChar { get { return playerChar; } }

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private bool isWalkable(Vector3 targetPos)
    {
        if (Physics2D.OverlapCircle(targetPos, 0.1f, whatStopsMovement | npc ) != null || Physics2D.OverlapCircle(targetPos, 0.2f, enemies) != null)
        {
            return false;
        }
        return true;
    }

    private void Start()
    {
        movePoint.parent = null; //detach so it moves seperately from player
        playerCheckPoint.parent = null; //same as above

        //intial tile positions for companion to follow
        LastTileVisited = new Vector3(transform.position.x, transform.position.y - 1, transform.position.z);
    }

    void CheckMoveInputs() //Check move inputs and interact inputs as well 
    {
        isMoving = true; //used more for mutual exclusion

        if (Mathf.Abs(Input.GetAxisRaw("Horizontal")) == 1f)
        {
            animator.SetFloat("moveX", Input.GetAxisRaw("Horizontal"));
            animator.SetFloat("moveY", 0f);
            if (isWalkable(transform.position + new Vector3(Input.GetAxisRaw("Horizontal"), 0f, 0f)))
            {
                animator.SetBool("isMoving", isMoving); //only set animator if actually moving
                LastTileVisited = transform.position;
                movePoint.position += new Vector3(Input.GetAxisRaw("Horizontal"), 0f, 0f);
            }
        }
        else if (Mathf.Abs(Input.GetAxisRaw("Vertical")) == 1f)
        {
            animator.SetFloat("moveX", 0f);
            animator.SetFloat("moveY", Input.GetAxisRaw("Vertical"));
            if (isWalkable(transform.position + new Vector3(0f, Input.GetAxisRaw("Vertical"), 0f)))
            {
                animator.SetBool("isMoving", isMoving); //only set animator if actually moving
                LastTileVisited = transform.position;
                movePoint.position += new Vector3(0f, Input.GetAxisRaw("Vertical"), 0f);
            }
        }

        if (Input.GetKeyDown(KeyCode.Z))
        {
            Interact();
        }

        if (Input.GetAxisRaw("Horizontal") == 0 && Input.GetAxisRaw("Vertical") == 0)
        {
            isMoving = false; animator.SetBool("isMoving", isMoving);
        }
    }

    public void HandleUpdateFreeRoam() //handles player movement, collisions and battle encounters as well as companion position in free roam
    {
        transform.position = Vector3.MoveTowards(transform.position, movePoint.position, moveSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, movePoint.position) <= .05f) //if not moving check for move inputs
        {
            CheckMoveInputs();
            CheckForEnemies(); //check while not moving 
            CheckForCutscene();
        }
        else //while moving
        {
            CheckForCutscene(); //Check for cutscene while moving as well
            CheckForEnemies(); //Check for encounters while moving as well
        }
    }

    public void HandleUpdateBattle() //handles player movement and battle position of companion
    {
        transform.position = Vector3.MoveTowards(transform.position, movePoint.position, moveSpeed * Time.deltaTime);
        
        //Allow player to move during battle without checking for battles, interactions and cutscenes
        if (Vector3.Distance(transform.position, movePoint.position) <= .05f)
            CheckMoveInputs();
    }

    public void Interact() //allows player to interact on z press and if there is an npc in the facing dir;
    {
        var facingDir = new Vector3(animator.GetFloat("moveX"), animator.GetFloat("moveY"));
        var interactPos = transform.position + facingDir;

        //interact if there is an npc there
        var collider = Physics2D.OverlapCircle(interactPos, .2f, npc);
        if (collider != null) //Is interactable object
        {
            collider.GetComponent<Interactable>()?.Interact();
        }
    }

    private void CheckForEnemies()
    {
        var collider = Physics2D.OverlapCircle(transform.position, 0.7f, enemies);
        if (collider != null)
        {
            animator.SetBool("isMoving", false);
            OnEncounter?.Invoke(collider);
        }
    }

    private void CheckForCutscene()
    {
        var collider = Physics2D.OverlapCircle(transform.position, 0.2f, cutscene);
        if (collider != null)
        {
            animator.SetBool("isMoving", false);
            OnCutscene?.Invoke(collider);
        }
    }
}
