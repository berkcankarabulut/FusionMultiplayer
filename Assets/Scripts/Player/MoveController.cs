using UnityEngine;
using Photon.Pun;

public class MoveController : MonoBehaviour
{
    public float walkSpeed = 4;
    public float sprintSpeed = 8; 
    public float jumpHeight = 4;
    [Space]
    public float airControl = 0.5f;
    
    [Space]
    public CharacterController characterController; // Rigidbody yerine CharacterController kullanılacak
    private Vector2 input;
    
    private bool sprinting = false;
    private bool jumping = false;    
    private bool grounded = false;
    
    private Vector3 moveDirection = Vector3.zero;
    private float verticalVelocity = 0;
    private float gravity = 20f;

    private PhotonView photonView; // Photon için eklendi

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        if (characterController == null)
            characterController = gameObject.AddComponent<CharacterController>();
            
        photonView = GetComponent<PhotonView>();
    }

    private void Update()
    {
        // Sadece lokal oyuncu için girdi al
        if (photonView && !photonView.IsMine) return;
        
        input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        input.Normalize();
        sprinting = Input.GetButton("Sprint");
        jumping = Input.GetButton("Jump");
        
        // Zemin kontrolü
        grounded = characterController.isGrounded;
        
        // Yerçekimi ve zıplama
        if (grounded)
        {
            verticalVelocity = -1f; // Zemine yapışması için küçük bir değer
            
            if (jumping)
            {
                verticalVelocity = Mathf.Sqrt(2f * gravity * jumpHeight);
            }
        }
        else
        {
            verticalVelocity -= gravity * Time.deltaTime;
        }
        
        // Hareket yönünü hesapla
        Vector3 targetDirection = CalculateMovement(sprinting ? sprintSpeed : walkSpeed);
        
        // Havadayken kontrolü azalt
        if (!grounded)
        {
            targetDirection *= airControl;
        }
        
        // Dikey hızı ekle
        targetDirection.y = verticalVelocity;
        
        // CharacterController ile hareket et
        characterController.Move(targetDirection * Time.deltaTime);
        
        // Eğer bu ağ üzerindeki lokal oyuncuysa, pozisyonu senkronize et
        if (photonView && photonView.IsMine && PhotonNetwork.IsConnected)
        {
            if (Time.frameCount % 3 == 0) // Her 3 karede bir pozisyon gönder
            {
                SyncPosition();
            }
        }
    }

    private Vector3 CalculateMovement(float speed)
    {
        Vector3 targetVelocity = new Vector3(input.x, 0, input.y);
        targetVelocity = transform.TransformDirection(targetVelocity);
        targetVelocity *= speed;
        
        return targetVelocity;
    }
    
    private void SyncPosition()
    {
        if (photonView == null) return;
        
        photonView.RPC("NetworkSyncPosition", RpcTarget.Others, transform.position, transform.rotation);
    }
    
    [PunRPC]
    private void NetworkSyncPosition(Vector3 position, Quaternion rotation)
    {
        // Bu RPC sadece diğer oyuncular için çalışır
        if (photonView.IsMine) return;
        
        // Hedef pozisyona yumuşak geçiş yap
        transform.position = Vector3.Lerp(transform.position, position, Time.deltaTime * 10);
        transform.rotation = Quaternion.Lerp(transform.rotation, rotation, Time.deltaTime * 10);
    }
    
    // Artık OnTriggerStay gerekmiyor çünkü CharacterController.isGrounded kullanıyoruz
    // private void OnTriggerStay(Collider other) kaldırıldı
}