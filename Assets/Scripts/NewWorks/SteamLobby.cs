using System.Collections;
using System.Collections.Generic;
using Steamworks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class SteamLobby : MonoBehaviour
{
    public Text debugText;
    public Transform lobbyListContent;
    public GameObject lobbyListItem;
    public static SteamLobby instance;
    private void Awake() {
        if(instance == null) {
            instance = this;
            DontDestroyOnLoad(gameObject);
        } else {
            Destroy(gameObject);
        }
    }
    private NetRoom netRoom;
    Dictionary<string, CSteamID> LobbyListMap = new Dictionary<string, CSteamID>{};
    private const string hostAddressKey_ = "HostAddress";
    private const string nameKey_ = "name";
    private const string FFF7RLobbyNameKey_ = "FFF7R_";
    protected Callback<LobbyMatchList_t> lobbyMatchListCallback;
    protected Callback<LobbyDataUpdate_t> lobbyDataUpdateCallback;
    protected Callback<LobbyCreated_t> lobbyCreated;
    protected Callback<GameLobbyJoinRequested_t> lobbyJoinRequested;
    protected Callback<LobbyEnter_t> lobbyEntered;

    private void Start() {
        netRoom = GetComponent<NetRoom>();
        if(!SteamManager.Initialized) {
            debugText.text = "Steam Link Failed.";
            return;
        }
        debugText.text = "Steam Link Success.";
        lobbyMatchListCallback = Callback<LobbyMatchList_t>.Create(GetSteamLobbyList);
        lobbyDataUpdateCallback = Callback<LobbyDataUpdate_t>.Create(OnLobbyDataUpdate);
        lobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
        lobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
        lobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnLobbyJoinRequested);
        SteamMatchmaking.RequestLobbyList();
    }
    private void GetSteamLobbyList(LobbyMatchList_t callback) {
        LobbyListMap.Clear();
        for (int i = 0; i < callback.m_nLobbiesMatching; i++)
        {
            CSteamID lobbyID = SteamMatchmaking.GetLobbyByIndex(i);
            string lobbyName = SteamMatchmaking.GetLobbyData(lobbyID, nameKey_);
            LobbyListMap.Add(lobbyName + i.ToString(), lobbyID);
            if(lobbyName.Contains(FFF7RLobbyNameKey_) && !LobbyListMap.ContainsKey(lobbyName + i.ToString())) {
                LobbyListMap.Add(lobbyName + i.ToString(), lobbyID);
            }
            Debug.Log($"Lobby ID: {lobbyID}, name :{lobbyName}");
            SteamMatchmaking.RequestLobbyData(lobbyID);
        }
    }
    private void OnLobbyDataUpdate(LobbyDataUpdate_t callback)
    {
        // if (callback.m_bSuccess == 1)
        // {
        //     CSteamID lobbyID = new CSteamID(callback.m_ulSteamIDLobby);
        //     string lobbyName = SteamMatchmaking.GetLobbyData(lobbyID, nameKey_);
        //     if(lobbyName.Contains(FFF7RLobbyNameKey_)) {
        //         LobbyListMap.Add(lobbyName, lobbyID);
        //     }
        //     Debug.Log($"Lobby Name: {lobbyName}");
        // }
    }
    private void OnLobbyJoinRequested(GameLobbyJoinRequested_t callback) {
        debugText.text = "Get Lobby Join Request.";
        SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
        
    }
    private void OnLobbyEntered(LobbyEnter_t callback) {
        debugText.text = "Other Player Joined";
        string hostAddressKey = SteamMatchmaking.GetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), hostAddressKey_);
        netRoom.networkAddress = hostAddressKey;

        if(!netRoom.isNetworkActive) {
            netRoom.StartClient();
            debugText.text = "Joining the Host";
        }
    }
    public void HostLobby() {
        SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypePublic, netRoom.maxConnections);
    }
    private void OnLobbyCreated(LobbyCreated_t callback) {
        if(callback.m_eResult != EResult.k_EResultOK) {
            debugText.text = "Create Steam Lobby Failed.";
            return;
        }
        debugText.text = "Create Steam Lobby Success.";
        netRoom.StartHost();
        CSteamID lobbyID = new CSteamID(callback.m_ulSteamIDLobby);
        SteamMatchmaking.SetLobbyData(lobbyID, hostAddressKey_, SteamUser.GetSteamID().ToString());
        SteamMatchmaking.SetLobbyData(lobbyID, nameKey_, "FFF7R_" + SteamUser.GetSteamID().ToString());
    }
    public void RefreshLobbyList() {
        SteamMatchmaking.RequestLobbyList();
        foreach (Transform child in lobbyListContent) {
            Destroy(child.gameObject);
        }
        foreach (var lobbyMapItem in LobbyListMap){
            GameObject lobbyListItemOne = Instantiate(lobbyListItem, lobbyListContent);
            // lobbyListItem.transform.SetParent(lobbyListContent, false);
            Text itemText = lobbyListItemOne.GetComponentInChildren<Text>();
            if (itemText != null) {
                itemText.text = lobbyMapItem.Key;
            }
        }
    }
}
