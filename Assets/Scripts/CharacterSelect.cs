using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using Mirror.Examples.NetworkRoom;

public class CharacterSelect : NetworkBehaviour
{
    public static CharacterSelect instance;

    [SerializeField] private GameObject characterSelectDisplay = default;
    [SerializeField] private Transform characterPreviewParent = default;
    [SerializeField] private TextMesh characterNameText = default;
    [SerializeField] private float turnSpeed = 90f;

    private int currentCharacterIndex = 0;
    private List<GameObject> characterInstances = new List<GameObject>();

    private NetworkRoomManagerExt room;
    private NetworkRoomManagerExt Room
    {
        get
        {
            if (room != null) { return room; }
            return room = NetworkManager.singleton as NetworkRoomManagerExt;
        }
    }

    private void Start()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }


    public override void OnStartClient()
    {
       
        if (characterPreviewParent.childCount == 0)
        {
            foreach (var character in Room.characters)
            {
                GameObject characterInstance =
                    Instantiate(character.CharacterPreviewPrefab, characterPreviewParent);

                characterInstance.SetActive(false);

                characterInstances.Add(characterInstance);
            }
        }

        characterInstances[currentCharacterIndex].SetActive(true);
        characterNameText.text = Room.characters[currentCharacterIndex].CharacterName;

        characterSelectDisplay.SetActive(true);
        

        Select();
    }

    public override void OnStopClient()
    {
        base.OnStopClient();

        characterSelectDisplay.SetActive(false);
    }

    void Select()
    {
        CmdSelect(currentCharacterIndex);
    }

    [Command(requiresAuthority = false)]
    public void CmdSelect(int _currentCharacterIndex , NetworkConnectionToClient sender = null)
    {
        sender.identity.GetComponent<NetworkRoomPlayer>().Character = _currentCharacterIndex;
    }

    private void Update()
    {
        characterPreviewParent.RotateAround(
            characterPreviewParent.position,
            characterPreviewParent.up,
            turnSpeed * Time.deltaTime);
    }

    public void Right()
    {
        characterInstances[currentCharacterIndex].SetActive(false);

        currentCharacterIndex = (currentCharacterIndex + 1) % characterInstances.Count;

        characterInstances[currentCharacterIndex].SetActive(true);
        characterNameText.text = Room.characters[currentCharacterIndex].CharacterName;

        Select();
    }

    public void Left()
    {
        characterInstances[currentCharacterIndex].SetActive(false);

        currentCharacterIndex--;
        if (currentCharacterIndex < 0)
        {
            currentCharacterIndex += characterInstances.Count;
        }

        characterInstances[currentCharacterIndex].SetActive(true);
        characterNameText.text = Room.characters[currentCharacterIndex].CharacterName;

        Select();
    }
}