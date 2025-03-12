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
    private const string HostPlayerNameKey_ = "HostPlayerName";
    private const string HostAddressKey_ = "HostAddress";
    private const string FFF7R_LobbyNameKey_ = "FFF7R_LobbyName";
    private const string FFF7RLobbyNamePrefix_ = "FFF7R_";
    protected Callback<LobbyMatchList_t> lobbyMatchListCallback;
    protected Callback<LobbyCreated_t> lobbyCreated;
    protected Callback<LobbyEnter_t> lobbyEntered;
    protected Callback<GameLobbyJoinRequested_t> lobbyJoinRequested;
    private void Start() {
        netRoom = GetComponent<NetRoom>();
        if(!SteamManager.Initialized) {
            debugText.text = "Steam Link Failed.";
            return;
        }
        debugText.text = "Steam Link Success.";
        lobbyMatchListCallback = Callback<LobbyMatchList_t>.Create(GetSteamLobbyList);
        lobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
        lobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
        lobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnLobbyJoinRequested);
        RefreshLobbyList();
    }
    private void GetSteamLobbyList(LobbyMatchList_t callback) {
        foreach (Transform child in lobbyListContent) {
            Destroy(child.gameObject);
        }
        for (int i = 0; i < callback.m_nLobbiesMatching; i++)
        {
            CSteamID lobbyID = SteamMatchmaking.GetLobbyByIndex(i);
            string lobbyHostAddress = SteamMatchmaking.GetLobbyData(lobbyID, HostAddressKey_);
            string lobbyHostName = SteamMatchmaking.GetLobbyData(lobbyID, HostPlayerNameKey_);
            string lobbyName = SteamMatchmaking.GetLobbyData(lobbyID, FFF7R_LobbyNameKey_);
            if(lobbyName.Contains(FFF7RLobbyNamePrefix_)) {
                GameObject lobbyListItemOne = Instantiate(lobbyListItem, lobbyListContent);
                Text itemText = lobbyListItemOne.GetComponentInChildren<Text>();
                if (itemText != null) {
                    itemText.text = lobbyHostName + "的房间: " + lobbyHostAddress;
                }
                UnityEngine.UI.Button joinButton = lobbyListItemOne.GetComponent<UnityEngine.UI.Button>();
                joinButton.onClick.RemoveAllListeners();
                joinButton.onClick.AddListener(() => JoinLobby(lobbyID));
                Debug.Log($"Lobby ID: {lobbyID}, name :{lobbyName}");
            }
        }
    }
    private void OnLobbyCreated(LobbyCreated_t callback) {
        if(callback.m_eResult != EResult.k_EResultOK) {
            debugText.text = "Create Steam Lobby Failed.";
            return;
        }
        debugText.text = "Create Steam Lobby Success.";
        netRoom.StartHost();
        CSteamID lobbyID = new CSteamID(callback.m_ulSteamIDLobby);
        SteamMatchmaking.SetLobbyData(lobbyID, HostAddressKey_, SteamUser.GetSteamID().ToString());
        SteamMatchmaking.SetLobbyData(lobbyID, FFF7R_LobbyNameKey_, FFF7RLobbyNamePrefix_ + SteamUser.GetSteamID().ToString() + "_" + SteamFriends.GetPersonaName());
        SteamMatchmaking.SetLobbyData(lobbyID, HostPlayerNameKey_, SteamFriends.GetPersonaName());
        Debug.Log("Created Lobby: " + SteamMatchmaking.GetLobbyData(lobbyID, FFF7R_LobbyNameKey_) + " HostAddressKey: " + SteamMatchmaking.GetLobbyData(lobbyID, HostAddressKey_));
    }
    private void OnLobbyEntered(LobbyEnter_t callback) {
        if(callback.m_EChatRoomEnterResponse != (uint)EChatRoomEnterResponse.k_EChatRoomEnterResponseSuccess) {
            debugText.text = "Join Steam Lobby Failed.";
            return;
        }
        CSteamID lobbyID = new CSteamID(callback.m_ulSteamIDLobby);
        string hostAddress = SteamMatchmaking.GetLobbyData(lobbyID, HostAddressKey_);
        Debug.Log("Entered Steam Lobby: " + hostAddress);

        netRoom.networkAddress = hostAddress;
        if(!netRoom.isNetworkActive && hostAddress != "") {
            netRoom.StartClient();
            Debug.Log("Entered Mirror Host by Steam Lobby: " + hostAddress);
        }
    }
    private void OnLobbyJoinRequested(GameLobbyJoinRequested_t callback) {
        SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
    }
    public void RefreshLobbyList() {
        SteamMatchmaking.AddRequestLobbyListDistanceFilter(ELobbyDistanceFilter.k_ELobbyDistanceFilterFar);
        SteamMatchmaking.AddRequestLobbyListResultCountFilter(100);
        SteamMatchmaking.AddRequestLobbyListStringFilter(FFF7R_LobbyNameKey_, FFF7RLobbyNamePrefix_, ELobbyComparison.k_ELobbyComparisonGreaterThan);
        SteamMatchmaking.RequestLobbyList();
    }
    public void HostLobby() {
        SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypePublic, netRoom.maxConnections);
    }
    public void JoinLobby(CSteamID lobbyID) {
        SteamMatchmaking.JoinLobby(lobbyID);
    }
}