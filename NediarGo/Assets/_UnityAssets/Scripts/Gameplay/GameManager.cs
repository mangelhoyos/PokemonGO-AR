using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Manages all the events and behaviour of all the game in general
/// </summary>
public class GameManager : MonoBehaviour
{
    [Header("Player prefs")]
    [SerializeField] TMP_Text userName;
    [SerializeField] private int escapeChance;
    

    [Header("Pokemon")]
    [SerializeField] private PokemonData pokemonObject;
    [SerializeField] private MeshRenderer pokemonRenderer;
    [SerializeField] private TMP_Text pokemonName;
    [SerializeField] TMP_Text pokemonStatus;
    private List<int> excludedPokemonIDs = new List<int>();


    void Awake()
    {
        GetAuthenticationData();
        GetPokemonFromAPICall();
        pokemonStatus.CrossFadeAlpha(0, 0.1f, true);
    }

    /// <summary>
    /// Gets player username
    /// </summary>
    void GetAuthenticationData()
    {
        AuthenticationManager auth = GameObject.FindObjectOfType<AuthenticationManager>();
        Debug.Log(auth);
        if(auth != null)
        {
            userName.text = (auth.GetUserData().Email.Split('@'))[0];
            Debug.Log(auth.GetUserData().DisplayName);
        }
    }

    /// <summary>
    /// Gets a random pokemon from the API and puts the texture and name inside the pokemon box
    /// </summary>
    void GetPokemonFromAPICall()
    {
        StartCoroutine(GetPokemonFromAPI());
    }
    IEnumerator GetPokemonFromAPI()
    {
        Pokemon pokemonRetrieved = null;
        int random = 0;
        while(pokemonRetrieved == null || excludedPokemonIDs.Contains(random))
        {
            random = Random.Range(1, 1000);
            yield return StartCoroutine(PokeAPIHandler.Instance.GetPokemon(random, result => pokemonRetrieved = result));
        }
        pokemonName.enabled = true;
        pokemonObject.SetPokemonData(pokemonRetrieved);
        pokemonName.text = pokemonRetrieved.name;
        pokemonRenderer.material.mainTexture = pokemonRetrieved.image;
        pokemonRenderer.enabled = true;
    }

    /// <summary>
    /// Called when the pokeball touches a pokemon and defines if it escaped or if the player catched him
    /// </summary>
    public void PokeballImpactedPokemon(PokemonData pokeData)
    {
        pokemonName.enabled = false;
        pokemonRenderer.enabled = false;

        string statusMessage = string.Empty;
        Color textColor = Color.black;

        int escapeProbability = Random.Range(0,101);
        if(escapeProbability < escapeChance)
        {
            //Escaped
            statusMessage = "Pokemon escaped";
            textColor = Color.red;
            AudioManager.instance.Play("PokemonEscaped");
        }
        else
        {
            //Captured
            statusMessage = $"You captured a {pokeData.GetPokemonData().name}!";
            textColor = Color.green;
            excludedPokemonIDs.Add(pokeData.GetPokemonData().id); //If it is caught then exclude him from appearing again
            AudioManager.instance.Play("PokemonCaptured");
        }

        pokemonStatus.text = statusMessage;
        StartCoroutine(ShowTextMessage(4f,textColor));
        
        GetPokemonFromAPICall();
    }

    IEnumerator ShowTextMessage(float time, Color color)
    {
        pokemonStatus.color = color;
        pokemonStatus.CrossFadeAlpha(0.9f, 1.4f, true);

        yield return new WaitForSeconds(time);

        pokemonStatus.CrossFadeAlpha(0, 0.8f, true);
    }
}
