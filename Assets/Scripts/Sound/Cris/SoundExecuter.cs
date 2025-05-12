using UnityEngine;
using ProjectColombo.GameManagement.Events;
using ProjectColombo.Combat;
using ProjectColombo;

public class PlayerComboSFXListener : MonoBehaviour
{
    public PlayerSFXManager sfxManager;

    private void OnEnable()
    {
        CustomEvents.OnDamageDelt += HandleDamageDelt;
    }

    private void OnDisable()
    {
        CustomEvents.OnDamageDelt -= HandleDamageDelt;
    }

    private void HandleDamageDelt(int damage, GameGlobals.MusicScale scale, HealthManager enemy, int comboLength)
    {
        string note = scale.ToString(); // Assuming scale is an enum like A4, C5, etc.

        switch (comboLength)
        {
            case 1:
                sfxManager.PlayComboFirstAttack();
                break;

            case 2:
                sfxManager.PlayComboSecondAttack(note);
                break;

            case 3:
                bool isMajor = note.Contains("Major"); // Or use a more precise method depending on how you handle this
                sfxManager.PlayComboThirdAttack(isMajor);
                break;

            default:
                Debug.LogWarning($"Unhandled combo length: {comboLength}");
                break;
        }
    }
}
