using Fusion;
using UnityEngine;

public class ScoreManager : NetworkBehaviour
{
    [SerializeField] NumberField ScoreDisplay;

    [Networked(OnChanged = nameof(NetworkedScoreChanged))]
    public int NetworkedScore { get; set; } = 0;

    private static void NetworkedScoreChanged(Changed<ScoreManager> changed)
    {
        if (changed.Behaviour.ScoreDisplay != null)
        {
            Debug.Log($"Score changed to: {changed.Behaviour.NetworkedScore}");
            changed.Behaviour.ScoreDisplay.SetNumber(changed.Behaviour.NetworkedScore);
        }
        else
        {
            Debug.LogWarning("ScoreDisplay is null in ScoreManager");
        }
    }


    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    // All players can call this function; only the StateAuthority receives the call.
    public void AddScoreRpc(int score)
    {
        // The code inside here will run on the client which owns this object (has state and input authority).
        Debug.Log("Received AddScoreRpc on StateAuthority, modifying Networked variable");
        NetworkedScore += score;
    }
}
