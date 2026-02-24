using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;

public class Test : MonoBehaviour
{
  
  [SerializeField]
  private int characterid = 0;
   [SerializeField]
  private string url = "https://rickandmortyapi.com/api/character";

    [System.Serializable]
    public class UIReferences
    {
        public RawImage CharacterImage;
        public TextMeshProUGUI CharacterName;
        public TextMeshProUGUI CharacterSpecies;
        public TMP_InputField CharacterIDInput;
    }

    [SerializeField] private UIReferences ui;    

  
  //void Start() {
        //StartCoroutine(GetText()); //funciona tambien como StartCoroutine("GetText");
    //}
    public void TomadeDatos() {
        string value = ui.CharacterIDInput.text;
        Debug.Log(value);
        characterid = int.Parse(value);
        StartCoroutine(GetText());
    }
    IEnumerator GetText() {
        UnityWebRequest www = UnityWebRequest.Get(url + "/" + characterid);
        yield return www.Send();
 
        if(www.isNetworkError) {
            Debug.Log(www.error);
            if (www.responseCode == 404) {
                Debug.Log("Character not found");
                ui.CharacterName.text = "Character not found";
                ui.CharacterSpecies.text = "Not found";
            }
        }
        else {
            // Show results as text
           
            Character character = JsonUtility.FromJson<Character>(www.downloadHandler.text);
            Debug.Log(character.name + " is a " + character.species + " and his id is " + character.id);
            ui.CharacterName.text = character.name;
            ui.CharacterSpecies.text = character.species;
            StartCoroutine(GetTexture(character.image));
             
        }
    }

     IEnumerator GetTexture(string imageUrl)
    {
        using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(imageUrl))
        {
            yield return uwr.SendWebRequest();

            if (uwr.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(uwr.error);
            }
            else
            {
                // Get downloaded asset bundle
                var texture = DownloadHandlerTexture.GetContent(uwr);
                ui.CharacterImage.texture = texture;
            }
        }
    }
    class Character
    {
        public int id;
        public string name;
        public string species;
        public string image;

    }
}
