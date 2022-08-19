using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class CharacterSelect : NetworkBehaviour
{
    public static CharacterSelect instance;

    [SerializeField] private GameObject characterSelectDisplay = default;
    [SerializeField] private Transform characterPreviewParent = default;
    [SerializeField] private TextMesh characterNameText = default;
    [SerializeField] private float turnSpeed = 90f;
    [SerializeField] private Character[] characters = default;

    private int currentCharacterIndex = 0;
    private List<GameObject> characterInstances = new List<GameObject>();

    private NetworkRoomManager room;
    private NetworkRoomManager Room
    {
        get
        {
            if (room != null) { return room; }
            return room = NetworkManager.singleton as NetworkRoomManager;
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
            foreach (var character in characters)
            {
                GameObject characterInstance =
                    Instantiate(character.CharacterPreviewPrefab, characterPreviewParent);

                characterInstance.SetActive(false);

                characterInstances.Add(characterInstance);
            }
        }

        characterInstances[currentCharacterIndex].SetActive(true);
        characterNameText.text = characters[currentCharacterIndex].CharacterName;

        characterSelectDisplay.SetActive(true);

        Select();
    }

    public void Select()
    {
        Room.myCharacterIndex = currentCharacterIndex;
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
        characterNameText.text = characters[currentCharacterIndex].CharacterName;

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
        characterNameText.text = characters[currentCharacterIndex].CharacterName;

        Select();
    }
}