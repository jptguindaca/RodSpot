using UnityEngine;

public partial class FishingController
{
    private void OnBobberTouched(Collider other)
    {
        // Decide se a boia tocou na agua ou numa zona invalida.
        if (bobberLandedOnWater)
            return;

        bool hitWater = (settings.waterMask.value & (1 << other.gameObject.layer)) != 0;

        if (hitWater)
        {
            LandBobber();
        }
        else
        {
            notifications?.ShowInvalidWater();
            ResetFishing();
        }
    }

    private void LandBobber()
    {
        // Trava a boia no local e desativa a fisica.
        bobberLandedOnWater = true;

        if (currentBobberRigidbody != null)
        {
            if (!currentBobberRigidbody.isKinematic)
            {
                currentBobberRigidbody.linearVelocity = Vector3.zero;
                currentBobberRigidbody.angularVelocity = Vector3.zero;
            }
            currentBobberRigidbody.useGravity = false;
            currentBobberRigidbody.isKinematic = true;
        }
    }

    // Encaminha colisoes da boia para o controlador.
    private class BobberContact : MonoBehaviour
    {
        private FishingController controller;
        private bool hasTouched;

        public void Initialize(FishingController controller)
        {
            this.controller = controller;
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (hasTouched)
                return;

            hasTouched = true;
            controller.OnBobberTouched(collision.collider);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (hasTouched)
                return;

            hasTouched = true;
            controller.OnBobberTouched(other);
        }
    }
}
