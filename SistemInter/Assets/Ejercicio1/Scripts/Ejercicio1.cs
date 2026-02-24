using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;

public class Ejercicio1 : MonoBehaviour
{
    [Header("API")]
    [SerializeField] private string apiurl =
        "https://my-json-server.typicode.com/danibeta33/SisIntDis/users";

    [Header("UI References")]
    [SerializeField] private RawImage[] sprites;
    [SerializeField] private TextMeshProUGUI[] names;
    [SerializeField] private TextMeshProUGUI[] pokedexNums;
    [SerializeField] private TextMeshProUGUI[] type1Texts;
    [SerializeField] private TextMeshProUGUI[] type2Texts;

    [SerializeField] private TextMeshProUGUI userNameText;
    [SerializeField] private TextMeshProUGUI gymText;

    private User[] allUsers;
    private int currentUserIndex = 0;


    void Start()
    {
        StartCoroutine(LoadUsers());
    }

    [System.Serializable]
    public class UserList
    {
        public User[] users;
    }

    [System.Serializable]
    public class User
    {
        public int id;
        public string name;
        public string gym;
        public int[] deck;
    }

    IEnumerator LoadUsers()
    {
    UnityWebRequest request = UnityWebRequest.Get(apiurl);
    yield return request.SendWebRequest();

    if (request.result != UnityWebRequest.Result.Success)
    {
        Debug.LogError(request.error);
        yield break;
    }

    string rawJson = request.downloadHandler.text;
    string wrappedJson = "{ \"users\": " + rawJson + "}";

    UserList data = JsonUtility.FromJson<UserList>(wrappedJson);

    if (data.users == null || data.users.Length == 0)
    {
        Debug.LogError("No se encontraron usuarios");
        yield break;
    }

    allUsers = data.users;

    ShowUser(currentUserIndex);
    }   

    IEnumerator LoadPokemon(int id, int slotIndex)
    {
        string url = "https://pokeapi.co/api/v2/pokemon/" + id;

        UnityWebRequest request = UnityWebRequest.Get(url);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError(request.error);
            yield break;
        }

        PokemonData pokemon = JsonUtility.FromJson<PokemonData>(request.downloadHandler.text);

        if (pokemon.sprites == null || string.IsNullOrEmpty(pokemon.sprites.front_default))
        {
            Debug.LogWarning("Pokemon sin sprite: " + pokemon.name);
            yield break;
        }

        string niceName = char.ToUpper(pokemon.name[0]) + pokemon.name.Substring(1);

        string type1 = pokemon.types[0].type.name;
        string type2 = pokemon.types.Length > 1 ? pokemon.types[1].type.name : "-";

        UnityWebRequest imageRequest =
            UnityWebRequestTexture.GetTexture(pokemon.sprites.front_default);

        yield return imageRequest.SendWebRequest();

        if (imageRequest.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError(imageRequest.error);
            yield break;
        }

        Texture2D tex = DownloadHandlerTexture.GetContent(imageRequest);

        // ✅ ACTUALIZAR UI
        sprites[slotIndex].texture = tex;
        names[slotIndex].text = niceName;
        pokedexNums[slotIndex].text = "#" + pokemon.id;
        type1Texts[slotIndex].text = type1;
        type2Texts[slotIndex].text = type2;

        Debug.Log($"Slot {slotIndex} → {niceName}");
    }

    IEnumerator LoadUserPokemon(User user)
    {
    for (int i = 0; i < user.deck.Length && i < 6; i++)
    {
        yield return StartCoroutine(LoadPokemon(user.deck[i], i));
    }
    }

    public void SiguienteUsuario()
   {
    if (allUsers == null || allUsers.Length == 0) return;

    currentUserIndex++;

    if (currentUserIndex >= allUsers.Length)
        currentUserIndex = 0;

    ShowUser(currentUserIndex);
    }

    public void AnteriorUsuario()
    {
    if (allUsers == null || allUsers.Length == 0) return;

    currentUserIndex--;

    if (currentUserIndex < 0)
        currentUserIndex = allUsers.Length - 1;

    ShowUser(currentUserIndex);
   }

    void ShowUser(int index)
    {
    StopAllCoroutines();

    User user = allUsers[index];

    userNameText.text = user.name;
    gymText.text = user.gym;

    StartCoroutine(LoadUserPokemon(user));
    }

    // ================= JSON CLASSES =================

    [System.Serializable]
    public class PokemonData
    {
        public int id;
        public string name;
        public Sprites sprites;
        public TypeSlot[] types;
    }

    [System.Serializable]
    public class Sprites
    {
        public string front_default;
    }

    [System.Serializable]
    public class TypeSlot
    {
        public TypeInfo type;
    }

    [System.Serializable]
    public class TypeInfo
    {
        public string name;
    }
}