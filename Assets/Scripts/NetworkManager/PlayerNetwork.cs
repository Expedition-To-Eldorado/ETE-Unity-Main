using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerNetwork : MonoBehaviour
{
    [SerializeField] private List<Material> PawnMaterials;
    private Transform Player;
    private Renderer[] childRenderers;

    // Start is called before the first frame update
    private void Awake()
    {
        Player = this.transform;
        childRenderers = Player.GetComponentsInChildren<Renderer>();
        childRenderers[0].sharedMaterial = PawnMaterials[1];
        childRenderers[0].transform.position = new Vector3(32f, 0.55f, -4.3f);
    }
}
