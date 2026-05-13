using UnityEngine;

public partial class FishingController
{
    private bool ValidateSettings()
    {
        // Confirma que os ScriptableObjects estao atribuídos.
        bool valid = true;

        if (settings == null)
        {
            valid = false;
        }

        return valid;
    }
}
