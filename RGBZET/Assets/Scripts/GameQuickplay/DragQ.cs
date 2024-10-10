using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Photon.Pun;

public class DragQ : MonoBehaviourPunCallbacks, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Transform parentToReturnTo = null;
    private Vector3 startPosition;
    private Quaternion startRotation;
    private DisplayCardQ displayCard;
    [SerializeField] private Outline outline;
    [SerializeField] private AudioSource audioSource;

    void Start()
    {
        displayCard = GetComponent<DisplayCardQ>();

        outline = GetComponent<Outline>();
        if (outline != null)
        {
            outline.enabled = false;
        }
        else
        {
            Debug.LogWarning("Outline component not found on this card.");
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource != null)
        {
            audioSource.enabled = false;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (MutimanageQ.isZETActive && MutimanageQ.playerWhoActivatedZET == PhotonNetwork.LocalPlayer)
        {
            Debug.Log("OnBeginDrag: Starting to drag the card");
            parentToReturnTo = this.transform.parent;
            startPosition = this.transform.localPosition;
            startRotation = this.transform.localRotation;

            this.transform.SetParent(this.transform.parent.parent);
            GetComponent<CanvasGroup>().blocksRaycasts = false;

            if (outline != null)
            {
                outline.enabled = true;
                outline.effectColor = Color.yellow;
                outline.effectDistance = new Vector2(7, 7);
                Debug.Log("Outline enabled with yellow color and thicker border.");
            }

            if (audioSource != null)
            {
                audioSource.enabled = true;
                audioSource.Play();
            }

            startPosition = this.transform.localPosition;
            startRotation = this.transform.localRotation;
            photonView.RPC("RPC_OnBeginDrag", RpcTarget.All, startPosition, startRotation);
        }
        else
        {
            eventData.pointerDrag = null;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (MutimanageQ.isZETActive && MutimanageQ.playerWhoActivatedZET == PhotonNetwork.LocalPlayer)
        {
            this.transform.position = eventData.position;

            photonView.RPC("RPC_OnDrag", RpcTarget.AllBuffered, (Vector2)eventData.position);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        this.transform.SetParent(parentToReturnTo);
        this.transform.localPosition = startPosition;
        this.transform.localRotation = startRotation;

        GetComponent<CanvasGroup>().blocksRaycasts = true;

        if (outline != null)
        {
            outline.enabled = false;
        }

        photonView.RPC("RPC_OnEndDrag", RpcTarget.AllBuffered, startPosition, startRotation);
    }

    [PunRPC]
    private void RPC_OnBeginDrag(Vector3 startPosition, Quaternion startRotation)
    {
        this.startPosition = startPosition;
        this.startRotation = startRotation;
    }

    [PunRPC]
    private void RPC_OnDrag(Vector2 position)
    {
        this.transform.position = position;
    }

    [PunRPC]
    private void RPC_OnEndDrag(Vector3 startPosition, Quaternion startRotation)
    {
        this.startPosition = startPosition;
        this.startRotation = startRotation;
        this.transform.localPosition = startPosition;
        this.transform.localRotation = startRotation;
    }
}